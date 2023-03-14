using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Configurations
{
   [DataContract]
   public class MultiSignSettings
   {
      [DataMember(Name = "Base Address Url Service", IsRequired = true)]
      public string BaseAddressUrl { get; set; }
   }
}
