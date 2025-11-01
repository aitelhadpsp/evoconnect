using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Common.Models;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IImagesDA
    {
        public Task<List<ImageLabel>> GetLabels();
    }
}