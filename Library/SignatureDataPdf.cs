using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Library
{
   public class SignatureDataPdf : ISignatureData
   {
      public string fieldName { get; set; } = string.Empty;
      public int page { get; set; } = 1;
      public int x { get; set; } = 5;
      public int y { get; set; } = 1;
      public int width { get; set; } = 1;
      public int height { get; set; } = 1;
      public string userName { get; set; } = string.Empty;
      public string reason { get; set; } = string.Empty;
      public string location { get; set; } = string.Empty;
      public string dateFormat { get; set; } = "dd/MM/yyyy";
      public string text { get; set; } = string.Empty;
      public int fontSize { get; set; } = 7;
      public string counterSignaturePath { get; set; } = string.Empty;
      public bool graphicalSignature { get; set; } = false;
      public bool ocsp { get; set; } = false;
      public bool cosignCoordinates { get; set; } = true;
      public string timestampCode { get; set; } = string.Empty;
      public int pdfSignatureLayout { get; set; } = 0;
      public string password { get; set; } = string.Empty;
      public int pdfSignatureCertificationLevel { get; set; } = 0;
      public bool cadesDetached { get; set; } = false;
      public string content { get; set; }
      public string digestType { get; set; } = "SHA256";
      public string X509certificate { get; set; } = string.Empty;
      public bool timestamp { get; set; } = false;
      public bool ltv { get; set; } = false;
   }
}
