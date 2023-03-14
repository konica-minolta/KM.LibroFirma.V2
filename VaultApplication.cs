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
                        if (this.Configuration != null && this.Configuration.MultiSignConfig != null && this.Configuration.MultiSignConfig.BaseAddressUrl != null)
                        {
                            _clientHttp.BaseAddress = new Uri(this.Configuration.MultiSignConfig.BaseAddressUrl);
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                        }
                        AutoMultiSignTaskQueueDirective _directive = new AutoMultiSignTaskQueueDirective();
                        _directive.ObjVerEx = env.ObjVerEx.ToString();
                        _directive.AutoSignConf = autoSignSettings;
                        this.TaskManager.AddTask(
                     env.Vault,
                     SignBookBackgroundOperationTaskQueueId,
                     TaskTypeAutoSignDocumentsTask,
                     directive: _directive,
                     activationTime:null
                     );
                  break;
               }
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
            this.Logger?.Debug($"List for Download Envelopes to sign: {envelopeFilesToSignList} " );
            this.Logger?.Debug($"Start Task Apply Electronic Signature.");
            foreach (EnvelopeFilesToSign envelopeFilesToSign in envelopeFilesToSignList)
            {
               //this.TaskManager.GetTaskProcessor(SignBookBackgroundOperationTaskQueueId, TaskTypeAutoSignDocumentsTask)
               job.Update( percentComplete: 30, details: "The process Apply Signature is ongoing...");
               //this.operationManager.TaskProcessor.UpdateTaskInfo(job, MFTaskState.MFTaskStateInProgress, "The process Apply Signature is ongoing...", false);
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
                  //this.operationManager.TaskProcessor.UpdateTaskInfo(job, MFTaskState.MFTaskStateInProgress, "The process Auto MultiSign Document is ongoing...", false);
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
               // ...do any work using the transactional vault reference.
            });
            
            //this.operationManager.TaskProcessor.UpdateTaskInfo(job, MFTaskState.MFTaskStateCompleted, "The process Sign Document is completed.", false);
             this.Logger?.Debug($"End Action AutoSign Document: { job.Directive.ObjVerEx}");
         }
         catch (AggregateException ex)
         {
            //this.operationManager.TaskProcessor.UpdateTaskInfo(job, MFTaskState.MFTaskStateFailed, "The process Sign Document is onerror.", false);
            foreach (Exception innerException in ex.InnerExceptions)
               this.Logger?.Error(innerException, $"ActionAutoSignDocument: {innerException.Message}" );

            throw new AppTaskException(TaskProcessingJobResult.Fail, "The process Sign Document is onerror.");
         }
         catch (Exception ex)
         {
            //this.operationManager.TaskProcessor.UpdateTaskInfo(job, MFTaskState.MFTaskStateFailed, "The process Sign Document is onerror.", false);
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