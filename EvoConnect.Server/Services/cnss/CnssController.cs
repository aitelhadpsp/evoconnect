using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace EvoConnect.Server.Services.cnss
{
    public class CnssController()
    {
        HttpClient _httpClient = new();

        private readonly string _baseUrl = "http://196.32.217.77";

        private static HttpRequestMessage CreateRequest(
            HttpMethod method,
            string endpoint,
            HttpContent? content = null,
            string? AccessToken = null
        )
        {
            var request = new HttpRequestMessage(method, endpoint);

            if (content != null)
                request.Content = content;

            if (!string.IsNullOrEmpty(AccessToken))
                request.Headers.Add("AccessToken", AccessToken);

            return request;
        }

        public async Task<AuthResponse> Authenticate(string inpe, string motDePasse)
        {
            var endpoint = $"{_baseUrl}/authenticate";
            var content = new StringContent(
                JsonSerializer.Serialize(new { inpe, motDePasse }),
                Encoding.UTF8,
                "application/json"
            );

            var httpRequest = CreateRequest(HttpMethod.Post, endpoint, content);
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<AuthResponse>(responseContent)!;

            if (string.IsNullOrWhiteSpace(res.token))
            {
                throw new Exception();
            }

            return res;
        }

        public async Task<BeneficiaryResponse> GetBeneficiaryInfoAsync(BeneficiaryRequest request)
        {
            var endpoint = $"{_baseUrl}api/beneficiary";
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var httpRequest = CreateRequest(HttpMethod.Post, endpoint, content);
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<BeneficiaryResponse>(responseContent)!;
            return res;
        }

        public class BeneficiaryRequest
        {
            public required string NumeroImmatriculation { get; set; }
            public required string CNIE_CS { get; set; }
            public DateTime? Date_naissance { get; set; }
        }

        public class BeneficiaryResponse
        {
            public required string CodeRetour { get; set; }
            public required List<Beneficiary> ListeBeneficiaires { get; set; }
        }

        public class Beneficiary
        {
            public required string numeroImmatriculation { get; set; }
            public required string Nom { get; set; }
            public required string Prenom { get; set; }
            public DateTime dateNaissance { get; set; }
            public char Genre { get; set; }
            public int NumIndividu { get; set; }
            public required string LienParente { get; set; }
        }

        public async Task<DeclarationResponseDto> VerifyFseAsync(PrescriptionInitiationDto request)
        {
            var endpoint = $"{_baseUrl}fse/verifierFse";
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var httpRequest = CreateRequest(HttpMethod.Post, endpoint, content);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DeclarationResponseDto>(responseContent)!;
        }

        public async Task<DeclarationResponseDto> DeclareFseAsync(
            object request,
            string AccessToken
        )
        {
            var endpoint = $"{_baseUrl}/creerFse";
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var httpRequest = CreateRequest(HttpMethod.Post, endpoint, content, AccessToken);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<DeclarationResponseDto>(responseContent)!;
        }

        public async Task<string> SendFilesAsync(
            DocumentUploadEditeurRequestDto req,
            string AccessToken
        )
        {



            var endpoint = $"{_baseUrl}/demandeComplement/files";


            var payload = new
            {
                req.IdentifiantTechnique,
                req.NumeroFse,
                Files = req.Files,
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            if (!string.IsNullOrEmpty(AccessToken))
                request.Headers.Add("AccessToken", AccessToken);

            var result = await _httpClient.SendAsync(request);
            result.EnsureSuccessStatusCode();

            var responseContent = await result.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
}