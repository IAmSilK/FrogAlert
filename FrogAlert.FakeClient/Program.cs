var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5003/");

var environmentSnapshots = new List<(int Delay, float Humidity, float TempC)>
{
    (5, 70.2f, 22.2f),
    (0, 73.2f, 22.6f),
    (0, 77.2f, 23.6f),
    (0, 79.2f, 23.9f),
    (20, 78.1f, 24.2f),
    (20, 78.1f, 24.5f),
    (20, 78.1f, 23.2f)
};

foreach (var (delay, humidity, tempC) in environmentSnapshots)
{
    await Task.Delay(delay * 1000);
    
    var parameters = new Dictionary<string, string>()
    {
        { "tempC", tempC.ToString("#.#") },
        { "humidity", humidity.ToString("#.#") },
        { "time", DateTimeOffset.UtcNow.AddSeconds(-15).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
    };

    var urlParameters = string.Join("&", parameters.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"));

    Console.WriteLine($"Posting data. Humidity: {humidity:#.#}%, Temperature: {tempC:#.#} C");

    var response = await client.PostAsync($"Environment?{urlParameters}", null);

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Post succeeded. Status code: {response.StatusCode}");
    }
    else
    {
        Console.WriteLine($"Post failed. Status code: {response.StatusCode}");
    }
}