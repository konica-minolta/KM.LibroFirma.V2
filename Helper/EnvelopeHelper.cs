using KM.LibroFirma.V2.Library;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Helper
{
   public static class EnvelopeHelper
   {

      public static string DownloadDocument(Vault vault, ObjectFile file)
      {
         string documentPath = Path.Combine(Path.GetTempFileName() + $".{file.Extension}");
         vault.ObjectFileOperations.DownloadFile(file.ID, file.Version, documentPath);
         return documentPath;
      }

      public static void ChangeWorkFlowStatus(ObjVerEx envelope, MFIdentifier workFlow,  int NewStatus)
      {
         envelope.SetWorkflowState(workFlow, NewStatus);
         envelope.SaveProperties(null);
      }
      public static FileVer ReplaceDocument(ObjVerEx envelope, int oldFileID, string pathNewFile, EnvelopeSetting envelopeSetting)
      {
         ObjectFile file = null;
         envelope.CheckOut();
         ObjectFiles files = envelope.Vault.ObjectFileOperations.GetFilesForModificationInEventHandler(envelope.ObjVer);
         foreach (ObjectFile oldFile in files)
         {
            if (oldFile.ID == oldFileID)
            {
               file = oldFile;
               break;
            }
         }
         string fileName = file.Title.Replace(Path.GetExtension(pathNewFile), "");
         string fileExtension = Path.GetExtension(pathNewFile).Replace(".", "");
         //Change file extension for xades
         if (!(envelopeSetting.OutputType == "PDF" && fileExtension.ToUpper() == "PDF"))
         {
            if (envelopeSetting.OutputType == "XML")
            {
               fileName = string.Format("{0}-signed", file.Title);
            }
            else
            {
               if (fileExtension.ToLower().Contains("p7m"))
               {
                  fileExtension = "p7m";
               }
               else
               {
                  fileExtension += ".p7m";
               }
            }
         }
         envelope.Vault.ObjectFileOperations.RemoveFile(envelope.ObjVer, file.FileVer);
         FileVer fileVer = envelope.Vault.ObjectFileOperations.AddFile(envelope.ObjVer, fileName, fileExtension, pathNewFile);
         envelope.CheckIn();
         return fileVer;
      }
   }
}
