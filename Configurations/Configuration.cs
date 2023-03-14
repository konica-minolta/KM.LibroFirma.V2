using KM.LibroFirma.V2.Configurations;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.JsonAdaptor;
using MFiles.VAF.Configuration.Logging.NLog.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MFiles.VAF.Extensions;
using MFilesAPI;

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
      [JsonConfEditor(Label = "Logging Configurations", Hidden = false)]
      public NLogLoggingConfiguration LoggingConfiguration { get; set; } = new NLogLoggingConfiguration();

      [DataMember]
      [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin)]
      [JsonConfEditor(Label = "Multi Sign Configurations")]
      public MultiSignSettings MultiSignConfig = new MultiSignSettings();
   }


   [DataContract]
   public class AutoSignedTaskQueueDirective : ObjIDTaskDirective
   {
      [DataMember]
      public string ObjVerEx { get; set; }
      [DataMember]
      public int UserIdModifier { get; set; }
      public AutoSignedTaskQueueDirective(ObjID objID, string objVerEx, int userIdModifier) : this()
      {
         if (null == objID)
            throw new ArgumentNullException(nameof(objID));
         this.ObjectID = objID.ID;
         this.ObjectTypeID = objID.Type;
         this.UserIdModifier = userIdModifier;
         this.ObjVerEx = objVerEx;
      }

      public AutoSignedTaskQueueDirective()
      {
      }
   }

    [DataContract]
    public class AutoMultiSignTaskQueueDirective : ObjVerExTaskQueueDirective
    {
        [DataMember]
        public AutoSignSettings AutoSignConf { get; set; }
 
    }

   [DataContract]
   public class ObjVerExTaskQueueDirective : ObjIDTaskDirective
   {
        /// <summary>
        /// Parse-able ObjVerEx string.
        /// </summary>
        [DataMember]
        public string ObjVerEx { get; set; }
        [DataMember]
        public string CurrentUser { get; set; }
   }
}