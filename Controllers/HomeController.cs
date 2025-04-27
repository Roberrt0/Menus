using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Menus.Models;
using menus.Models;
using System.Text.Json;
using System.Net.Http;

namespace Menus.Controllers;

public class HomeController : Controller
{
    // JOKE API
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task<IActionResult> GetJoke()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://v2.jokeapi.dev/joke/Any");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var joke = JsonSerializer.Deserialize<JokeResponse>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            System.Console.WriteLine("Error: " + response.StatusCode);
            return View(joke);
        }

        System.Console.WriteLine("Error: " + response.StatusCode);
        return NotFound();
    }

    // POKEMON API
    [HttpGet("Home/GetPokemon/{name}")]
    public async Task<IActionResult> GetPokemon(string name)
    {
        String url = $"https://pokeapi.co/api/v2/pokemon/{name}";
        HttpResponseMessage response =
             await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("Error: " + response.StatusCode);
            return NotFound();
        }
        String json = await response
                           .Content
                           .ReadAsStringAsync();
        JsonDocument doc = JsonDocument
                          .Parse(json);
        Pokemon pokemon = new Pokemon
        {
            Name = doc.RootElement.GetProperty("name").GetString(),
            ImageUrl = doc
                   .RootElement
                   .GetProperty("sprites")
                   .GetProperty("front_default")
                   .GetString(),
            Types = [.. doc.RootElement.GetProperty("types")
           .EnumerateArray()
           .Select(t => t.GetProperty("type")
                       .GetProperty("name")
                       .GetString())],
            Abilities = [.. doc.RootElement.GetProperty("abilities")
                .EnumerateArray()
                .Select(a => a.GetProperty("ability")
                              .GetProperty("name")
                              .GetString())],
            Weight = doc.RootElement.GetProperty("weight").GetInt32(),
        };

        return View(pokemon);
    }

    private readonly HttpClient httpClient;

    private readonly ILogger<HomeController> _logger;

    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger, HttpClient httpClient)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        this.httpClient = httpClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Photos()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
