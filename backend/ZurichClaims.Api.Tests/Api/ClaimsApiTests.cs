using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using ZurichClaims.Api.Tests.Helpers;

namespace ZurichClaims.Api.Tests.Api;

public class ClaimsApiTests
{
    private static HttpClient CreateClientWithUser(CustomWebApplicationFactory factory, string? userId = "1001")
    {
        var client = factory.CreateClient();
        if (userId != null)
        {
            client.DefaultRequestHeaders.Add("x-usuario", userId);
        }
        return client;
    }

    private static StringContent CreateJsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");

    [Theory]
    [InlineData("GET", "/api/claims")]
    [InlineData("POST", "/api/claims")]
    [InlineData("GET", "/api/claims/1")]
    [InlineData("PUT", "/api/claims/1")]
    [InlineData("POST", "/api/claims/1/status")]
    [InlineData("DELETE", "/api/claims/1")]
    public async Task Endpoints_SinHeaderXUsuario_Retornan400BadRequest(string method, string url)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: null); // Sin header

        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
        {
            request.Content = CreateJsonContent("{}");
        }

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123456789012345678901")] // 21 dígitos
    [InlineData("   ")]
    public async Task Endpoints_ConXUsuarioInvalido_Retornan400BadRequest(string invalidUser)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: invalidUser);

        // Act
        using var response = await client.GetAsync("/api/claims");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_BodyInvalidoYSinHeaderXUsuario_Retorna400ConAmbosErrores()
    {
        // Arrange: body con monto omitido Y sin x-usuario
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: null);

        var json = """
        {
            "policyNumber": "POL-2024-999",
            "insuredName": "Juan Pérez",
            "claimType": "Robo parcial"
        }
        """;

        // Act
        using var response = await client.PostAsync("/api/claims", CreateJsonContent(json));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("errors", out var errors));

        // Debe contener error por el header x-usuario (o userId)
        var tieneErrorHeader = errors.TryGetProperty("x-usuario", out _) ||
                               errors.TryGetProperty("userId", out _);
        Assert.True(tieneErrorHeader, $"Se esperaba error de x-usuario/userId en errors: {body}");

        // Debe contener error por el monto omitido (EstimatedAmount)
        var tieneErrorMonto = errors.TryGetProperty("EstimatedAmount", out _) ||
                              errors.TryGetProperty("estimatedAmount", out _);
        Assert.True(tieneErrorMonto, $"Se esperaba error de EstimatedAmount en errors: {body}");
    }

    [Fact]
    public async Task Post_Valido_Retorna201ConLocationEstadoOpenYCreatedAtTerminaEnZ()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "1001");

        var json = """
        {
            "policyNumber": "POL-2024-NEW",
            "insuredName": "Pedro Picapiedra",
            "claimType": "Colisión",
            "estimatedAmount": 1234.56
        }
        """;

        // Act
        using var response = await client.PostAsync("/api/claims", CreateJsonContent(json));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal("1001", root.GetProperty("createdBy").GetString());
        Assert.Equal("Open", root.GetProperty("currentStatus").GetString());

        var createdAtString = root.GetProperty("createdAt").GetString();
        Assert.NotNull(createdAtString);
        Assert.EndsWith("Z", createdAtString);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("10.999")]
    [InlineData("1000000000000")]
    [InlineData("79228162514264337593543950335")]
    public async Task Post_MontosInvalidos_Retorna400SiempreNunca500(string montoLiteral)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "1001");

        var json = $$"""
        {
            "policyNumber": "POL-2024-MONTO",
            "insuredName": "Asegurado Test",
            "claimType": "Colisión",
            "estimatedAmount": {{montoLiteral}}
        }
        """;

        // Act
        using var response = await client.PostAsync("/api/claims", CreateJsonContent(json));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_Invalido_Retorna400ConTypeInvalidStatusTransition_EIdInexistenteRetorna404()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "1001");

        // Caso 1: Siniestro 1 está en Open. Transición a Closed es inválida
        var jsonInvalido = """
        {
            "newStatus": "Closed",
            "reason": "Intento de salto directo"
        }
        """;

        using var respInvalido = await client.PostAsync("/api/claims/1/status", CreateJsonContent(jsonInvalido));
        Assert.Equal(HttpStatusCode.BadRequest, respInvalido.StatusCode);

        var bodyInvalido = await respInvalido.Content.ReadAsStringAsync();
        using var docInvalido = JsonDocument.Parse(bodyInvalido);
        var type = docInvalido.RootElement.GetProperty("type").GetString();
        Assert.NotNull(type);
        Assert.EndsWith("invalid-status-transition", type);

        // Caso 2: Id inexistente (999) retorna 404 ProblemDetails
        var jsonValido = """
        {
            "newStatus": "InReview"
        }
        """;
        using var resp404 = await client.PostAsync("/api/claims/999/status", CreateJsonContent(jsonValido));
        Assert.Equal(HttpStatusCode.NotFound, resp404.StatusCode);

        var body404 = await resp404.Content.ReadAsStringAsync();
        using var doc404 = JsonDocument.Parse(body404);
        Assert.Equal(404, doc404.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task ChangeStatus_NewStatusNumerosOStringsInvalidos_Retorna400()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "1001");

        // newStatus 99 -> 400
        using var resp99 = await client.PostAsync("/api/claims/1/status", CreateJsonContent("""{"newStatus":99}"""));
        Assert.Equal(HttpStatusCode.BadRequest, resp99.StatusCode);

        // newStatus "Foo" -> 400
        using var respFoo = await client.PostAsync("/api/claims/1/status", CreateJsonContent("""{"newStatus":"Foo"}"""));
        Assert.Equal(HttpStatusCode.BadRequest, respFoo.StatusCode);

        // newStatus "1" (número como texto) -> 400 rechazado por StrictJsonStringEnumConverter
        using var resp1 = await client.PostAsync("/api/claims/1/status", CreateJsonContent("""{"newStatus":"1"}"""));
        Assert.Equal(HttpStatusCode.BadRequest, resp1.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_Valido_Retorna200YUltimoHistorialTieneChangedByIgualAXUsuario()
    {
        // Arrange: Siniestro 1 (Open) -> InReview
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "8888");

        var json = """
        {
            "newStatus": "InReview",
            "reason": "Dictamen inicial aprobado"
        }
        """;

        // Act
        using var response = await client.PostAsync("/api/claims/1/status", CreateJsonContent(json));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal("InReview", root.GetProperty("currentStatus").GetString());

        var historyArray = root.GetProperty("history");
        var count = historyArray.GetArrayLength();
        Assert.True(count >= 2);

        var ultimoHistorial = historyArray[count - 1];
        Assert.Equal("8888", ultimoHistorial.GetProperty("changedBy").GetString());
        Assert.Equal("InReview", ultimoHistorial.GetProperty("newStatus").GetString());
        Assert.Equal("Dictamen inicial aprobado", ultimoHistorial.GetProperty("changeReason").GetString());
    }

    [Fact]
    public async Task Delete_CicloCompleto_Retorna204LuegoGetRetorna404YSegundoDeleteRetorna404()
    {
        // Arrange: Usamos siniestro 2 del seed
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "9999");

        // 1. DELETE -> 204
        using var respDelete = await client.DeleteAsync("/api/claims/2");
        Assert.Equal(HttpStatusCode.NoContent, respDelete.StatusCode);

        // 2. GET -> 404
        using var respGet = await client.GetAsync("/api/claims/2");
        Assert.Equal(HttpStatusCode.NotFound, respGet.StatusCode);

        // 3. Segundo DELETE -> 404
        using var respDelete2 = await client.DeleteAsync("/api/claims/2");
        Assert.Equal(HttpStatusCode.NotFound, respDelete2.StatusCode);
    }

    [Fact]
    public async Task GetAll_FiltroPorPoliza_ComodinesYTexto()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "1001");

        // ?policyNumber=%25 (escapado % codificado en URL) -> lista vacía
        using var respComodin = await client.GetAsync("/api/claims?policyNumber=%25");
        Assert.Equal(HttpStatusCode.OK, respComodin.StatusCode);
        var bodyComodin = await respComodin.Content.ReadAsStringAsync();
        using var docComodin = JsonDocument.Parse(bodyComodin);
        Assert.Equal(0, docComodin.RootElement.GetArrayLength());

        // ?policyNumber=pol-2024 -> los 3 del seed
        using var respPol = await client.GetAsync("/api/claims?policyNumber=pol-2024");
        Assert.Equal(HttpStatusCode.OK, respPol.StatusCode);
        var bodyPol = await respPol.Content.ReadAsStringAsync();
        using var docPol = JsonDocument.Parse(bodyPol);
        Assert.Equal(3, docPol.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetById_ElEstadoEnLaRespuestaVieneComoTextoNoComoNumero()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = CreateClientWithUser(factory, userId: "1001");

        // Act
        using var response = await client.GetAsync("/api/claims/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rawJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(rawJson);
        var currentStatusProp = doc.RootElement.GetProperty("currentStatus");

        Assert.Equal(JsonValueKind.String, currentStatusProp.ValueKind);
        Assert.Equal("Open", currentStatusProp.GetString());
    }
}
