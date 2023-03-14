using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Helper
{
   public static class CommonUtility
   {
      public static string FileToBase64(string filePath)
      {
         byte[] bytes = File.ReadAllBytes(filePath);
         return Convert.ToBase64String(bytes);
      }

      public static Byte[] Base64ToByte(string fileBase64)
      {
         return Convert.FromBase64String(fileBase64);
      }
      public static byte[] ObjectToByteArray(object obj)
      {
         if (obj == null)
            return null;
         BinaryFormatter bf = new BinaryFormatter();
         using (MemoryStream ms = new MemoryStream())
         {
            bf.Serialize(ms, obj);
            return ms.ToArray();
         }
      }

      public static string SaveTempDocument(Byte[] stream, string fileExtension)
      {
         string documentPath = Path.Combine(Path.GetTempFileName() + $".{fileExtension}");
         File.WriteAllBytes(documentPath, stream);
         return documentPath;
      }

      public static void DeleteFile(string filePath)
      {
         File.Delete(filePath);
      }

      public static List<string> GetPlaceHolderArray(string source)
      {
         string pattern = @"{{[a-z_.*]+}}";
         var matches = from Match match in Regex.Matches(source, pattern)
                       select match.Groups[1].Value;
         return matches.ToList();
      }
   }
}
