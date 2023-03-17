using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Validation;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Configurations
{
   [DataContract]
   [MFWorkflow(RefMember = "MF_WF_SendEmail", Required = false, AllowEmpty = true)]
   public class SendMailConfiguration
   {
      public SendMailConfiguration() { }

      // Summary:
      //     Indicates whether Mails sender is enabled or not.
      [DataMember(Order = 0)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Enabled_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Enabled_HelpText", DefaultValue = false)]
      [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
      public bool Enabled { get; set; }


      [MFWorkflow(Required = false, AllowEmpty = true)]
      [DataMember(Order = 1)]
      [JsonConfEditor(HelpText = "The workflow to be set for send eMail", Label = "Send Email Workflow", Hidden = true, ShowWhen = ".parent._children{.key == 'Enabled' && .value == true }")]
      //[RequireWhen(true, new[] { typeof(RequireWhenMailTargetEnabled) })]
      public MFIdentifier MF_WF_SendEmail { get; set; }

      [MFState(Required = false, AllowEmpty = true)]
      [DataMember(Order = 2)]
      [JsonConfEditor(HelpText = "The \"state\"  in the workflow.", Label = "State to send Email", Hidden = true, ShowWhen = ".parent._children{.key == 'Enabled' && .value == true }")]
      //[RequireWhen(true, new[] { typeof(RequireWhenMailTargetEnabled) })]
      public MFIdentifier MF_ST_SendEmail { get; set; }

      [MFPropertyDef(Required = false, AllowEmpty = true, Datatypes = new MFDataType[] { MFDataType.MFDatatypeLookup, MFDataType.MFDatatypeMultiSelectLookup })]
      [DataMember(Order = 3)]
      [JsonConfEditor(Label = "Property of mail recipents", HelpText = "The email property to get the recipints", Hidden = true, ShowWhen = ".parent._children{.key == 'Enabled' && .value == true }")]
      public MFIdentifier MF_PD_MailRecipintsPropery { get; set; }

      [MFPropertyDef(Required = false, AllowEmpty = true, Datatypes = new MFDataType[] { MFDataType.MFDatatypeText })]
      [DataMember(Order = 4)]
      [JsonConfEditor(Label = "Property of mail Address", HelpText = "The email property to get the address of recipents", Hidden = true, ShowWhen = ".parent._children{.key == 'Enabled' && .value == true }")]
      public MFIdentifier MF_PD_MailRecipintAddressProperty { get; set; }

        [MFPropertyDef(Required = false, AllowEmpty = true, Datatypes = new MFDataType[] { MFDataType.MFDatatypeText })]
        [DataMember(Order = 5)]
        [JsonConfEditor(Label = "Property full name of mail Address", HelpText = "The Full name property for emailto get the address of recipents", Hidden = true, ShowWhen = ".parent._children{.key == 'Enabled' && .value == true }")]
        public MFIdentifier MF_PD_MailRecipintFullNameProperty { get; set; }

        [DataMember(Order = 6)]
      [JsonConfEditor(Label = "Mail Configuration", HelpText = "Configuration mail parameters", Hidden = true, ShowWhen = ".parent._children{.key == 'Enabled' && .value == true }")]
      [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
      public MailTargetConfiguration MailTargetConfiguration { get; set; } = new MailTargetConfiguration();

   }

   //
   // Summary:
   //      mail target configuration.
   [DataContract]

   public class MailTargetConfiguration
   {
      //
      // Summary:
      //     Constructor.
      public MailTargetConfiguration() { }

      //
      // Summary:
      //      email's subject format.
      [DataMember(Order = 1000)]
      [JsonConfEditor(DefaultValue = "Mail event from ${application-name}", Label = "$$KM_LibroFirma_V2_Configuration_MailTargetConfiguration_Subject_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Mail_Subject_HelpText\n\n$$DefaultValue_Label event from ${application-name}\n\n$$KM_LibroFirma_V2_Configuration_Mail_LayoutRenderers_HelpText", Hidden = true, ShowWhen = ".parent.parent._children{.key == 'Enabled' && .value == true }")]
      public string Subject { get; set; }
      //
      // Summary:
      //     email's sender.
      [DataMember(Order = 1001)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Mail_From_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Mail_From_HelpText", Hidden = true, ShowWhen = ".parent.parent._children{.key == 'Enabled' && .value == true }")]
      public EmailAddress From { get; set; }
      //
      // Summary:
      //     email's recipient.
      [DataMember(Order = 1002)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Mail_To_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Mail_To_HelpText", ChildName = "$$KM_LibroFirma_V2_Configuration_Mail_EmailAddress_ChildName", Hidden = true, ShowWhen = ".parent.parent._children{.key == 'Enabled' && .value == true }")]
      public List<EmailAddress> To { get; set; }
      //
      // Summary:
      //      email's carbon copy recipients.
      [DataMember(Order = 1003)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Mail_CC_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Mail_CC_HelpText", ChildName = "$$KM_LibroFirma_V2_Configuration_Mail_EmailAddress_ChildName", Hidden = true, ShowWhen = ".parent.parent._children{.key == 'Enabled' && .value == true }")]
      public List<EmailAddress> CC { get; set; }
      //
      // Summary:
      //      email's blind carbon copy recipients.
      [DataMember(Order = 1004)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Mail_BCC_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Mail_BCC_HelpText", ChildName = "$$KM_LibroFirma_V2_Configuration_Mail_EmailAddress_ChildName", Hidden = true, ShowWhen = ".parent.parent._children{.key == 'Enabled' && .value == true }")]
      public List<EmailAddress> BCC { get; set; }
      //
      // Summary:
      //     Smtp settings for sending  emails.
      [DataMember(Order = 1005)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Mail_Smtp_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Mail_Smtp_HelpText")]
      public SmtpConfiguration Smtp { get; set; }
      //
      // Summary:
      //     Advanced email settings.
      [DataMember(Order = int.MaxValue)]
      [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Advanced_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Advanced_HelpText", Hidden = true, ShowWhen = ".parent.parent._children{.key == 'Enabled' && .value == true }")]
      [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
      public AdvancedConfiguration Advanced { get; set; }



      //
      // Summary:
      //     Smtp settings for sending emails.
      [DataContract]
      public class SmtpConfiguration
      {

         //
         // Summary:
         //     The default value for Port.
         public const int DefaultPort = 587;

         public SmtpConfiguration() { }


         //
         // Summary:
         //     The remote host / server address to send through.
         [DataMember]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Smtp_ServerAddress_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Smtp_ServerAddress_HelpText")]

         public string ServerAddress { get; set; }
         //
         // Summary:
         //     If true, will attempt to connect using an encrypted connection.
         [DataMember]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Smtp_UseEncryptedConnection_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Smtp_UseEncryptedConnection_HelpText", DefaultValue = true, HideWhen = ".parent._children{ .key == 'UseLocalPickupFolder' && .value === true }")]
         public bool UseEncryptedConnection { get; set; }
         //
         // Summary:
         //     The port to connect with.
         [DataMember]
         [JsonConfIntegerEditor(Label = "$$KM_LibroFirma_V2_Configuration_Smtp_Port_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Smtp_Port_HelpText", DefaultValue = DefaultPort, Min = 1, Max = 65535, HideWhen = ".parent._children{ .key == 'UseLocalPickupFolder' && .value === true }")]
         public int Port { get; set; } = DefaultPort;
         //
         // Summary:
         //     If true then the remote server requires authentication to send through.
         [DataMember]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Smtp_AuthenticationMode_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Smtp_AuthenticationMode_HelpText", DefaultValue = SmtpAuthenticationMode.None, HideWhen = ".parent._children{ .key == 'UseLocalPickupFolder' && .value === true }")]
         public SmtpAuthenticationMode AuthenticationMode { get; set; }
         //
         // Summary:
         //     The authentication credentials to use
         //     is Basic is true.
         [DataMember]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Smtp_Credentials_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Smtp_Credentials_HelpText", Hidden = true, ShowWhen = ".parent{ ._children{.key == 'AuthenticationMode' && .value == 'Basic' } && ( !._children{ .key == 'UseLocalPickupFolder' }.value || ._children{.key == 'UseLocalPickupFolder' }.value === false ) }")]
         public Credentials Credentials { get; set; }


      }
      //
      // Summary:
      //     SMTP authentication modes.
      public enum SmtpAuthenticationMode
      {
         //
         // Summary:
         //     No authentication.
         None = 0,
         //
         // Summary:
         //     Basic - username and password.
         Basic = 1,
         //
         // Summary:
         //     NTLM Authentication.
         Ntlm = 2
      }
      //
      // Summary:
      //     Represents an email address to send to, or send from.
      [DataContract]
      [JsonConfEditor(NameMember = "DisplayName")]

      public class EmailAddress
      {
         //
         // Summary:
         //     Creates an EmailAddress with no EmailAddress.Address
         //     populated. Usage of an EmailAddress without an address will throw an exception.
         public EmailAddress() { }
         //
         // Summary:
         //     Creates an EmailAddress
         //     instance with just the EmailAddress.Address
         //     property populated.
         //
         // Parameters:
         //   address:
         //     The email address of the user.
         //
         // Remarks:
         //     The email address will be checked to see whether it is null or whitespace on
         //     creation, but no other checks are made. The mail library may do additional checks
         //     when the message is created.
         public EmailAddress(string address) { this.Address = address; }
         //
         // Summary:
         //     Creates an EmailAddress
         //     instance with just the EmailAddress.Address
         //     property populated.
         //
         // Parameters:
         //   address:
         //     The email address of the user.
         //
         //   displayName:
         //     The display name for the user.
         //
         // Remarks:
         //     The email address will be checked to see whether it is null or whitespace on
         //     creation, but no other checks are made. The mail library may do additional checks
         //     when the message is created.
         public EmailAddress(string address, string displayName) { this.Address = address; this.DisplayName = displayName; }

         //
         // Summary:
         //     The actual email address of the user.
         [DataMember]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_EmailAddress_Address_Label", HelpText = "$$NKM_LibroFirma_V2_Configuration_EmailAddress_Address_HelpText")]
         public string Address { get; set; }
         //
         // Summary:
         //     The display name for the user.
         [DataMember]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_EmailAddress_DisplayName_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_EmailAddress_DisplayName_HelpText")]
         public string DisplayName { get; set; }
      }
      //
      // Summary:
      //     Configuration settings for authentication to the Smtp server.
      [DataContract]

      public class Credentials
      {
         public Credentials() { }

         //
         // Summary:
         //     The account name to connect as.
         [DataMember(Order = 0)]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Credentials_AccountName_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Credentials_AccountName_HelpText")]
         public string AccountName { get; set; }
         //
         // Summary:
         //     The password for the account.
         [DataMember(Order = 1)]
         [JsonConfEditor(Label = "$$KM_LibroFirma_V2_Configuration_Credentials_Password_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Credentials_Password_HelpText")]
         [Security(IsPassword = true)]
         public string Password { get; set; }
      }
      //
      // Summary:
      //     Advanced configuration for mail targets.
      [DataContract]

      public class AdvancedConfiguration
      {
         public AdvancedConfiguration() { }
         //
         // Summary:
         //     Email message body format for the event entry.
         [DataMember(Order = 0)]
         [MultilineTextEditor]
         [JsonConfEditor(DefaultValue = "${message}${newline}", Label = "$$KM_LibroFirma_V2_Configuration_BodyMessage_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_BodyMessage_Helptext\n\n$$DefaultValue_LabelBody${message}${newline}\n\n$$KM_LibroFirma_V2_Configuration_BodyMessageRenderers_HelpText")]
         [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
         public string BodyMessage { get; set; }
         //
         // Summary:
         //     Indicates whether the email message body is html or plain text.
         [DataMember(Order = 1)]
         [JsonConfEditor(DefaultValue = false, Label = "$$KM_LibroFirma_V2_Configuration_Html_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Html_HelpText")]
         [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
         public bool Html { get; set; }
         //
         // Summary:
         //     Indicates whether new lines should be replaced with html line break tags (<br>).
         [DataMember(Order = 2)]
         [JsonConfEditor(DefaultValue = false, Label = "$$KM_LibroFirma_V2_Configuration_ReplaceNewlineWithBrTagInHtml_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_ReplaceNewlineWithBrTagInHtml_HelpText", Hidden = true, ShowWhen = ".parent._children{ .key == 'Html' && .value === true }")]
         [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
         public bool ReplaceNewlineWithBrTagInHtml { get; set; }
         //
         // Summary:
         //     The character encoding to use for the email message body.
         [DataMember(Order = 3)]
         [JsonConfEditor(TypeEditor = "options", DefaultValue = "UTF-8", Label = "$$KM_LibroFirma_V2_Configuration_Encoding_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Encoding_HelpText", Options = "{selectOptions:[\"UTF-8\",\"ASCII\",\"Unicode\"]}")]
         [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
         public string Encoding { get; set; }
         ////
         //// Summary:
         ////     Header message to include at the beginning of all emails before the message layout.
         //[DataMember(Order = 4)]
         //[MultilineTextEditor(Label = "$$KM_LibroFirma_V2_Configuration_Header_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Header_HelpText")]
         //[Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
         //public string Header { get; set; }
         ////
         //// Summary:
         ////     Header message to include at the end of all emails before the message layout.
         //[DataMember(Order = 5)]
         //[MultilineTextEditor(Label = "$$KM_LibroFirma_V2_Configuration_Footer_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_Footer_HelpText")]
         //[Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
         //public string Footer { get; set; }
         //
         // Summary:
         //     Indicates whether new lines should be added between messages.
         //
         // Remarks:
         //     Currently hidden because it seems currently only a single message is sent per
         //     email.
         [DataMember(Order = 6)]
         [JsonConfEditor(DefaultValue = false, Label = "$$KM_LibroFirma_V2_Configuration_AddNewLinesBetweenMessages_Label", HelpText = "$$KM_LibroFirma_V2_Configuration_AddNewLinesBetweenMessages_HelpText", Hidden = true)]
         [Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
         public bool AddNewLinesBetweenMessages { get; set; }
      }
   }

   public class RequireWhenMailTargetEnabled : IConditionalRequirement
   {
      public RequireWhenMailTargetEnabled() { }

      //
      // Summary:
      //     Human readable description when the setting is required. Only the actual condition
      //     is needed, as the framework puts it in a sentence.
      public string Description { get; }
      //
      // Summary:
      //     JsPath describing the condition, used on client for local validation.
      public string JsPath { get; }

      //
      // Summary:
      //     Condition checker on server, used by server validation.
      //
      // Parameters:
      //   conf:
      //     The setting's configuration object.
      //
      // Returns:
      //     True if the requiring conditions are met.
      public bool IsRequired(object conf)
      {
         return ((Configuration)conf).sendMailConfig.Enabled;
      }
   }
}
