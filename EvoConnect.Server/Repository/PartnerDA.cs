using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EvoConnect.Server.Data;
using EvoConnect.Server.Models;
using EvoConnect.Server.Repository.Interfaces;
using EvoConnect.Common.Models;
using EvoConnect.Common;

namespace EvoConnect.Server.Repository
{
    public class PartnerDA(ClinicDbContext context) : IPartnerDA
    {
        private readonly ClinicDbContext _context = context;

        public async Task<PatientDto?> GetPatient(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.Personne)
                    .Where(p => p.IdPersonne == id)
                    .FirstOrDefaultAsync();

                if (patient == null)
                    return null;

                // Then, get the portrait image separately
                var portraitImage = await _context.Objets
                    .Where(o => o.IdPatient == patient.IdPersonne)
                    .Where(o => o.Etiquettes.Any(e => e.Nom == "Portrait profil"))
                    .Select(o => new { o.Nom, o.Extension })
                    .FirstOrDefaultAsync();

                string? imagePath = null;
                if (portraitImage != null)
                {
                    var fileName = $"{portraitImage.Nom?.Trim()}.{portraitImage.Extension?.Trim()}";
                    imagePath = Path.Combine(patient.IdPersonne.ToString(), fileName);
                }

                return new PatientDto
                {
                    InternId = patient.Personne.IdPersonne,
                    LastName = patient.Personne.PerNom?.Trim() ?? "",
                    FirstName = patient.Personne.PerPrenom?.Trim() ?? "",
                    PhoneNumber = patient.Personne.PerTelPrinc?.Trim() ?? "",
                    Gender = patient.Personne.PerGenre?.Trim() ?? "",
                    BirthDate = patient.Personne.PerDatNaiss,
                    CaseId = patient.PatNumDossier,
                    Image = imagePath
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<ImageDocument>> GetPatientImages(int id, string? labels)
        {
            var imgDir = AppData.ImageDir();
            try
            {
                var query = _context.Objets
                    .Where(o => o.IdPatient == id);

                if (!string.IsNullOrEmpty(labels))
                {
                    var labelList = labels.Split(',').Select(l => int.Parse(l.Trim())).ToList();
                    query = query.Where(o => o.Etiquettes.Any(e => labelList.Contains(e.PkEtiquette)));
                }

                var objets = await query
                    .Select(o => new
                    {
                        o.PkObjet,
                        o.IdPatient,
                        o.Nom,
                        o.Extension,
                        o.DateInsertion
                    })
                    .ToListAsync();

                return [.. objets.Select(o => new ImageDocument
                {
                    Id = o.PkObjet,
                    PatientId = o.IdPatient,
                    Path = Path.Combine(o.IdPatient.ToString(), $"{o.Nom?.Trim()}.{o.Extension?.Trim()}"),
                    Date = o.DateInsertion
            }).Where(img => {
            var path = Path.Combine(imgDir, img.Path);
            return File.Exists(path);

            })];
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<ImageDocument?> GetPatientImagesWithLabels(int id, List<int> labels)
        {
            try
            {
                var labelList = labels.Select(l => l).ToList();
                var labelCount = labelList.Count;

                // Trouver les objets qui ont TOUTES les étiquettes spécifiées
                var objet = await _context.Objets
                    .Include(o => o.ObjetEtiquettes)
                    .Where(o => o.IdPatient == id)
                    .Where(o => o.ObjetEtiquettes
                        .Count(oe => labelList.Contains(oe.Etiquette.PkEtiquette)) == labelCount)
                    .Select(o => new
                    {
                        o.PkObjet,
                        o.IdPatient,
                        o.Nom,
                        o.Extension,
                        o.DateInsertion,
                        ObjetEtiquettes = o.ObjetEtiquettes.Select(oe => oe.IdEtiquette)
                    })
                    .FirstOrDefaultAsync();

                if (objet == null)
                    return null;

                return new ImageDocument
                {
                    Id = objet.PkObjet,
                    PatientId = objet.IdPatient,
                    Path = Path.Combine(objet.IdPatient.ToString(), $"{objet.Nom?.Trim()}.{objet.Extension?.Trim()}"),
                    Date = objet.DateInsertion,
                    Labels = [.. objet.ObjetEtiquettes]
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> TogglePatientImageLabel(int id, int  labelId)
        {
            try
            {
                var label = await _context.Etiquettes
                    .Where(e => e.PkEtiquette == labelId)
                    .FirstOrDefaultAsync();
                if (label == null)
                    return false;

                var exist = await _context.ObjetEtiquettes
                    .Where(oe => oe.IdObjet == id && oe.Etiquette.PkEtiquette == label.PkEtiquette)
                    .FirstOrDefaultAsync();
                if (exist != null)
                {
                    _context.ObjetEtiquettes.Remove(exist);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var newLink = new ObjetEtiquette
                    {
                        IdObjet = id,
                        IdEtiquette = label.PkEtiquette
                    };
                    _context.ObjetEtiquettes.Add(newLink);
                    await _context.SaveChangesAsync();
                }
                return true;
    
            }
            catch (Exception)
            {
                return false;
            }
        }



        public async Task<ImageDocument> GetImage(int id)
        {
            try
            {
                var objet = await _context.Objets
                    .Where(o => o.PkObjet == id)
                    .Select(o => new
                    {
                        o.PkObjet,
                        o.IdPatient,
                        o.Nom,
                        o.Extension,
                        o.DateInsertion,
                        ObjetEtiquettes = o.ObjetEtiquettes.Select(oe => oe.IdEtiquette)

                    })
                    .FirstAsync();

                return new ImageDocument
                {
                    Id = objet.PkObjet,
                    PatientId = objet.IdPatient,
                    Path = Path.Combine(objet.IdPatient.ToString(), $"{objet.Nom?.Trim()}.{objet.Extension?.Trim()}"),
                    Name = objet.Nom?.Trim(),
                    Date = objet.DateInsertion,
                    Labels = [.. objet.ObjetEtiquettes]
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task DeleteImage(int id)
        {
            
                var theobject = await _context.Objets
                    .Include(o => o.ObjetEtiquettes)
                    .Where(o => o.PkObjet == id)
                    .FirstAsync();
                    if (theobject == null)
                        return;

                _context.Objets.Remove(theobject);;
                await _context.SaveChangesAsync();
          
        }
        public async Task<ImageDocument> AddImage(int id, string name, string ext)
        {
            try
            {
                var lastId = await _context.Objets.MaxAsync(o => (int?)o.PkObjet) ?? 0;
                var objet = new FileRecord
                {
                    PkObjet = lastId + 1,
                    IdPatient = id,
                    Nom = name,
                    Extension = ext,
                    DateInsertion = DateTime.Now
                };

                await _context.Objets.AddAsync(objet);
                await _context.SaveChangesAsync();

                return new ImageDocument
                {
                    Id = objet.PkObjet,
                    PatientId = objet.IdPatient,
                    Path = Path.Combine(objet.IdPatient.ToString(), $"{objet.Nom?.Trim()}.{objet.Extension?.Trim()}"),
                    Name = objet.Nom?.Trim(),
                    Date = objet.DateInsertion
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PatientDto>> GetPatients(string? search)
        {
            try
            {
                var query = _context.Patients
                    .Include(p => p.Personne)
                    .Where(p => p.Personne.IdPersonne > 0);

                // Appliquer le filtre de recherche si fourni
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(p =>
                        (p.Personne.PerNom != null && p.Personne.PerNom.ToLower().Contains(searchLower)) ||
                        (p.Personne.PerPrenom != null && p.Personne.PerPrenom.ToLower().Contains(searchLower)) ||
                        (p.Personne.PerNom != null && p.Personne.PerPrenom != null &&
                         (p.Personne.PerNom + " " + p.Personne.PerPrenom).ToLower().Contains(searchLower)));
                }

                var patients = await query
                    .Take(30)
                    .Select(p => new PatientDto
                    {
                        InternId = p.Personne.IdPersonne,
                        LastName = p.Personne.PerNom != null ? p.Personne.PerNom.Trim() : "",
                        FirstName = p.Personne.PerPrenom != null ? p.Personne.PerPrenom.Trim() : "",
                        PhoneNumber = p.Personne.PerTelPrinc != null ? p.Personne.PerTelPrinc.Trim() : "",
                        Gender = p.Personne.PerGenre != null ? p.Personne.PerGenre.Trim() : "",
                        BirthDate = p.Personne.PerDatNaiss,
                        CaseId = p.PatNumDossier
                    })
                    .ToListAsync();

                return patients;
            }
            catch (Exception)
            {
                return new List<PatientDto>();
            }
        }

        public async Task UpdateImagePath(int id, string name, string ext)
        {
            try
            {
                var objet = await _context.Objets
                    .FirstOrDefaultAsync(o => o.PkObjet == id);

                if (objet == null)
                    return;

                objet.Nom = name;
                objet.Extension = ext;

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Additional Helper Methods

        /// <summary>
        /// Get all images for a patient with their labels
        /// </summary>
        public async Task<List<ImageDocumentWithLabels>> GetPatientImagesWithAllLabels(int patientId)
        {
            try
            {
                var objets = await _context.Objets
                    .Include(o => o.Etiquettes)
                    .Where(o => o.IdPatient == patientId)
                    .ToListAsync();

                return objets.Select(o => new ImageDocumentWithLabels
                {
                    Id = o.PkObjet,
                    PatientId = o.IdPatient,
                    Path = Path.Combine(o.IdPatient.ToString(), $"{o.Nom?.Trim()}.{o.Extension?.Trim()}"),
                    Name = o.Nom?.Trim(),
                    Extension = o.Extension?.Trim(),
                    Date = o.DateInsertion,
                    Labels = o.Etiquettes?.Select(e => e.Nom?.Trim()).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
                }).ToList();
            }
            catch (Exception)
            {
                return new List<ImageDocumentWithLabels>();
            }
        }

        /// <summary>
        /// Get patient with their portrait image specifically
        /// </summary>
        public async Task<PatientDto?> GetPatientWithPortrait(int id)
        {
            try
            {
                var patientData = await _context.Patients
                    .Include(p => p.Personne)
                    .Where(p => p.IdPersonne == id)
                    .Select(p => new
                    {
                        Patient = p,
                        Personne = p.Personne,
                        PortraitImages = _context.Objets
                            .Where(o => o.IdPatient == p.IdPersonne)
                            .Where(o => o.Etiquettes.Any(e => e.Nom == "Portrait sourire"))
                            .Select(o => new { o.Nom, o.Extension })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (patientData == null)
                    return null;

                // Prendre la première image portrait s'il y en a plusieurs
                var portraitImage = patientData.PortraitImages.FirstOrDefault();
                string? imagePath = null;

                if (portraitImage != null)
                {
                    var fileName = $"{portraitImage.Nom?.Trim()}.{portraitImage.Extension?.Trim()}";
                    imagePath = Path.Combine(patientData.Patient.IdPersonne.ToString(), fileName);
                }

                return new PatientDto
                {
                    InternId = patientData.Personne.IdPersonne,
                    LastName = patientData.Personne.PerNom?.Trim() ?? "",
                    FirstName = patientData.Personne.PerPrenom?.Trim() ?? "",
                    PhoneNumber = patientData.Personne.PerTelPrinc?.Trim() ?? "",
                    Gender = patientData.Personne.PerGenre?.Trim() ?? "",
                    BirthDate = patientData.Personne.PerDatNaiss,
                    CaseId = patientData.Patient.PatNumDossier,
                    Image = imagePath
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get available labels/tags for images
        /// </summary>
        public async Task<List<string>> GetAvailableImageLabels()
        {
            try
            {
                var labels = await _context.Etiquettes
                    .Where(e => e.Nom != null)
                    .Select(e => e.Nom.Trim())
                    .Distinct()
                    .OrderBy(name => name)
                    .ToListAsync();

                return labels;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Add label to an image
        /// </summary>
        public async Task<bool> AddLabelToImage(int imageId, string labelName)
        {
            try
            {
                // Vérifier si l'étiquette existe
                var etiquette = await _context.Etiquettes
                    .FirstOrDefaultAsync(e => e.Nom == labelName);

                if (etiquette == null)
                {
                    // Créer la nouvelle étiquette si elle n'existe pas
                    etiquette = new Etiquette { Nom = labelName };
                    _context.Etiquettes.Add(etiquette);
                    await _context.SaveChangesAsync();
                }

                // Vérifier si la liaison existe déjà
                var existingLink = await _context.ObjetEtiquettes
                    .FirstOrDefaultAsync(oe => oe.IdObjet == imageId && oe.IdEtiquette == etiquette.PkEtiquette);

                if (existingLink == null)
                {
                    // Créer la liaison
                    var objetEtiquette = new ObjetEtiquette
                    {
                        IdObjet = imageId,
                        IdEtiquette = etiquette.PkEtiquette
                    };

                    _context.ObjetEtiquettes.Add(objetEtiquette);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove label from an image
        /// </summary>
        public async Task<bool> RemoveLabelFromImage(int imageId, string labelName)
        {
            try
            {
                var objetEtiquette = await _context.ObjetEtiquettes
                    .Include(oe => oe.Etiquette)
                    .FirstOrDefaultAsync(oe => oe.IdObjet == imageId && oe.Etiquette.Nom == labelName);

                if (objetEtiquette != null)
                {
                    _context.ObjetEtiquettes.Remove(objetEtiquette);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }

    // Classe helper pour les images avec labels
    public class ImageDocumentWithLabels : ImageDocument
    {
        public string? Extension { get; set; }
        public List<string> Labels { get; set; } = new();
    }
}