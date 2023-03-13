using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.JsonAdaptor;
using MFiles.VAF.Configuration.Logging.NLog.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KM.LibroFirma.V2
{
   [DataContract]
   public class Configuration
   {

      [DataMember(EmitDefaultValue = false, Order = 10)]
      [JsonConfEditor(HelpText = "Specifies configuration for Automatic signing documents with advanced signature.", IsRequired = false, Label = "Auto Signing Configurations")]
      [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin)]
      public List<AutoSignSettings> AutoSignsConfig = new List<AutoSignSettings>();


      [DataMember(Order = 11)]
      [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_LoggingConfiguration_Label", Hidden = false)]
      public NLogLoggingConfiguration LoggingConfiguration { get; set; } = new NLogLoggingConfiguration();
   }
}