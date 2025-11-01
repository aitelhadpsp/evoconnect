using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EvoConnect.Server.Data
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
        {
        }

        public DbSet<Personne> Personnes { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<RendezVous> RendezVous { get; set; }
        public DbSet<PlanApplique> PlansAppliques { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<Devis> Devis { get; set; }
        public DbSet<Mutuelle> Mutuelles { get; set; }
        public DbSet<Caisse> Caisses { get; set; }
        public DbSet<FileRecord> Objets { get; set; }
        public DbSet<Etiquette> Etiquettes { get; set; }
        public DbSet<ObjetEtiquette> ObjetEtiquettes { get; set; }
        public DbSet<DentalisDevis> DentalisDevis { get; set; }
        public DbSet<Fauteuil> Fauteuils { get; set; }
        public DbSet<Acte> Actes { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<ZoneComm> ZoneComm { get; set; }
        public DbSet<TracerAppNatif> TracerAppNatif { get; set; }
        public DbSet<DentalisActes> DentalisActes { get; set; }
        public DbSet<DentalisActesPatient> DentalisActesPatient { get; set; }
        public DbSet<DentalisFamillesActes> DentalisFamillesActes { get; set; }
        public DbSet<CodeSecu> CodeSecu { get; set; }
        public DbSet<KpiConfig> KpiConfigs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
    

            modelBuilder.Entity<KpiConfig>(entity =>
            {
                entity.ToTable("KPI_CONFIG");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.IsEnabled)
                    .HasDatabaseName("IDX_KPI_ENABLED");

                entity.Property(e => e.TargetValue)
                    .HasPrecision(10, 2);

                entity.Property(e => e.WarningThreshold)
                    .HasPrecision(10, 2);

                entity.Property(e => e.CriticalThreshold)
                    .HasPrecision(10, 2);
            });
        

            modelBuilder.Entity<Utilisateur>(entity =>
            {
                entity.HasOne(d => d.Personne)
                    .WithOne(p => p.Utilisateur)
                    .HasForeignKey<Utilisateur>(d => d.IdPersonne)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
            modelBuilder.Entity<ZoneComm>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.ZoneComm)
                    .HasForeignKey(d => d.IdPatient)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
       modelBuilder.Entity<DentalisActes>(entity =>
            {
                entity.HasKey(e => e.ActePk);
                
                entity.HasOne(e => e.Famille)
                    .WithMany(f => f.Actes)
                    .HasForeignKey(e => e.ActeFamille)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CodeSecu)
                    .WithMany(c => c.Actes)
                    .HasForeignKey(e => e.ActeCodeSecu)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DentalisActesPatient>(entity =>
            {
                entity.HasKey(e => e.ApPk);

                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.DentalisActes)
                    .HasForeignKey(e => e.ApPatient)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Acte)
                    .WithMany(a => a.ActesPatient)
                    .HasForeignKey(e => e.ApActe)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CodeSecu)
                    .WithMany()
                    .HasForeignKey(e => e.IdCodeSecu)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DentalisFamillesActes>(entity =>
            {
                entity.HasKey(e => e.FactePk);
            });

            modelBuilder.Entity<CodeSecu>(entity =>
            {
                entity.HasKey(e => e.IdCodeSecu);
            });
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasOne(d => d.Personne)
                    .WithMany(p => p.Patients)
                    .HasForeignKey(d => d.IdPersonne)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ProfessionnelPersonne)
                    .WithMany()
                    .HasForeignKey(d => d.PerIdPersonne)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Professionnel2Personne)
                    .WithMany()
                    .HasForeignKey(d => d.Per2IdPersonne)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Professionnel3Personne)
                    .WithMany()
                    .HasForeignKey(d => d.Per3IdPersonne)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Professionnel4Personne)
                    .WithMany()
                    .HasForeignKey(d => d.Per4IdPersonne)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Professionnel5Personne)
                    .WithMany()
                    .HasForeignKey(d => d.Per5IdPersonne)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<RendezVous>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.RendezVous)
                    .HasForeignKey(d => d.IdPersonne)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.Professionnel)
                    .WithMany(p => p.RendezVousAsProfessionnel)
                    .HasForeignKey(d => d.PerIdPersonne)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.Fauteuil)
                    .WithMany(f => f.RendezVous)
                    .HasForeignKey(d => d.IdFauteuil)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.Acte)
                    .WithMany(a => a.RendezVous)
                    .HasForeignKey(d => d.IdActe)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<PlanApplique>(entity =>
            {
                entity.HasKey(pa => new { pa.IdPatient, pa.CodePlan, pa.NumDate, pa.TypeRegle });

                entity.HasOne(d => d.Patient)
                    .WithMany()
                    .HasForeignKey(d => d.IdPatient)
                    .OnDelete(DeleteBehavior.ClientSetNull);



                entity.HasOne(d => d.Praticien)
                    .WithMany()
                    .HasForeignKey(d => d.IdPraticien)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Mutuelle)
                    .WithMany(m => m.PlansAppliques)
                    .HasForeignKey(d => d.IdMutuelle)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Caisse)
                    .WithMany(c => c.PlansAppliques)
                    .HasForeignKey(d => d.IdCaisse)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Facture relationships
            modelBuilder.Entity<Facture>(entity =>
            {
                entity.HasOne(d => d.Personne)
                    .WithMany()
                    .HasForeignKey(d => d.IdPersonne)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Utilisateur)
                    .WithMany()
                    .HasForeignKey(d => d.IdUtil)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Configure Devis relationships
            modelBuilder.Entity<Devis>(entity =>
            {

                entity.Property(d => d.DevisPk)
                    .ValueGeneratedOnAdd();
            });

            // Configure Personne navigation properties
            modelBuilder.Entity<Personne>(entity =>
            {
                entity.HasMany(e => e.RendezVousAsProfessionnel)
                    .WithOne(e => e.Professionnel)
                    .HasForeignKey(e => e.PerIdPersonne);
            });


            // Configure Mutuelle relationships
            modelBuilder.Entity<Mutuelle>(entity =>
            {
                entity.HasMany(m => m.PlansAppliques)
                    .WithOne(pa => pa.Mutuelle)
                    .HasForeignKey(pa => pa.IdMutuelle);
            });

            // Configure Caisse relationships
            modelBuilder.Entity<Caisse>(entity =>
            {
                entity.HasMany(c => c.PlansAppliques)
                    .WithOne(pa => pa.Caisse)
                    .HasForeignKey(pa => pa.IdCaisse);
            });

            // Configure Objet relationships
            modelBuilder.Entity<FileRecord>(entity =>
            {
      

             

                // Configuration de la relation Many-to-Many avec Etiquette
                entity.HasMany(o => o.Etiquettes)
                    .WithMany(e => e.Objets)
                    .UsingEntity<ObjetEtiquette>(
                        j => j
                            .HasOne(oe => oe.Etiquette)
                            .WithMany(e => e.ObjetEtiquettes)
                            .HasForeignKey(oe => oe.IdEtiquette),
                        j => j
                            .HasOne(oe => oe.Objet)
                            .WithMany(o => o.ObjetEtiquettes)
                            .HasForeignKey(oe => oe.IdObjet),
                        j =>
                        {
                            j.Property(oe => oe.IdPk).ValueGeneratedOnAdd();
                        });
            });

            // Configure Etiquette relationships
            modelBuilder.Entity<Etiquette>(entity =>
            {
                // Configuration pour l'auto-génération de la clé primaire
                entity.Property(e => e.PkEtiquette)
                    .ValueGeneratedOnAdd();
            });

            // Configure ObjetEtiquette relationships
            modelBuilder.Entity<ObjetEtiquette>(entity =>
            {
                // Configuration pour l'auto-génération de la clé primaire
                entity.Property(oe => oe.IdPk)
                    .ValueGeneratedOnAdd();
            });

            // Configure DentalisDevis relationships
            modelBuilder.Entity<DentalisDevis>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(d => d.DentalisDevis)
                    .HasForeignKey(d => d.DevisIdPatient)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.Property(d => d.DevisPk)
                    .ValueGeneratedOnAdd();

                // Configuration des valeurs par défaut
                entity.Property(d => d.DevisNom)
                    .HasDefaultValue("");

                entity.Property(d => d.DevisIdPatient)
                    .HasDefaultValue(-1);

                entity.Property(d => d.DevisAccepte)
                    .HasDefaultValue((short)0);

                entity.Property(d => d.DevisMontant)
                    .HasDefaultValue(0.0f);

                entity.Property(d => d.DevisDateCreat)
                    .HasDefaultValue(new DateTime(2000, 1, 1));

                entity.Property(d => d.DevisDateMaj)
                    .HasDefaultValue(new DateTime(2000, 1, 1));

                entity.Property(d => d.DevisDateImpr)
                    .HasDefaultValue(new DateTime(2000, 1, 1));

                entity.Property(d => d.DevisPresente)
                    .HasDefaultValue((short)0);

                entity.Property(d => d.DevisDatePresent)
                    .HasDefaultValue(new DateTime(2000, 1, 1));

                entity.Property(d => d.DevisRemise)
                    .HasDefaultValue(0.0f);

                entity.Property(d => d.DevisMontantAvRemise)
                    .HasDefaultValue(0.0f);

                entity.Property(d => d.DevisRemiseType)
                    .HasDefaultValue((short)0);
            });
            modelBuilder.Entity<Fauteuil>(entity =>
   {
       entity.HasMany(f => f.RendezVous)
           .WithOne()
           .HasForeignKey(rdv => rdv.IdFauteuil)
           .OnDelete(DeleteBehavior.ClientSetNull);

       // Configuration pour l'auto-génération de la clé primaire
       entity.Property(f => f.IdFauteuil)
           .ValueGeneratedOnAdd();

       // Configuration des valeurs par défaut
       entity.Property(f => f.FautPraticien)
           .HasDefaultValue(-1);
   });
        }

        // Override SaveChanges to handle triggers
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Personne || e.Entity is RendezVous || e.Entity is PlanApplique)
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is Personne personne)
                {
                    personne.LastModif = DateTime.Now;

                    // Handle GUID generation for new entities
                    if (entityEntry.State == EntityState.Added && (personne.Guid == null || personne.Guid.Length == 0))
                    {
                        personne.Guid = Guid.NewGuid().ToByteArray();
                    }
                }
                else if (entityEntry.Entity is RendezVous rdv)
                {
                    rdv.LastModif = DateTime.Now;

                    // Handle GUID generation for new entities
                    if (entityEntry.State == EntityState.Added && (rdv.Guid == null || rdv.Guid.Length == 0))
                    {
                        rdv.Guid = Guid.NewGuid().ToByteArray();
                    }
                }
                else if (entityEntry.Entity is PlanApplique planApplique)
                {
                    // Handle GUID generation for new entities
                    if (entityEntry.State == EntityState.Added && (planApplique.Guid == null || planApplique.Guid.Length == 0))
                    {
                        planApplique.Guid = Guid.NewGuid().ToByteArray();
                    }
                }
            }
        }
               private  void ConfigureGuidByteArrayConversionsAdvanced(ModelBuilder modelBuilder)
        {
            var guidByteArrayConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<byte[], Guid?>(
                v => v != null ? new Guid(v) : null,
                v => v.HasValue ? v.Value.ToByteArray() : null
            );

            var byteArrayGuidConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Guid?, byte[]>(
                v => v.HasValue ? v.Value.ToByteArray() : null,
                v => v != null ? new Guid(v) : null
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties()
                    .Where(p =>
                        p.Name.Contains("Guid", StringComparison.OrdinalIgnoreCase) &&
                        (p.ClrType == typeof(byte[]) || p.ClrType == typeof(byte[])))
                    .ToList();

                foreach (var property in properties)
                {
                    if (property.ClrType == typeof(byte[]))
                    {
                        property.SetValueConverter(guidByteArrayConverter);
                    }
                }
            }
        }
  
    }

    // Extension methods for easier usage
    public static class DbContextExtensions
    {
        public static IQueryable<Patient> WithPersonne(this IQueryable<Patient> patients)
        {
            return patients.Include(p => p.Personne);
        }

        public static IQueryable<RendezVous> WithPatientAndProfessionnel(this IQueryable<RendezVous> rendezVous)
        {
            return rendezVous
                .Include(r => r.Patient)
                .Include(r => r.Professionnel);
        }

        public static IQueryable<Patient> WithAllRelations(this IQueryable<Patient> patients)
        {
            return patients
                .Include(p => p.Personne)
                .Include(p => p.ProfessionnelPersonne)
                .Include(p => p.Professionnel2Personne)
                .Include(p => p.Professionnel3Personne)
                .Include(p => p.Professionnel4Personne)
                .Include(p => p.Professionnel5Personne);
        }

        public static IQueryable<PlanApplique> WithAllRelations(this IQueryable<PlanApplique> plansAppliques)
        {
            return plansAppliques
                .Include(pa => pa.Patient)
                .ThenInclude(p => p.Personne)
                .Include(pa => pa.Praticien)
                .Include(pa => pa.Mutuelle)
                .Include(pa => pa.Caisse);
        }

        public static IQueryable<Facture> WithRelations(this IQueryable<Facture> factures)
        {
            return factures
                .Include(f => f.Personne)
                .Include(f => f.Utilisateur);
        }





        public static IQueryable<FileRecord> WithEtiquettes(this IQueryable<FileRecord> objets)
        {
            return objets.Include(o => o.Etiquettes);
        }

   

        public static IQueryable<DentalisDevis> WithPatient(this IQueryable<DentalisDevis> devis)
        {
            return devis.Include(d => d.Patient)
                       .ThenInclude(p => p.Personne);
        }

        public static IQueryable<DentalisDevis> WithAllRelations(this IQueryable<DentalisDevis> devis)
        {
            return devis.Include(d => d.Patient)
                       .ThenInclude(p => p.Personne);
        }

        public static IQueryable<Patient> WithObjets(this IQueryable<Patient> patients)
        {
            return patients.Include(p => p.Personne);

        }
        public static IQueryable<RendezVous> WithFauteuil(this IQueryable<RendezVous> rendezVous)
        {
            return rendezVous.Include(rdv => rdv.Patient)
                           .ThenInclude(p => p.Personne)
                           .Include(rdv => rdv.Professionnel);
        }
       }
}