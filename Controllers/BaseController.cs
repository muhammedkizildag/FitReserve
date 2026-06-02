using Microsoft.AspNetCore.Mvc;

namespace FitReserve.Controllers;

public abstract class BaseController : Controller
{
    protected void BasariMesaji(string mesaj)
    {
        TempData["BasariMesaji"] = mesaj;
    }

    protected void HataMesaji(string mesaj)
    {
        TempData["HataMesaji"] = mesaj;
    }
}
