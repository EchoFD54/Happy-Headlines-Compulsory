using System.Net.Http.Json;

public class ProfanityServiceClient{
    private readonly HttpClient _httpClient;

    public ProfanityServiceClient(HttpClient httpClient){
        _httpClient = httpClient;
    }

    public async Task<bool> ContainsProfanity(string text){
        var response = await _httpClient.PostAsJsonAsync("/api/profanity/check", text);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProfanityResponse>();

        return result!.ContainsProfanity;
    }
}

public class ProfanityResponse{
    public bool ContainsProfanity { get; set; }
}