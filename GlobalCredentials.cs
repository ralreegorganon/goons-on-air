using System;
using System.IO;
using Caliburn.Micro;
using Newtonsoft.Json;
using ServiceReference2;

namespace GoonsOnAir
{
    public class GlobalCredentials : PropertyChangedBase
    {
        public AccessParams AccessParams
        {
            get
            {
                var token = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "OnAir Company", "auth", "accesstoken.dat");
                if (!File.Exists(token)) return null;
                var ap = JsonConvert.DeserializeObject<AccessParams>(File.ReadAllText(token));
                return ap;
            }
        }
    }
}
