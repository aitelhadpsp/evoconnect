using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;

namespace EvoConnect.Common
{
    public class DbContext
    {

        public readonly SQLiteConnection _connection;
        private const string DatabaseFileName = "evoconnect.db";
        private static readonly string DatabaseFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "dev_connect"
        );
        private  readonly string DatabasePath = Path.Combine(DatabaseFolderPath, DatabaseFileName);

        public DbContext()
        {
            // Ensure the directory exists
            if (!Directory.Exists(DatabaseFolderPath))
            {
                Directory.CreateDirectory(DatabaseFolderPath);
            }

            _connection = new SQLiteConnection(DatabasePath);
            Init();
        }
        public SQLiteConnection Instance()
        {
            var connection = new SQLiteConnection(DatabasePath);
            return connection;

        }

     
        public void Reset()
        {
            var exist = _connection.Table<SyncConfigs>().First();
            exist.DeleteSync = null;
            exist.DoctorsSync = null;
            exist.ImageLabelsSync = null;
            exist.ImageSync = null;
            exist.LabelsSync = null;
            exist.NotesSync = null;
            exist.PatientsSync = null;
            exist.PaymentsSync = null;
            exist.TreatmentsSync = null;
            exist.AppointmentsSync = null;
            
            _connection.Update(exist);
        }

        public void Init()
        {
            _connection.CreateTable<SyncConfigs>();
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            if(item == null)
            {
                _connection.Insert(new SyncConfigs { Id = 1 });
            }


        }

        public DateTime? GetPatientSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item?.PatientsSync;
        }
        public void UpdateFlashEvo(bool active)
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            item.FlashEvo = active;
            _connection.Update(item);
        }       public bool IsFlashEvoActive()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
           return item.FlashEvo;
        }
        public DateTime? GetNoteSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item.NotesSync;
        }
        public int? GetDeleteSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item.DeleteSync;
        }
        public DateTime? GetAppointmentSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item.AppointmentsSync;
        }
        public DateTime? GetImagesSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item.ImageSync;
        }
        public int? GetImageLabelSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item.ImageLabelsSync;
        }
        public DateTime? GetPaymentSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item.PaymentsSync;
        }
        public void UpdatePatientSync(DateTime time)
        {

            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.PatientsSync = time;
            _connection.Update(exist);
        }
        public void UpdateDoctorsSync(DateTime time)
        {

            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.DoctorsSync = time;
            _connection.Update(exist);
        }
        public void UpdateLabelSync(DateTime time)
        {

            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.LabelsSync = time;
            _connection.Update(exist);
        }
        public void UpdateImagesSync(DateTime time)
        {

            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.ImageSync = time;
            _connection.Update(exist);
        }
        public void UpdateAppointmentSync()
        {

            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.AppointmentsSync = DateTime.Now;
            _connection.Update(exist);
        }
        public void UpdatePaymentsSync(DateTime time)
        {

            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.PaymentsSync = time;
            _connection.Update(exist);
        }
        public void UpdateNoteSync(DateTime time)
        {
            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.NotesSync = time;
            _connection.Update(exist);


        }
        public void UpdateDeleteSync(int id)
        {
            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.DeleteSync = id;
            _connection.Update(exist);

        }
        public void UpdateImageLabelsSync(int last)
        {
            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.ImageLabelsSync = last;
            _connection.Update(exist);

        }

  

        public void UpdateTreatmentsSync(DateTime time)
        {
            var exist = _connection.Table<SyncConfigs>().FirstOrDefault();
            exist.TreatmentsSync = time;
            _connection.Update(exist);
        }
        public DateTime? GetTreatmentsSync()
        {
            var item = _connection.Table<SyncConfigs>().FirstOrDefault();
            return item?.TreatmentsSync;
        }


    }
}
