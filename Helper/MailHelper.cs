using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Configuration.Logging;
using static KM.LibroFirma.V2.Configurations.MailTargetConfiguration;

namespace KM.LibroFirma.V2.Helper
{
   public static class MailHelper
   {
      public static bool CheckMailConfiguration(Configuration configuration, ILogger logger)
      {
         bool retVal = true;
         if (configuration.sendMailConfig.MailTargetConfiguration == null)
         {
            logger?.Error($"Mail Sender is not properly configured! Cannot send any mail.");
            retVal = false;
         }

         if (configuration.sendMailConfig.MailTargetConfiguration.From == null || string.IsNullOrEmpty(configuration.sendMailConfig.MailTargetConfiguration.From.Address))
         {
            logger?.Error($"Mail Address From is empty! Cannot send any mail.");
            retVal = false;
         }
         if (configuration.sendMailConfig.MailTargetConfiguration.Smtp == null || string.IsNullOrEmpty(configuration.sendMailConfig.MailTargetConfiguration.Smtp.ServerAddress))
         {
            logger?.Error($"Mail Server Address is empty! Cannot send any mail.");
            retVal = false;
         }
         if (configuration.sendMailConfig.MailTargetConfiguration.Smtp.AuthenticationMode == SmtpAuthenticationMode.Basic && (configuration.sendMailConfig.MailTargetConfiguration.Smtp.Credentials == null ||
               string.IsNullOrEmpty(configuration.sendMailConfig.MailTargetConfiguration.Smtp.Credentials.AccountName) ||
               string.IsNullOrEmpty(configuration.sendMailConfig.MailTargetConfiguration.Smtp.Credentials.Password)))
         {
            logger?.Error($"Mail Server Credentials is empty! Cannot send any mail.");
            retVal = false;
         }
         if (configuration.sendMailConfig.MF_PD_MailRecipintsPropery == null)
         {
            logger?.Error($"Mail Server Recipient property is empty! Cannot send any mail.");
            retVal = false;
         }
         if (configuration.sendMailConfig.MF_PD_MailRecipintAddressProperty == null)
         {
            logger?.Error($"Mail Server mail address property of recipients is empty! Cannot send any mail.");
            retVal = false;
         }
         return retVal;
      }
   }
}
