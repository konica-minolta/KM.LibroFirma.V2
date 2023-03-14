using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Library
{
   [JsonObject(MemberSerialization.OptIn)]
   public class MultiSignData
   {
      [JsonProperty(PropertyName = "AccountCredentials")]
      public AccountCredentials accountCredentials { get; set; }

      [JsonProperty(PropertyName = "SignatureData")]
      public ISignatureData signatureData { get; set; }
   }
}
