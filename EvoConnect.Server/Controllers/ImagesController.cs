using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvoConnect.Server.Controllers
{
	public class ImagesController(IImagesDA _imagesDA) : Controller
	{
		[HttpGet("images/labels")]
		public async Task<IActionResult> GetLabels()
		{
			var data = await _imagesDA.GetLabels();
			return Ok(data);
		}

	}
}