using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Library
{
   public class SignatureDataP7M : ISignatureData
   {
      public string content { get; set; } = string.Empty;
      public string digestType { get; set; } = "SHA256";
      public string X509certificate { get; set; } = string.Empty;
      public bool timestamp { get; set; } = false;
      public bool ltv { get; set; } = false;
      public string p7m { get; set; } = "SHA256";
      public string counterSignaturePath { get; set; } = string.Empty;
   }
}
