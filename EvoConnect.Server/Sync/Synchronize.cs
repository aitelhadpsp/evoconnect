using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvoConnect.Common.Models;
using FirebirdSql.Data.FirebirdClient;

using Newtonsoft.Json;


using EvoConnect.Server.Repository.Interfaces;
using System.Transactions;
using EvoConnect.Server.Data;
using EvoConnect.Common;
using EvoConnect.Common.Helpers;

namespace EvoConnect.Server.Sync
{
    public class Synchronise(CancelationConf _cancelation, IExecuterDA _executerDA)
    {

        private readonly DbContext syncDbContext = new();
        private readonly LocalSyncronise   localSyncronise = new();


        public void Reset()
        {
            syncDbContext.Reset();
            _cancelation.Reset();
        }


        public async Task SyncTreatments()
        {


            var conn = AppData.ConnectionString();
            var connection = new FbConnection(conn);

            DateTime? date = syncDbContext.GetTreatmentsSync();
            var error = false;
            try
            {
                await connection.OpenAsync();

                string countSql = @"  SELECT 
                                     count(AP_PK)
                                    FROM DENTALIS_ACTES_PATIENT
                                    INNER JOIN DENTALIS_ACTES ON ACTE_PK=AP_ACTE
                                    Left outer JOIN DENTALIS_FAMILLES_ACTES ON FACTE_PK=ACTE_FAMILLE
                                    Left outer join DENTALIS_DEVIS DE ON DEVIS_PK=AP_DEVIS
                                    Left outer join CODE_SECU CS ON CS.ID_CODESECU = ACTE_CODE_SECU
                                    left join PREFERENCE Prf on DENTALIS_ACTES_PATIENT.AP_PRAT=Prf.ID_UTIL and prf.PREF_CLE='PRATICIEN_COLOR'
                                    left join Personne prat on DENTALIS_ACTES_PATIENT.AP_PRAT=prat.ID_PERSONNE
                                    LEFT JOIN ACTE_CORRESPONDANCE ac ON CLE_AUTRE_LOGICIEL = AP_PK
                                    LEFT JOIN PLAN_APPLIQUE pa ON pa.INFOSECHEANCES = ac.ID_PLAN_ORTHALIS
                                    LEFT JOIN FACTUREPAIEMENT fp ON pa.ID_PLAN = fp.ID_PLANFACTURE

                                          " + (date != null ? @" WHERE ( DATE_SOLDER >= @date OR AP_DATE_MAJ >= @date OR AP_DATE_REALISE >= @date OR DATEDEPPREVUE >= @day OR DATE_RENS >= @date ) " : "");

                using var ccommand = new FbCommand(countSql, connection);
                if (date != null)
                {
                    ccommand.Parameters.AddWithValue("@date", date);
                    ccommand.Parameters.AddWithValue("@day", date.Value.Date);
                }
                using var creader = await ccommand.ExecuteReaderAsync();
                int count = 0;

                while (await creader.ReadAsync())
                {
                    count = creader.GetInt32(0);
                }
                var pages = Math.Ceiling(count / 200.0);

                var now = DateTime.Now;

                for (int i = 0; i <= pages; i++)
                {
                    var offset = i * 200;



                    string sql = @"  SELECT 
                                      FIRST 200 SKIP @offset
                                        
                                        AP_PK AS InternId,
                                        AP_PATIENT AS PatientId,
                                        AP_LIBELLE_ACTE AS Label,
                                        AP_MONTANT AS Amount,
                                        MONTANT_ENCAIS_EURO Paid,
                                        AP_DATE_REALISE RealisedDate,
                                        AP_REALISE IsRealised,
                                        AP_SELECTED_DENTS Teeths,
                                        AP_DATE_CREATION AS CreatedDate
                                    FROM DENTALIS_ACTES_PATIENT
                                    INNER JOIN DENTALIS_ACTES ON ACTE_PK=AP_ACTE
                                    Left outer JOIN DENTALIS_FAMILLES_ACTES ON FACTE_PK=ACTE_FAMILLE
                                    Left outer join DENTALIS_DEVIS DE ON DEVIS_PK=AP_DEVIS
                                    Left outer join CODE_SECU CS ON CS.ID_CODESECU = ACTE_CODE_SECU
                                    left join PREFERENCE Prf on DENTALIS_ACTES_PATIENT.AP_PRAT=Prf.ID_UTIL and prf.PREF_CLE='PRATICIEN_COLOR'
                                    left join Personne prat on DENTALIS_ACTES_PATIENT.AP_PRAT=prat.ID_PERSONNE
                                    LEFT JOIN ACTE_CORRESPONDANCE ac ON CLE_AUTRE_LOGICIEL = AP_PK
                                    LEFT JOIN PLAN_APPLIQUE pa ON pa.INFOSECHEANCES = ac.ID_PLAN_ORTHALIS
                                    LEFT JOIN FACTUREPAIEMENT fp ON pa.ID_PLAN = fp.ID_PLANFACTURE

                                          " + (date != null ? @" Where ( DATE_SOLDER >= @date OR AP_DATE_MAJ >= @date OR AP_DATE_REALISE >= @date OR DATEDEPPREVUE >= @day OR DATE_RENS >= @date ) " : "") +
                                      @" ORDER BY CreatedDate Asc ";









                    using var command = new FbCommand(sql, connection);

                    command.Parameters.AddWithValue("@offset", offset);

                    if (date != null)
                    {
                        command.Parameters.AddWithValue("@date", date);
                        command.Parameters.AddWithValue("@day", date.Value.Date);
                    }
                    using var reader = await command.ExecuteReaderAsync();
                    List<Treatment> data = [];
                    while (await reader.ReadAsync())
                    {

                        data.Add(new Treatment
                        {
                            InternId = reader.GetInt32(0),
                            PatientId = reader.GetInt32(1),
                            Label = reader.GetString(2).Trim(),
                            Amount = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                            Paid = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                            RealisedDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            IsRealised = reader.GetBoolean(6),
                            Teeths = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            CreatedDate = reader.GetDateTime(8)
                        });
                    }



                    if (data.Count != 0)
                    {
                        var res = await Helpers.SendData(Api.syncTreatemant, data);
                    }
                }
                syncDbContext.UpdateTreatmentsSync(now);
            }
            catch (OperationCanceledException)
            {
                error = false;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                error = true;
            }
            finally
            {
                await connection.DisposeAsync();
                if (error)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await SyncTreatments();
                }


            }


        }
        public async Task SyncPayments()
        {
            DateTime? date = syncDbContext.GetPaymentSync();
            var connection = new FbConnection(AppData.ConnectionString());
            var still = true;
            var page = 0;

            var error = false;

            try
            {
                var now = DateTime.Now;
                await connection.OpenAsync();

                while (still)
                {

                    string sql = @$"SELECT
                         FIRST 200 SKIP @offset
                     ID_PLAN , ID_PATIENT , DATE_REF_PAIE , MONTANT_ENCAIS_EURO , MODE_PAY  FROM PLAN_APPLIQUE WHERE TYPE_REGLE=0 {(date == null ? "" : "AND DATE_REF_PAIE >=@date")}
                                Order By DATE_REF_PAIE asc ";
                    using var command = new FbCommand(sql, connection);
                    if (date != null)
                    {
                        command.Parameters.AddWithValue("@date", date.Value.Date);
                    }
                    command.Parameters.AddWithValue("@offset", 200 * page);
                    page++;


                    List<Payment> data = [];
                    using var reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        Payment payment = new();

                        if (!reader.IsDBNull(0))
                        {
                            payment.Id = reader.GetInt32(0);
                        }

                        if (!reader.IsDBNull(1))
                        {
                            payment.PatientId = reader.GetInt32(1);
                        }

                        if (!reader.IsDBNull(2))
                        {
                            payment.MadeAt = reader.GetDateTime(2);
                        }

                        if (!reader.IsDBNull(3))
                        {
                            payment.Amount = reader.GetFloat(3);
                        }

                        if (!reader.IsDBNull(4))
                        {
                            payment.PaymentMethode = reader.GetInt32(4);
                        }

                        data.Add(payment);
                    }

                    if (data.Count > 0)
                    {
                        var res = await Helpers.SendData(Api.syncPayments, data);
                    }
                    else still = false;

                }
                syncDbContext.UpdatePaymentsSync(now);
            }
            catch (OperationCanceledException)
            {
                error = false;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                error = true;
            }
            finally
            {
                await connection.CloseAsync();
                if (error)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await SyncPayments();
                }
            }

        }
        public async Task CollectDoctors()
        {
            var excep = false;

            using FbConnection fbconnection = new(AppData.ConnectionString());
            try
            {
                await fbconnection.OpenAsync();
                FbCommand command = new(
            @"SELECT 
                ID_UTIL,
                UTIL_IDENT 
            FROM 
                UTILISATEUR 
            WHERE
                ID_PERSONNE IS NOT NULL
            AND
                UTIL_TYPE=4 ;
                 ",
           fbconnection);

                var now = DateTime.Now;


                using FbDataReader reader = await command.ExecuteReaderAsync();


                var list = new List<Doctor>();
                TimeZoneInfo localZone = TimeZoneInfo.Local;

                while (reader.Read())
                {
                    var item = new Doctor
                    {
                        InternId = reader.GetInt32(0),
                        DisplayName = reader.GetString(1),
                    };
                    list.Add(item);
                }
                await reader.DisposeAsync();



                var res = await Helpers.SendData(Api.syncDoctors, list);


                syncDbContext.UpdateDoctorsSync(now);


            }
            catch (OperationCanceledException)
            {
                excep = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                excep = true;

            }
            finally
            {
                await fbconnection.DisposeAsync();
                if (excep)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await CollectDoctors();
                }
            }


        }
        public async Task CollectAppointments()
        {
            using FbConnection fbconnection = new(AppData.ConnectionString());
            var error = false;
            try
            {
                await fbconnection.OpenAsync();
                DateTime todayAtMidnight = DateTime.Today;
                FbCommand command = new(
               @"SELECT 
                    R.ID_RDV, 
                    R.RDV_DATE, 
                    R.ID_PERSONNE,
                    R.ID_ACTE, 
                    P.PER_NOM, 
                    P.PER_PRENOM, 
                    P.PER_TELPRINC , 
                    P.PER_GENRE, 
                    A.ACTE_LIBELLE,     
                    A.ACTE_COULEUR, 
                    U.UTIL_IDENT,
                    U.ID_UTIL,
                    R.GUID
                    FROM RENDEZ_VOUS R 
                    JOIN PERSONNE P ON R.ID_PERSONNE = P.ID_PERSONNE 
                    JOIN UTILISATEUR U ON R.PER_ID_PERSONNE = U.ID_UTIL     
                    JOIN ACTES A ON R.ID_ACTE = A.ID_ACTE 
                    WHERE R.RDV_DATE >= TIMESTAMP 'today' AND R.RDV_DATE < DATEADD(DAY, 15, TIMESTAMP 'today') ",
              fbconnection);
                using FbDataReader reader = await command.ExecuteReaderAsync();

                var list = new List<Appointment>();
                TimeZoneInfo localZone = TimeZoneInfo.Local;

                while (reader.Read())
                {
                    var item = new Appointment
                    {
                        ID_RDV = reader.GetInt32(0),
                        RDV_DATE = reader.GetDateTime(1),
                        ID_PERSONNE = reader.GetInt32(2),
                        ID_ACTE = reader.GetInt32(3),
                        PER_NOM = reader.IsDBNull(4) ? "" : reader.GetString(4).ToString(),
                        PER_PRENOM = reader.IsDBNull(5) ? "" : reader.GetString(5).ToString(),
                        PER_TELPRINC = reader.IsDBNull(6) ? "" : reader.GetString(6).ToString(),
                        PER_GENRE = reader.IsDBNull(7) ? "" : reader.GetString(7).ToString(),
                        ACTE_LIBELLE = reader.IsDBNull(8) ? "" : reader.GetString(8).ToString(),
                        ACTE_COULEUR = "#" + (reader.IsDBNull(7) ? "FFF" : Helpers.ColorToHex(Color.FromArgb(reader.GetInt32(9)))),
                        UTIL_IDENT = reader.IsDBNull(10) ? "" : reader.GetString(10).ToString(),
                        ID_UTIL = reader.GetInt32(11),
                    };
                    list.Add(item);
                }
                await reader.DisposeAsync();

                var response = await Helpers.SendData(Api.Collect, list); 

                var res = JsonConvert.DeserializeObject<List<AppointmentCollect>>((string)response);
                res?.ForEach(async e =>
                {
                    await localSyncronise.UpdateConfirmation(
                        e.InternIds,
                        e.Ids,
                        e.Status == AppointmentStatusEnum.Delivered ? "livre" :
                        e.Status == AppointmentStatusEnum.Accepted ? "Rdv confirme" :
                        e.Status == AppointmentStatusEnum.Refused ? "Rdv annule" :
                        e.Status == AppointmentStatusEnum.DelayRequested ? "demande de Rdv" :
                        e.Status == AppointmentStatusEnum.Error ? "Erreur" : ""
                        );
                });

        syncDbContext.UpdateAppointmentSync();


            }
            catch (OperationCanceledException)
            {
                error = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                error = true;
            }
            finally
            {
                await fbconnection.DisposeAsync();
                if (error)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await CollectAppointments();
                }
            }
        }
        public async Task CollectImageLabels()
        {
            var excep = false;
            int? last = syncDbContext.GetImageLabelSync();
            var connection = new FbConnection(AppData.ConnectionString());

            try
            {
                await connection.OpenAsync();
                var still = true;
                var page = 0;
                while (still)
                {

                    string sql = @"SELECT 
                    FIRST 200 SKIP @offset
                    ID_PK as Id,
                    ID_ETIQUETTE as LabelId,
                    ID_OBJET as ImageId
                    
                     FROM OBJETS_ETIQUETTE
                     ";
                    if (last != null)
                    {
                        sql += " WHERE ID_PK > @last";
                    }
                    sql += "  Order by ID_PK ASC";

                    using FbCommand command = new FbCommand(sql, connection);
                    command.Parameters.AddWithValue("@offset", 200 * page);
                    page++;
                    if (last != null)
                    {
                        command.Parameters.AddWithValue("@last", last);
                    }

                    using FbDataReader reader = await command.ExecuteReaderAsync();
                    DataTable dt = new();
                    dt.Load(reader);
                    List<ImageLink> list = [.. dt.AsEnumerable()
                                                    .Select(row => {
                                                        return new ImageLink
                                                        {

                                                         InternImageId=int.Parse( row["ImageId"].ToString()),
                                                         InternLabelId=int.Parse( row["LabelId"].ToString()),
                                                         InterId=int.Parse( row["Id"].ToString()),
                                                        };
                                                        }

                                                    ),
                                               ];
                    if (list.Count > 0)
                    {

                        var response = await Helpers.SendData(Api.syncImageLabels, list);
                        last = list.Last().InterId;

                        syncDbContext.UpdateImageLabelsSync((int)last);

                    }
                    else
                    {
                        still = false;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            catch (OperationCanceledException)
            {
                excep = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                excep = true;
            }
            finally
            {
                connection.Dispose();
                if (excep)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await CollectImageLabels();
                }
            }

        }
        public async Task CollectLabels()
        {
            var exeption = false;
            using FbConnection fbconnection = new(AppData.ConnectionString());
            try
            {
                await fbconnection.OpenAsync();
                FbCommand command = new(
            @"SELECT 
                PK_ETIQUETTE,
                NOM 
            FROM 
                ETIQUETTE ;
                 ",
           fbconnection);

                var now = DateTime.Now;


                using FbDataReader reader = await command.ExecuteReaderAsync();


                var list = new List<ImageLabel>();
                TimeZoneInfo localZone = TimeZoneInfo.Local;

                while (reader.Read())
                {
                    var item = new ImageLabel
                    {
                        InternId = reader.GetInt32(0),
                        Label = reader.GetString(1),
                    };
                    list.Add(item);
                }
                await reader.DisposeAsync();
                var response = await Helpers.SendData(Api.syncLabels, list);
                syncDbContext.UpdateLabelSync(now);

            }
            catch (OperationCanceledException)
            {
                exeption = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                exeption = true;

            }
            finally
            {
                await fbconnection.DisposeAsync();
                if (exeption)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await CollectLabels();
                }

            }


        }
        public async Task CollectImages()
        {
            DateTime? last = syncDbContext.GetImagesSync();
            var connection = new FbConnection(AppData.ConnectionString());
            var now = DateTime.Now;
            var error = false;
            try
            {
                await connection.OpenAsync();
                var still = true;
                var page = 0;
                while (still)
                {

                    string sql = @"SELECT 
                    FIRST 200 SKIP @offset
                     PK_OBJET as Id,
                     ID_PATIENT as PatientId,
                     NOM as Path,
                     EXTENSION as Ext,
                     DATEINSERTION as CreatedAt
                      FROM OBJET
                     ";
                    if (last != null)
                    {
                        sql += " WHERE DATEINSERTION >= @last";
                    }
                    sql += "  Order by CreatedAt";

                    using FbCommand command = new FbCommand(sql, connection);
                    command.Parameters.AddWithValue("@offset", 200 * page);
                    page++;
                    if (last != null)
                    {
                        command.Parameters.AddWithValue("@last", last);
                    }

                    using FbDataReader reader = await command.ExecuteReaderAsync();
                    DataTable dt = new();
                    dt.Load(reader);
                    var dir = AppData.ImageDir();
                    var count = dt.AsEnumerable().Count();

                    foreach (var row in dt.AsEnumerable())
                    {

                        using var formData = new MultipartFormDataContent();

                        var path = dir + row["PatientId"] + "\\" + row["Path"] + "." + row["Ext"];
                        FileStream? fileStream = null; ;
                        try
                        {
                            fileStream = File.OpenRead(path);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error: {e.Message}");
                            continue;
                        }
                        formData.Add(new StreamContent(fileStream), "Image", Path.GetFileName(path));
                        formData.Add(new StringContent(row["id"].ToString()), "InternId");
                        formData.Add(new StringContent(row["PatientId"].ToString()), "PatientId");
                        try
                        {
                            var response = await Helpers.SendData(Api.syncImages, formData);
                        }
                        catch
                        {
                            await fileStream.DisposeAsync();

                            throw;
                        }



                    }

                    if (count == 0)
                    {
                        still = false;
                    }
                }
                syncDbContext.UpdateImagesSync(now);
            }
            catch (OperationCanceledException)
            {
                error = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                error = true;
            }
            finally
            {
                connection.Dispose();
                if (error)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await CollectImages();
                }

            }
        }
        public async Task SynchronizePatients()
        {
            DateTime? last = syncDbContext.GetPatientSync();
            using FbConnection connection = new(AppData.ConnectionString());
            var still = true;
            var error = false;
            var page = 0;
            try
            {
                var now = DateTime.Now;
                await connection.OpenAsync();

                while (still)
                {
                    string sql = @"SELECT
                     FIRST 200 SKIP @offset
                    ID_PERSONNE, PER_NOM , PER_PRENOM, PER_TELPRINC, PER_GENRE, PER_DATNAISS FROM PERSONNE 
                    WHERE PER_TYPE = 1 ";
                    if (last != null)
                    {
                        sql += " AND LASTMODIF >= @last";
                    }
                    sql += " Order By LASTMODIF";

                    using var command = new FbCommand(sql, connection);
                    command.Parameters.AddWithValue("@offset", 200 * page);
                    page++;
                    if (last != null)
                    {
                        command.Parameters.AddWithValue("@last", last);
                    }

                    using var reader = await command.ExecuteReaderAsync();
                    List<Common.Models.PatientDto> data = [];

                    while (reader.Read())
                    {
                        data.Add(new Common.Models.PatientDto
                        {
                            InternId = reader.GetInt32(0),
                            LastName = reader.GetString(1).Trim(),
                            FirstName = reader.GetString(2).Trim(),
                            PhoneNumber = reader.GetString(3)?.Trim() ?? "",
                            Gender = reader.GetString(4).Trim(),
                            BirthDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5)

                        });
                    }

                    if (data.Count > 0)
                    {
                        var response = await Helpers.SendData(Api.syncPatients, data);
                    }
                    else still = false;

                }
                syncDbContext.UpdatePatientSync(now);
            }
            catch (OperationCanceledException)
            {
                error = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                error = true;
            }
            finally
            {
                await connection.DisposeAsync();
                if (error)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await SynchronizePatients();
                }
            }
        }
        public async Task SynchronizeNotes()
        {
            DateTime? last = syncDbContext.GetNoteSync();
            var connection = new FbConnection(AppData.ConnectionString());
            var now = DateTime.Now;
            var error = false;
            try
            {
                await connection.OpenAsync();
                var still = true;
                var page = 0;
                while (still)
                {
                    string sql = @"SELECT 
                    FIRST 200 SKIP @offset
                     ID_COMM as Id, DATE_COMM as CreatedAt, FAIT as Done,AFAIRE as ToBeDone ,ID_PAT as PatientId FROM ZONE_COMM
                     ";
                    if (last != null)
                    {
                        sql += " WHERE DATE_COMM >= @last";
                    }
                    sql += "  Order by CreatedAt";

                    using FbCommand command = new FbCommand(sql, connection);
                    command.Parameters.AddWithValue("@offset", 200 * page);
                    page++;
                    if (last != null)
                    {
                        command.Parameters.AddWithValue("@last", last);
                    }

                    using FbDataReader reader = await command.ExecuteReaderAsync();
                    DataTable dt = new();
                    dt.Load(reader);
                    List<Note> list = [.. dt.AsEnumerable()
                                                    .Select(row => {
                                                        string? DoneString = row["Done"]?.ToString();
                                                        byte[]? DoneBytes = DoneString is null ? null : System.Text.Encoding.Default.GetBytes(DoneString);
                                                        string? ToBeDoneString = row["ToBeDone"]?.ToString();
                                                        byte[]? ToBeDoneBytes = ToBeDoneString is null ? null: System.Text.Encoding.Default.GetBytes(ToBeDoneString);

                                                        return new Note
                                                        {

                                                            InternId = Convert.ToInt32(row["Id"]),
                                                            PatientId = Convert.ToInt32(row["PatientId"]),
                                                            CreatedAt = Convert.ToDateTime(row["CreatedAt"]).ToUniversalTime(),
                                                            Done = DoneBytes,
                                                            ToBeDone= ToBeDoneBytes

                                                        };
                                                        }

                                                    ),
                                               ];
                    if (list.Count > 0)
                    {
                        var response = await Helpers.SendData(Api.syncNotes, list);
                    }
                    else
                    {
                        still = false;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                syncDbContext.UpdateNoteSync(now);
            }
            catch (OperationCanceledException)
            {
                error = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                error = true;
            }
            finally
            {
                connection.Dispose();
                if (error)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await SynchronizeNotes();
                }

            }
        }
        public async Task CollectDeleted()
        {
            var check = await CheckDeleteTable();
            if (!check)
                return;


            int? last = syncDbContext.GetDeleteSync();
            var now = DateTime.Now;
            var except = false;


            try
            {
                await _executerDA.ExecuteDbOperationAsync(async (connection, transaction) =>
                 {

                     var still = true;
                     var page = 0;
                     while (still)
                     {

                         string sql = @"SELECT 
                    FIRST 200 SKIP @offset
                     ID_ITEM as Id,
                     TABLE_NAME as Name,
                     ID_KEY
                      FROM TRACER_APP_NATIF
                     ";
                         if (last != null)
                         {
                             sql += " WHERE ID_KEY > @last";
                         }
                         sql += "  Order by ID_KEY ";

                         using FbCommand command = new FbCommand(sql, connection, transaction);
                         command.Parameters.AddWithValue("@offset", 200 * page);
                         page++;
                         if (last != null)
                         {
                             command.Parameters.AddWithValue("@last", last);
                         }


                         using FbDataReader reader = await command.ExecuteReaderAsync();
                         DataTable dt = new();
                         dt.Load(reader);
                         var count = dt.AsEnumerable().Count();

                         var list = dt.AsEnumerable().ToList().Select(row => new
                         {
                             InternId = int.Parse(row["Id"].ToString()),
                             TableName = row["Name"].ToString(),
                             Id = int.Parse(row["ID_KEY"].ToString())
                         });

                         var data = new
                         {
                             Data = list
                         };
                         if (count != 0)
                         {
                             var response = await Helpers.SendData(Api.syncDeleted, list);
                         }
                         else
                         {
                             still = false;
                         }
                         var lastId = list.LastOrDefault()?.Id;
                         if (lastId is not null)
                             syncDbContext.UpdateDeleteSync((int)lastId);
                     }
                     return Task.CompletedTask;
                 });
            }
            catch (OperationCanceledException)
            {
                except = false;
                throw;
            }
            catch (Exception ex)
            {
                await Helpers.LogErrorToFileAsync(ex);
                except = true;
            }
            finally
            {
                if (except)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await CollectDeleted();
                }
            }

        }



        public async Task SyncImages()
        {
            var partner = await Getters.GetPartner();
            if (partner is null || partner.SubscriptionType == 0)
                return;
            await CollectLabels();
            await CollectImages();
            await CollectImageLabels();

        }
     


        private async Task<bool> CheckDeleteTable()
        {
            return await _executerDA.ExecuteDbOperationAsync(async (connection, transaction) =>
                  {
                      string sql = @"SELECT 
                    FIRST 1
                     ID_ITEM
                      FROM TRACER_APP_NATIF
                     ";
                      using FbCommand command = new FbCommand(sql, connection, transaction);
                      try
                      {
                          using FbDataReader reader = await command.ExecuteReaderAsync();
                          return true;
                      }
                      catch
                      {
                          return false;
                      }

                  });

        }


    }
}