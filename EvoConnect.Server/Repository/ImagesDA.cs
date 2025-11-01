using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EvoConnect.Common.Models;
using EvoConnect.Server.Repository.Interfaces;
using EvoConnect.Server.Data;
using EvoConnect.Server.Models;

namespace EvoConnect.Server.Repository
{
	public class ImagesDA : IImagesDA
	{
		private readonly ClinicDbContext _context;

		public ImagesDA(ClinicDbContext context)
		{
			_context = context;
		}

		public async Task<List<ImageLabel>> GetLabels()
		{
			try
			{
				var labels = await _context.Etiquettes
					.Where(e => e.Nom != null) // Éviter les noms null
					.Select(e => new ImageLabel
					{
						InternId = e.PkEtiquette,
						Label = e.Nom.Trim()
					})
					.OrderBy(l => l.Label) 
					.ToListAsync();

				return labels;
			}
			catch (Exception)
			{
				return new List<ImageLabel>();
			}
		}
	}


}