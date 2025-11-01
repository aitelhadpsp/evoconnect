using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Common.Models;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IPartnerDA
    {
        public Task<Common.Models.PatientDto> GetPatient(int id);
        public Task<List<Common.Models.PatientDto>> GetPatients(string? search);
        public Task<List<ImageDocument>> GetPatientImages(int id,string? labels);
        public Task<ImageDocument> GetImage(int id);
        public Task UpdateImagePath(int id,string path ,string ext);
        public Task<ImageDocument?> GetPatientImagesWithLabels(int id, string labels);
        public Task<ImageDocument> AddImage(int id,string name,string ext);
        public Task<bool> TogglePatientImageLabel(int id, int labelId);
        public Task DeleteImage(int id);
        
        

    }
}