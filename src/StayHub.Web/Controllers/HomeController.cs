using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Interfaces;
using StayHub.Web.Models;

namespace StayHub.Web.Controllers;

/// <summary>
/// Serves the public landing page and the shared error view.
/// </summary>
public class HomeController : Controller
{
    private readonly IPropertyRepository _properties;

    /// <summary>
    /// Initialises a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="properties">The listing repository.</param>
    public HomeController(IPropertyRepository properties)
    {
        _properties = properties;
    }

    /// <summary>
    /// Renders the hero banner and a strip of featured active listings.
    /// </summary>
    /// <returns>The landing page.</returns>
    public async Task<IActionResult> Index()
    {
        var featured = (await _properties.GetActiveAsync()).Take(6);
        return View(featured);
    }

    /// <summary>
    /// Renders the shared error page.
    /// </summary>
    /// <returns>The error page.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
