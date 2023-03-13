using MFiles.VAF;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Core;
using MFilesAPI;
using System;
using System.Diagnostics;

namespace KM.LibroFirma.V2
{
   /// <summary>
   /// The entry point for this Vault Application Framework application.
   /// </summary>
   /// <remarks>Examples and further information available on the developer portal: http://developer.m-files.com/. </remarks>
   public class VaultApplication
      : ConfigurableVaultApplicationBase<Configuration>
   {
      #region Public Variables
      public ILogger Logger { get; private set; }
      public const string TaskTypeAutoSignDocumentsTask = "TaskType.AutoSignDocumentsTask";
      #endregion
      public VaultApplication()
      {
         // Populate the logger instance.
         this.Logger = LogManager.GetLogger(this.GetType());

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
         this.Logger.Info($"StartApplication STARTED");
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
                  // TODO: trigger event to auto sign documents

                  //TaskQueueBackgroundOperation multiSignOperation = this.MyAutoMultiSignOperation;
                  //DateTime? runAt = new DateTime?();
                  //AutoMultiSignTaskQueueDirective directive = new AutoMultiSignTaskQueueDirective();
                  //directive.ObjVerEx = env.ObjVerEx.ToString();
                  //directive.AutoSignConf = autoSignSettings;
                  //multiSignOperation.RunOnce(runAt, (TaskQueueDirective)directive);
                  break;
               }
            }
         }
      }
   }
}