using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoConnect.Common
{

    static class BaseUrl
    {
        public const string Base = "https://dentalevo.net/";
        public const string Url = Base + "api";
        public const string WsUrl = "wss://dentalevo.net/graphql";
        public const string SyncHub = Base + "hub/sync";
    }
    public static class Api
    {

        public const string GetByToken = BaseUrl.Url + "/partner/";
        public const string Verify = BaseUrl.Url + "/verify";
        public const string Collect = BaseUrl.Url + "/collect";
        public const string synchronized = BaseUrl.Url + "/appointment/syncronized";
        public const string syncPatients = BaseUrl.Url + "/sync/patients";
        public const string syncTreatemant = BaseUrl.Url + "/sync/treatments";
        public const string syncNotes = BaseUrl.Url + "/sync/notes";
        public const string syncPayments = BaseUrl.Url + "/sync/payments";
        public const string syncDoctors = BaseUrl.Url + "/sync/doctors";
        public const string syncLabels = BaseUrl.Url + "/sync/labels";
        public const string syncImages = BaseUrl.Url + "/sync/images";
        public const string syncImageLabels = BaseUrl.Url + "/sync/image/lables";
        public const string syncDeleted = BaseUrl.Url + "/sync/delete";

    }
}
