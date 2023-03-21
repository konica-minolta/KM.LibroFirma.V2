using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
//using MFiles.VaultApplications.Logging.NLog;

namespace KM.LibroFirma.V2.Configurations
{
   [DataContract]
   [MFWorkflow(RefMember = "MF_WF_AutoSign")]
   public class AutoSignSettings
   {
      public AutoSignSettings() { }

      [DataMember(Order = 1)]
      [TextEditor(HelpText = "The value of this property will be used as the name of the Auto Sign.", IsRequired = true)]
      public string Name { get; set; }
      [MFWorkflow(AllowEmpty = false, Required = true)]
      [DataMember(Order = 2)]
      [JsonConfEditor(HelpText = "The workflow to be set for documents to be auto signed.", Label = "Auto Sign Workflow")]
      public MFIdentifier MF_WF_AutoSign { get; set; }

      [MFState(AllowEmpty = false, Required = true)]
      [DataMember(Order = 3)]
      [JsonConfEditor(HelpText = "The \"Sent for Auto Signing\" state in the workflow.", Label = "State to send for Auto Signing")]
      public MFIdentifier MF_ST_AutoSign_SentForSigning { get; set; }
      [MFState(AllowEmpty = false, Required = true)]
      [DataMember(Order = 4)]
      [JsonConfEditor(HelpText = "The \"Signed\" state in the workflow.", Label = "State for auto Signed documents ")]
      public MFIdentifier MF_ST_AutoSign_Signed { get; set; }
      [DataMember(Order = 5)]
      [JsonConfEditor(Label = "Height", Hidden = false, DefaultValue = 100)]
      public int Height { get; set; } = 100;
      [DataMember(Order = 6)]
      [JsonConfEditor(Label = "Width", Hidden = false, DefaultValue = 100)]
      public int Width { get; set; } = 100;

      [DataMember(Order = 7)]
      [JsonConfEditor(HelpText = "Specifies configuration for all signers Certificate.", IsRequired = true, Label = "Signers Certificate")]
      public List<CertificateProperties> certificateGroupsList = new List<CertificateProperties>();

      [DataMember(Order = 8)]
      [JsonConfEditor(HelpText = "Specifies regular expressions to match file name to exclude from Auto Sign.", IsRequired = false, Label = "RegEx Files Exclusion")]
      public List<string> filesExclusionList = new List<string>();
   }

   [DataContract]
   public class CertificateProperties
   {
      public CertificateProperties() { }
        [DataMember]
        [TextEditor(HelpText = "Certificate User ", IsRequired = true)]
        public string UserName { get; set; }

        [DataMember]
        [TextEditor(HelpText = "Certificate Password ", IsRequired = true)]
        public string Password { get; set; }

        [DataMember]
        [TextEditor(HelpText = "Certificate Pin ", IsRequired = true)]
        public string Pin { get; set; }
    }

}
