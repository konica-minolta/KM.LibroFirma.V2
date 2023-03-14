using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2.Library
{
   public class MultiSign
   {
      private AccountCredentials AccountCredentials;
      private HttpClient client;
      public MultiSign(string UserName, string Password, string Pin, HttpClient Client)
      {
         AccountCredentials = new AccountCredentials()
         {
            userid = UserName,
            password = Password,
            signaturePassword = Pin
         };
         client = Client;
      }
      public string SignDocument(ISignatureData signatureData)
      {
         string signedSignature = null;
         if (signatureData.GetType().Equals(typeof(SignatureDataPdf)))
         {
            signedSignature = SignPDFAsync(signatureData).Result;
         }
         else if (signatureData.GetType().Equals(typeof(SignatureDataP7M)))
         {
            signedSignature = SignP7MAsync(signatureData).Result;
         }
         else
         {
            signedSignature = SignXMLAsync(signatureData).Result;
         }
         return signedSignature;
      }

      private async Task<string> SignPDFAsync(ISignatureData signatureData)
      {
         string signedSignature = null;
         SignatureDataPdf signatureDataPdf = (SignatureDataPdf)signatureData;
         MultiSignData multiSignData = new MultiSignData()
         {
            accountCredentials = AccountCredentials,
            signatureData = signatureDataPdf
         };
         string jsonString = JsonConvert.SerializeObject(multiSignData);
         var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
         string uriString = client.BaseAddress.ToString() + "/signPDF";
         // HTTP POST  
         HttpResponseMessage response = await client.PostAsync(uriString, content);

         // Verification  
         if (response.IsSuccessStatusCode)
         {
            // Reading Response. 
            dynamic responseJson = JsonConvert.DeserializeObject<ExpandoObject>(response.Content.ReadAsStringAsync().Result);
            if ((bool)responseJson.success)
            {
               signedSignature = responseJson.signature;
            }
         }
         return signedSignature;
      }
      private async Task<string> SignP7MAsync(ISignatureData signatureData)
      {
         string signedSignature = null;
         SignatureDataP7M signatureDataP7M = (SignatureDataP7M)signatureData;
         MultiSignData multiSignData = new MultiSignData()
         {
            accountCredentials = AccountCredentials,
            signatureData = signatureDataP7M
         };

         string jsonString = JsonConvert.SerializeObject(multiSignData);
         jsonString = jsonString.Replace("SignatureData", "SignatureDataP7M");
         var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
         string uriString = client.BaseAddress.ToString() + "/signP7M";
         // HTTP POST  
         HttpResponseMessage response = await client.PostAsync(uriString, content);
         // Verification  
         if (response.IsSuccessStatusCode)
         {
            // Reading Response. 
            dynamic responseJson = JsonConvert.DeserializeObject<ExpandoObject>(response.Content.ReadAsStringAsync().Result);
            if ((bool)responseJson.success)
            {
               signedSignature = responseJson.signature;
            }
         }
         return signedSignature;
      }
      private async Task<string> SignXMLAsync(ISignatureData signatureData)
      {
         string signedSignature = null;
         SignatureDataXML signatureDataXML = (SignatureDataXML)signatureData;
         MultiSignData multiSignData = new MultiSignData()
         {
            accountCredentials = AccountCredentials,
            signatureData = signatureDataXML
         };
         string jsonString = JsonConvert.SerializeObject(multiSignData);
         jsonString = jsonString.Replace("SignatureData", "SignatureDataXML");
         var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
         string uriString = client.BaseAddress.ToString() + "/signXML";
         // HTTP POST  
         HttpResponseMessage response = await client.PostAsync(uriString, content);
         // Verification  
         if (response.IsSuccessStatusCode)
         {
            // Reading Response. 
            dynamic responseJson = JsonConvert.DeserializeObject<ExpandoObject>(response.Content.ReadAsStringAsync().Result);
            if ((bool)responseJson.success)
            {
               signedSignature = responseJson.signature;
            }
         }
         return signedSignature;
      }
   }
}
