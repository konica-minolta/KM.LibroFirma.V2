using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Library
{
   public class SignatureDataXML : ISignatureData
   {
      public string content { get; set; }
      public string digestType { get; set; } = "SHA256";
      public string X509certificate { get; set; } = string.Empty;
      public bool timestamp { get; set; } = false;
      public bool ltv { get; set; } = false;
      public string detachedReferenceURI { get; set; }
      public string elemenXPath { get; set; }
      public string form { get; set; } = "BES";
      public string signatureId { get; set; }
      public string type { get; set; } = "ENVELOPED";
      public string validationData { get; set; } = "T";
   }
}
