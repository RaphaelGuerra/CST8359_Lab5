using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lab5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lab5.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }


    }
}