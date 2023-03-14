using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Library
{
   public interface ISignatureData
   {
      string content { get; set; }
      bool ltv { get; set; }
      bool timestamp { get; set; }
   }
}
