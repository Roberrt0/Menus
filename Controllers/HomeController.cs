using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Menus.Models;
using menus.Models;
using System.Text.Json;
using System.Net.Http;
using menus.Services;
using AspNetCoreGeneratedDocument;

namespace Menus.Controllers;

public class HomeController : Controller
{
    // LOGIN
    [HttpGet]
    public ActionResult Login()
    {
        var isAuthenticated = HttpContext.Session.GetString("UserAuthenticated");
        if (isAuthenticated == "true")
        {
            return RedirectToAction("Welcome");
        }
        return View();
    }
    [HttpPost]
    public ActionResult Login(User user)
    {
        if (authService.Authenticate(user.Username, user.Password))
        {
            HttpContext.Session.SetString("UserAuthenticated", "true");
            return RedirectToAction("Welcome");
        }
        else
        {
            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }
    }

    // autenticacion exitosa
    public IActionResult Welcome()
    {
        var isAuthenticated = HttpContext.Session.GetString("UserAuthenticated");
        if (isAuthenticated != "true")
        {
            return RedirectToAction("Index");
        }
        return View();
    }

    // no se ha auntenticado
    public IActionResult UnauthorizedAccess()
    {
        return View();
    }

    // LOGOUT
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }


    // JOKE API
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task<IActionResult> GetJoke()
    {
        var isAuthenticated = HttpContext.Session.GetString("UserAuthenticated");
        if (isAuthenticated != "true")
        {
            return RedirectToAction("UnauthorizedAccess");
        }

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://v2.jokeapi.dev/joke/Any");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var joke = JsonSerializer.Deserialize<JokeResponse>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return View(joke);
        }

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
    private readonly IAuthService authService;

    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger, HttpClient httpClient)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        this.httpClient = httpClient;
        this.authService = new SimpleAuthService();
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
