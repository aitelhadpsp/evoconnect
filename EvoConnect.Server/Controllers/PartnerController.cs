using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EvoConnect.Server.Controllers
{
    public class PartnerController : Controller
    {
        [HttpGet("WifiApp")]
        public async Task<IActionResult> Index()
        {
            DbContext db = new();
            return Ok(new {
                Status=true,
                Access = db.IsFlashEvoActive(),
            });
        }

    }
}