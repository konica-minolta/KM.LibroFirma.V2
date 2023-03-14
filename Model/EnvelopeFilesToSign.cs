using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2
{
   public class EnvelopeFilesToSign
   {
      public int ID { get; set; }
      public string Title { get; set; }
      public string Extension { get; set; }
      public string TempPathFile { get; set; }
      public string SignedPathFile { get; set; }
      public int? CoverPage { get; set; } = null;
      public string GUID { get; set; }
   }
}
