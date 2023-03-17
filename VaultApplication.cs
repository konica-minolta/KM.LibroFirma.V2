using KM.LibroFirma.V2.Configurations;
using KM.LibroFirma.V2.Helper;
using KM.LibroFirma.V2.Library;
using MFiles.VAF;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Net.Mail;
using static KM.LibroFirma.V2.Configurations.MailTargetConfiguration;

namespace KM.LibroFirma.V2
{
   /// <summary>
   /// The entry point for this Vault Application Framework application.
   /// </summary>
   /// <remarks>Examples and further information available on the developer portal: http://developer.m-files.com/. </remarks>
   public class VaultApplication
      : MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
   {
      #region Public Variables
      public ILogger Logger { get; private set; }
      [TaskQueue]
      public const string SignBookBackgroundOperationTaskQueueId = "MFiles.SignBookV2.TasksQueue";
      public const string TaskTypeAutoSignDocumentsTask = "TaskType.AutoSignDocumentsTask";
      #endregion
      #region Private Variables
      private static HttpClient _clientHttp = new HttpClient();
      private bool isInEditMode; // used to avoid unattended to process object after checkin event 
      #endregion
      public VaultApplication()
      {
         // Populate the logger instance.
         this.Logger = LogManager.GetLogger(this.GetType());
         if (this.Configuration != null && this.Configuration.MultiSignConfig != null && this.Configuration.MultiSignConfig.BaseAddressUrl != null)
         {
            _clientHttp.BaseAddress = new Uri(this.Configuration.MultiSignConfig.BaseAddressUrl);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
         }

      }
      protected override void StartApplication()
      {
         // Allow the application to start up as normal.
         base.StartApplication();
#if DEBUG
         // Enable logging to any attached debugger, but do not launch the debugger.
         LogManager.EnableLoggingToAttachedDebugger(launchDebugger: false);
#endif
         LogManager.Initialize(this.PermanentVault, this.Configuration?.LoggingConfiguration);
         this.Logger = LogManager.GetLogger(this.GetType());
         if (this.Configuration != null && this.Configuration.MultiSignConfig != null && this.Configuration.MultiSignConfig.BaseAddressUrl != null)
         {
            _clientHttp.BaseAddress = new Uri(this.Configuration.MultiSignConfig.BaseAddressUrl);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
         }
         this.Logger?.Info($"StartApplication STARTED");
      }


      protected override void InitializeTaskManager()
      {
         // Allow the task manager to be initialized.
         base.InitializeTaskManager();

         // If we do not have a task manager then die.
         if (null == this.TaskManager)
            return;
         this.Logger?.Debug($"Initialize TaskManager");

         // Cancel anything that's scheduled.
         this.CancelFutureExecutions(SignBookBackgroundOperationTaskQueueId, TaskTypeAutoSignDocumentsTask);

         // Schedule an execution for now.
         //this.TaskManager.AddTask(this.PermanentVault, SignBookBackgroundOperationTaskQueueId, TaskTypeAutoSignDocumentsTask);
         this.Logger?.Debug($"Initialize TaskManager Done");
      }
      protected override void UninitializeApplication(Vault vault)
      {
         this.Logger?.Info($"Logging stopping");
         LogManager.Shutdown();
         base.UninitializeApplication(vault);
      }
      protected virtual void CancelFutureExecutions(string queueId, string taskType = null)
      {
         // Get all future executions of this type in this queue.
         List<TaskInfo<TaskDirective>> futureExecutions = new List<TaskInfo<TaskDirective>>();
         {
            var query = new TaskQuery();
            query.Queue(queueId);
            if (false == string.IsNullOrWhiteSpace(taskType))
               query.TaskType(taskType);
            query.TaskState(MFTaskState.MFTaskStateWaiting);

            futureExecutions = query
                .FindTasks<TaskDirective>(this.TaskManager);
         }

         // Cancel any future executions.
         foreach (var execution in futureExecutions)
         {
            try
            {
               this.TaskManager.CancelWaitingTask
               (
                   this.PermanentVault,
                   execution.TaskId
               );
            }
            catch
            {
               SysUtils.ReportInfoToEventLog($"Could not cancel task with id {execution.TaskId}");
            }
         }
      }
      protected override void OnConfigurationUpdated(Configuration oldConfiguration, bool isValid, bool updateExternals)
      {
         this.Logger?.Info($"Logging configuration updating");
         // Call the base implementation.
         base.OnConfigurationUpdated(oldConfiguration, isValid, updateExternals);
         LogManager.UpdateConfiguration(this.Configuration?.LoggingConfiguration);

         if (this.Configuration != null && this.Configuration.MultiSignConfig != null && this.Configuration.MultiSignConfig.BaseAddressUrl != null)
         {
            _clientHttp.BaseAddress = new Uri(this.Configuration.MultiSignConfig.BaseAddressUrl);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
         }
         this.Logger?.Info($"Logging configuration updated");
      }

      [EventHandler(MFEventHandlerType.MFEventHandlerAfterCheckInChanges)]
      public void AfterCheckInChanges(EventHandlerEnvironment env)
      {
         ObjectVersionAndProperties versionAndProperties = env.Vault.ObjectOperations.GetObjectVersionAndProperties(env.ObjVer);
         PropertyValue propertyWorkFlow = versionAndProperties.Properties.GetProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow);
         PropertyValue propertyState = versionAndProperties.Properties.GetProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
         if (propertyWorkFlow == null || propertyWorkFlow.Value.IsNULL() || propertyState == null || propertyState.Value.IsNULL())
            return;
         foreach (AutoSignSettings autoSignSettings in this.Configuration.AutoSignsConfig)
         {
            if (propertyWorkFlow.Value.GetLookupID() == autoSignSettings.MF_WF_AutoSign.ID)
            {
               int id = autoSignSettings.MF_ST_AutoSign_SentForSigning.ID;
               if (propertyState.Value.GetLookupID() == id)
               {
                  this.Logger.Info($"Start Auto Sign");
                  // trigger event to auto sign documents
                  if (this.Configuration != null && this.Configuration.MultiSignConfig != null && this.Configuration.MultiSignConfig.BaseAddressUrl != null && _clientHttp.BaseAddress ==null)
                  {
                     _clientHttp.BaseAddress = new Uri(this.Configuration.MultiSignConfig.BaseAddressUrl);
                     System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                  }
                  AutoMultiSignTaskQueueDirective _directive = new AutoMultiSignTaskQueueDirective();
                  _directive.ObjVerEx = env.ObjVerEx.ToString();
                  _directive.AutoSignConf = autoSignSettings;
                  isInEditMode = true;
                  this.TaskManager.AddTask(
                         env.Vault,
                         SignBookBackgroundOperationTaskQueueId,
                         TaskTypeAutoSignDocumentsTask,
                         directive: _directive,
                         activationTime: null
               );
                  break;
               }
            }
         }

         if (this.Configuration.sendMailConfig != null && this.Configuration.sendMailConfig.Enabled && this.Configuration.sendMailConfig.MF_WF_SendEmail != null && this.Configuration.sendMailConfig.MF_ST_SendEmail != null &&
            propertyState.Value.GetLookupID() == this.Configuration.sendMailConfig.MF_ST_SendEmail.ID && propertyWorkFlow.Value.GetLookupID() == this.Configuration.sendMailConfig.MF_WF_SendEmail.ID && !isInEditMode)
         {
            this.Logger.Info($"START Send Mails. Send Mail is Enable.");
            // check configuration settings
            if (!MailHelper.CheckMailConfiguration(this.Configuration,this.Logger))
            {
               return;
            }

            List<string> tempAttachedFiles = new List<string>();
            try
            {
               MailMessage mail = new MailMessage();
               string host = this.Configuration.sendMailConfig.MailTargetConfiguration.Smtp.ServerAddress; // "smtp.mailtrap.io";
               int port = 587;
               if (this.Configuration.sendMailConfig.MailTargetConfiguration.Smtp.Port != 0)
               {
                  port = this.Configuration.sendMailConfig.MailTargetConfiguration.Smtp.Port;
               }
               SmtpClient client = new SmtpClient(host, port);
               client.EnableSsl = this.Configuration.sendMailConfig.MailTargetConfiguration.Smtp.UseEncryptedConnection;

               string subject = "Email From M-Files";
               // add subject
               if (!string.IsNullOrEmpty(this.Configuration.sendMailConfig.MailTargetConfiguration.Subject))
               {
                  subject = this.Configuration.sendMailConfig.MailTargetConfiguration.Subject;
               }
               mail.Subject = subject;
               // add body
               var body = string.Empty;
               if (this.Configuration.sendMailConfig.MailTargetConfiguration.Advanced != null && !string.IsNullOrEmpty(this.Configuration.sendMailConfig.MailTargetConfiguration.Advanced.BodyMessage))
               {
                  body = this.Configuration.sendMailConfig.MailTargetConfiguration.Advanced.BodyMessage;
                  switch (this.Configuration.sendMailConfig.MailTargetConfiguration.Advanced.Encoding)
                  {
                     case "UTF-8":
                        mail.BodyEncoding = System.Text.Encoding.UTF8;
                        mail.SubjectEncoding = System.Text.Encoding.UTF8;
                        break;
                     case "ASCII":
                        mail.BodyEncoding = System.Text.Encoding.ASCII;
                        mail.SubjectEncoding = System.Text.Encoding.ASCII;
                        break;
                     case "Unicode":
                        mail.BodyEncoding = System.Text.Encoding.Unicode;
                        mail.SubjectEncoding = System.Text.Encoding.Unicode;
                        break;
                     default:
                        mail.BodyEncoding = System.Text.Encoding.UTF8;
                        mail.SubjectEncoding = System.Text.Encoding.UTF8;
                        break;
                  }
               }
               mail.Body = body;
               switch (this.Configuration.sendMailConfig.MailTargetConfiguration.Smtp.AuthenticationMode)
               {
                  case SmtpAuthenticationMode.None:
                     client.DeliveryMethod = SmtpDeliveryMethod.Network;
                     client.UseDefaultCredentials = true;
                     break;
                  case SmtpAuthenticationMode.Basic:
                     client.DeliveryMethod = SmtpDeliveryMethod.Network;
                     client.UseDefaultCredentials = false;
                     client.Credentials = new NetworkCredential(this.Configuration.sendMailConfig.MailTargetConfiguration.Smtp.Credentials.AccountName, this.Configuration.sendMailConfig.MailTargetConfiguration.Smtp.Credentials.Password);
                     break;
                  default:
                     client.DeliveryMethod = SmtpDeliveryMethod.Network;
                     client.UseDefaultCredentials = true;
                     break;
               }
               // Add mail address From in config
               MailAddress from = new MailAddress(this.Configuration.sendMailConfig.MailTargetConfiguration.From.Address, this.Configuration.sendMailConfig.MailTargetConfiguration.From.DisplayName);
               mail.From = from;
               //add mail address TO in config
               if (this.Configuration.sendMailConfig.MailTargetConfiguration.To != null && this.Configuration.sendMailConfig.MailTargetConfiguration.To.Count > 0)
               {
                  foreach (MailTargetConfiguration.EmailAddress itemTo in this.Configuration.sendMailConfig.MailTargetConfiguration.To)
                  {
                     if (!string.IsNullOrEmpty(itemTo.Address))
                     {
                        MailAddress to = new MailAddress(itemTo.Address, itemTo.DisplayName);
                        mail.To.Add(to);
                        this.Logger?.Info($"Added Mail address To: {itemTo.Address} - {itemTo.DisplayName}");
                     }
                  }
               }
               //add mail address CC in config
               if (this.Configuration.sendMailConfig.MailTargetConfiguration.CC != null && this.Configuration.sendMailConfig.MailTargetConfiguration.CC.Count > 0)
               {
                  foreach (MailTargetConfiguration.EmailAddress itemCC in this.Configuration.sendMailConfig.MailTargetConfiguration.CC)
                  {
                     if (!string.IsNullOrEmpty(itemCC.Address))
                     {
                        MailAddress CC = new MailAddress(itemCC.Address, itemCC.DisplayName);
                        mail.CC.Add(CC);
                        this.Logger?.Info($"Added Mail address CC: {itemCC.Address} - {itemCC.DisplayName}");
                     }
                  }
               }
               //add mail address BCC in config
               if (this.Configuration.sendMailConfig.MailTargetConfiguration.BCC != null && this.Configuration.sendMailConfig.MailTargetConfiguration.BCC.Count > 0)
               {
                  foreach (MailTargetConfiguration.EmailAddress itemBCC in this.Configuration.sendMailConfig.MailTargetConfiguration.BCC)
                  {
                     if (!string.IsNullOrEmpty(itemBCC.Address))
                     {
                        MailAddress BCC = new MailAddress(itemBCC.Address, itemBCC.DisplayName);
                        mail.Bcc.Add(BCC);
                        this.Logger?.Info($"Added Mail address CC: {itemBCC.Address} - {itemBCC.DisplayName}");
                     }
                  }
               }
               Lookups recipinetsList = env.ObjVerEx.GetProperty(this.Configuration.sendMailConfig.MF_PD_MailRecipintsPropery).Value.GetValueAsLookups();
               foreach (Lookup recipient in recipinetsList)
               {
                  string recipientMailAddress = recipient.ToObjVerEx(env.Vault).GetProperty(this.Configuration.sendMailConfig.MF_PD_MailRecipintAddressProperty).Value.GetValueAsLocalizedText();
                  if (string.IsNullOrEmpty(recipientMailAddress))
                  {
                     this.Logger?.Error($"Mail is empty for: {recipient.DisplayValue}");
                     continue;
                  }
                  string recipinetFullName = string.Empty;
                  if (this.Configuration.sendMailConfig.MF_PD_MailRecipintFullNameProperty != null)
                  {
                     recipinetFullName = recipient.ToObjVerEx(env.Vault).GetProperty(this.Configuration.sendMailConfig.MF_PD_MailRecipintFullNameProperty).Value.GetValueAsLocalizedText();
                  }
                  MailAddress to = new MailAddress(recipientMailAddress, recipinetFullName);
                  mail.To.Add(to);
                  this.Logger?.Info($"Added Mail address To: {recipientMailAddress} - {recipinetFullName}");
               }
               if (mail.To.Count == 0)
               {
                  this.Logger?.Error($"Mail TO is empty! Cannot send any mail. ");
                  return;
               }

               Attachment attachment = null;
               foreach (ObjectFile file in env.ObjVerEx.Info.Files)
               {
                  string tempPathFile = EnvelopeHelper.DownloadDocument(env.Vault, file);
                  tempAttachedFiles.Add(tempPathFile);
                  attachment = new Attachment(tempPathFile);
                  attachment.Name = file.Title + "." + file.Extension;
                  mail.Attachments.Add(attachment);
               }
               try
               {
                  client.Send(mail);
               }
               catch (Exception ex)
               {
                  this.Logger?.Error(ex, $"ERROR to Send mail.");
               }

               attachment.Dispose();
               mail.Dispose();
               this.Logger.Info($"END Send Mails");
            }
            catch (Exception e)
            {

               this.Logger?.Error(e, $"ERROR in Send mail function.");
            }
            finally
            {
               foreach (string itemTempFile in tempAttachedFiles)
               {
                  if (System.IO.File.Exists(itemTempFile))
                  {
                     System.IO.File.Delete(itemTempFile);
                  }
               }
               tempAttachedFiles.Clear();
            }
         }
      }

      [TaskProcessor(SignBookBackgroundOperationTaskQueueId, TaskTypeAutoSignDocumentsTask)]
      [ShowOnDashboard("Auto Sign Process", ShowRunCommand = true)]
      public void ActionAutoMultiSignDocument(ITaskProcessingJob<AutoMultiSignTaskQueueDirective> job)
      {
         List<EnvelopeFilesToSign> envelopeFilesToSignList = new List<EnvelopeFilesToSign>();
         try
         {
            if (string.IsNullOrEmpty(job.Directive.ObjVerEx))
            {
                    job.Update(percentComplete: 100, details: "The process Sign Document is completed.");
                    job.Commit((transactionalVault) =>
                    {
                        // ...do any work using the transactional vault reference.
                    });
                    return;
            }
            this.Logger?.Debug($"Start Action Sign Document: {job.Directive.ObjVerEx}");
            ObjVerEx envelope = ObjVerEx.Parse(job.Vault, job.Directive.ObjVerEx);
            Configurations.AutoSignSettings autoSignConf = job.Directive.AutoSignConf;
            DateTime now = DateTime.Now;
            int num1 = 0;
            int num2 = 0;
            List<string> filesExclusionList = autoSignConf.filesExclusionList;
            foreach (ObjectFile file1 in (IObjectFiles)envelope.Info.Files)
            {
               ObjectFile file = file1;
               bool matchFounded = false;
               foreach (string itemRegExp in filesExclusionList)
               {
                  // Call Matches method for case-insensitive matching.
                  try
                  {
                     foreach (var itemMatch in Regex.Matches(file.GetNameForFileSystem(), itemRegExp, RegexOptions.IgnoreCase))
                     {
                        matchFounded = true;
                        break;
                     }
                  }
                  catch (Exception e)
                  {

                  }
               }
               if (!matchFounded)
               {
                  string str = EnvelopeHelper.DownloadDocument(job.Vault, file);
                  envelopeFilesToSignList.Add(new EnvelopeFilesToSign()
                  {
                     ID = file.ID,
                     Title = file.Title,
                     Extension = file.Extension.ToUpper(),
                     TempPathFile = str
                  });
               }
            }
            this.Logger?.Debug($"List for Download Envelopes to sign: {envelopeFilesToSignList} ");
            this.Logger?.Debug($"Start Task Apply Electronic Signature.");
            foreach (EnvelopeFilesToSign envelopeFilesToSign in envelopeFilesToSignList)
            {
               job.Update(percentComplete: 30, details: "The process Apply Signature is ongoing...");
               ISignatureData signatureData;
               if (envelopeFilesToSign.Extension.ToUpper() == "PDF")
               {
                  signatureData = (ISignatureData)new SignatureDataPdf()
                  {
                     height = autoSignConf.Height,
                     width = autoSignConf.Width
                  };
                  num1 = ((SignatureDataPdf)signatureData).y;
                  num2 = ((SignatureDataPdf)signatureData).x;
               }
               else
                  signatureData = (ISignatureData)new SignatureDataP7M();
               string fileBase64 = CommonUtility.FileToBase64(envelopeFilesToSign.TempPathFile);
               signatureData.ltv = false;
               signatureData.timestamp = false;
               int num3 = 0;
               foreach (Configurations.CertificateProperties certificateGroups in autoSignConf.certificateGroupsList)
               {
                  job.Update(percentComplete: 60, details: "The process Auto MultiSign Document is ongoing...");
                  if (signatureData.GetType().Equals(typeof(SignatureDataPdf)))
                  {
                     ((SignatureDataPdf)signatureData).x = num2;
                     ((SignatureDataPdf)signatureData).y = num1;
                     ((SignatureDataPdf)signatureData).userName = certificateGroups.UserName;
                     ((SignatureDataPdf)signatureData).text = "Firmato digitalmente da " + certificateGroups.UserName + " - In data " + now.ToString("D");
                     ++num3;
                  }
                  signatureData.content = fileBase64;
                  fileBase64 = new MultiSign(certificateGroups.UserName, certificateGroups.Password, certificateGroups.Pin, _clientHttp).SignDocument(signatureData);
                  if (fileBase64 == null)
                     throw new Exception("Error on Sign Document " + envelopeFilesToSign.Title);
               }
               byte[] stream = CommonUtility.Base64ToByte(fileBase64);
               envelopeFilesToSign.SignedPathFile = CommonUtility.SaveTempDocument(stream, envelopeFilesToSign.Extension);
            }
            this.Logger?.Debug($"End Task Apply Auto MultiSign Electronic Signature.");
            envelope.SetProperty(MFAlias.SignDate, MFDataType.MFDatatypeDate, (object)now);

            EnvelopeHelper.ChangeWorkFlowStatus(envelope, MFAlias.WF_WorkFlowFirma, MFAlias.SignStateSigned.ID);
            this.Logger?.Debug($"Start Replace Signed document to M-Files.");
            foreach (EnvelopeFilesToSign envelopeFilesToSign in envelopeFilesToSignList)
               EnvelopeHelper.ReplaceDocument(envelope, envelopeFilesToSign.ID, envelopeFilesToSign.SignedPathFile, new EnvelopeSetting());

            this.Logger?.Debug($"End Replace Signed document to M-Files.");
            job.Update(percentComplete: 100, details: "The process Sign Document is completed.");
            job.Commit((transactionalVault) =>
            {
                // remove the job when all done
                //this.TaskManager.CancelActiveTask(job.Vault, job.TaskInfo.TaskID);
                envelope.CheckOut();
                isInEditMode = false;
                envelope.CheckIn();
                envelope.SaveProperties(null);
            });

            this.Logger?.Debug($"End Action AutoSign Document: { job.Directive.ObjVerEx}");
         }
         catch (AggregateException ex)
         {
            foreach (Exception innerException in ex.InnerExceptions)
               this.Logger?.Error(innerException, $"ActionAutoSignDocument: {innerException.Message}");
            throw new AppTaskException(TaskProcessingJobResult.Fail, "The process Sign Document is onerror.");
         }
         catch (Exception ex)
         {
            this.Logger?.Error(ex, $"ActionAutoSignDocument: {ex.Message}");
            throw new AppTaskException(TaskProcessingJobResult.Fail, "The process Sign Document is onerror.");
         }
         finally
         {
            foreach (EnvelopeFilesToSign envelopeFilesToSign in envelopeFilesToSignList)
            {
               if (System.IO.File.Exists(envelopeFilesToSign.TempPathFile))
                  System.IO.File.Delete(envelopeFilesToSign.TempPathFile);
               if (System.IO.File.Exists(envelopeFilesToSign.SignedPathFile))
                  System.IO.File.Delete(envelopeFilesToSign.SignedPathFile);
            }
         }
      }

   }
}