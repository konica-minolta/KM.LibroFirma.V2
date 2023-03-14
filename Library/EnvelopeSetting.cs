using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Library
{
   public class EnvelopeSetting
   {
      public string OutputType { get; set; } = "PDF"; //Corresponding to DisplayValue
      public bool TimeStamp { get; set; } = false;
      public bool SignersCover { get; set; } = false;
      public string SignatureType { get; set; } = "2"; //Qualified Electronic Signature
   }
}
