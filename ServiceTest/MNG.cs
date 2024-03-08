using Newtonsoft.Json; 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; 

namespace ServiceTest
{
    public partial class MNG : Form
    {
        public MNG()
        {
            InitializeComponent();
        }

        private void MNG_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = @"{
    ""customerNumber"": ""251871723"",
    ""password"": ""251871723..!!"",
    ""identityType"": 1
}";
            richTextBox2.Text = @"{
  ""order"": {
    ""referenceId"": ""SIPARIS34510"",
    ""barcode"": ""SIPARIS34567"",
    ""billOfLandingId"": ""İrsaliye 1"",
    ""isCOD"": 0,
    ""codAmount"": 0,
    ""shipmentServiceType"": 1,
    ""packagingType"": 1,
    ""content"": ""İçerik 1"",
    ""smsPreference1"": 1,
    ""smsPreference2"": 0,
    ""smsPreference3"": 0,
    ""paymentType"": 1,
    ""deliveryType"": 1,
    ""description"": ""Açıklama 1"",
    ""marketPlaceShortCode"": """",
    ""marketPlaceSaleCode"": """",
    ""pudoId"": """"
  },
  ""orderPieceList"": [
    {
      ""barcode"": ""SIPARIS34567_PARCA1"",
      ""desi"": 0,
      ""kg"": 0,
      ""content"": ""Parça açıklama 1""
    },
    {
      ""barcode"": ""SIPARIS34567_PARCA2"",
      ""desi"": 0,
      ""kg"": 0,
      ""content"": ""Parça açıklama 2""
    }
  ],
  ""recipient"": {
    ""customerId"": 58513278,
    ""refCustomerId"": """",
    ""cityCode"": 0,
    ""cityName"": """",
    ""districtName"": """",
    ""districtCode"": 0,
    ""address"": """",
    ""bussinessPhoneNumber"": """",
    ""email"": """",
    ""taxOffice"": """",
    ""taxNumber"": """",
    ""fullName"": """",
    ""homePhoneNumber"": """",
    ""mobilePhoneNumber"": """"
  }
}";
        }
         
        private async void btnCreateOrderJson_Click(object sender, EventArgs e)
        {
            #region MNGTokenResponse serviste gözükmediğinden hata veriyor bulamadım kapattım
            //AIFCargoWebServices.MNGTokenRequest mngTokenRequest = new AIFCargoWebServices.MNGTokenRequest();
            //AIFCargoWebServices.MNGTokenResponse mngTokenResponse = new AIFCargoWebServices.MNGTokenResponse();
            //AIFCargoWebServices.Error error = new AIFCargoWebServices.Error();
            //List<AIFCargoWebServices.MNGCreateOrderRequest> mngCreateOrderRequest = new List<AIFCargoWebServices.MNGCreateOrderRequest>();
            //List<AIFCargoWebServices.MNGCreateOrderResponse> mngCreateOrderResponse = new List<AIFCargoWebServices.MNGCreateOrderResponse>();

            //#region TOKEN
            //var url = "https://testapi.mngkargo.com.tr/mngapi/api/token";
            //var clientId = "b885d92fdd79c8179c428db771025e7e"; // Müşteri kimliği
            //var clientSecret = "c32ace5b5e662c0dbaf7a7a0ec77a2b9"; // Müşteri gizli anahtarı

            //var requestBody = richTextBox1.Text;

            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Add("X-IBM-Client-Id", clientId);
            //    client.DefaultRequestHeaders.Add("X-IBM-Client-Secret", clientSecret);
            //    //client.DefaultRequestHeaders.Add("x-api-version", "REPLACE_THIS_VALUE");
            //    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            //    var response = await client.PostAsync(url, content);

            //    if (response.StatusCode == HttpStatusCode.OK)
            //    {
            //        var responseContent = response.Content.ReadAsStringAsync();

            //        //mngTokenResponse = JsonConvert.DeserializeObject<AIFCargoWebServices.MNGTokenResponse>(responseContent.Result);

            //    }
            //    else if (response.StatusCode == HttpStatusCode.NotFound)
            //    {
            //        mngTokenResponse.errors.Add(new AIFCargoWebServices.Error
            //        {
            //            code = "404",
            //            description = "Token için Kaynak bulunamadı: " + response.StatusCode,
            //            message = "Sunucu, isteğin hedef kaynağı bulamadı."
            //        });

            //    }
            //    else if (response.StatusCode == HttpStatusCode.Unauthorized)
            //    {
            //        mngTokenResponse.errors.Add(new AIFCargoWebServices.Error
            //        {
            //            code = "401",
            //            description = "CreateOrders için Yetkilendirme hatası:" + response.StatusCode,
            //            message = "Sunucu, isteği yetkilendirme başarısız olduğu için reddetti."
            //        });

            //    }
            //}
            //#endregion

            //#region CREATE ORDER
            //var url2 = "https://testapi.mngkargo.com.tr/mngapi/api/standardcmdapi/createOrder";
            //var clientId2 = "9577d78b27e83bf284774054410a5b39"; // Müşteri kimliği
            //var clientSecret2 = "93e6b79f3fabfb212537f68057572c16"; // Müşteri gizli anahtarı

            //var requestBody2 = richTextBox2.Text;

            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Add("X-IBM-Client-Id", clientId2);
            //    client.DefaultRequestHeaders.Add("X-IBM-Client-Secret", clientSecret2);
            //    //client.DefaultRequestHeaders.Add("x-api-version", "");
            //    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {mngTokenResponse.jwt}");

            //    //var request = new HttpRequestMessage(HttpMethod.Post, url2);
            //    //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 
            //    //request.Content = new StringContent(requestBody2, Encoding.UTF8, "application/json");


            //    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //    var content = new StringContent(requestBody2, Encoding.UTF8, "application/json");

            //    var response = await client.PostAsync(url2, content);

            //    //var response = await client.SendAsync(request);

            //    //var response = await client.PostAsync(url2, request.Content);

            //    if (response.StatusCode == HttpStatusCode.OK)
            //    {
            //        var responseContent = response.Content.ReadAsStringAsync();
            //        //Console.WriteLine("Sunucudan dönen veri: " + responseContent);

            //        mngCreateOrderResponse = JsonConvert.DeserializeObject<List<AIFCargoWebServices.MNGCreateOrderResponse>>(responseContent.Result);

            //    }
            //    else if (response.StatusCode == HttpStatusCode.NotFound)
            //    {
            //        //error.Add(new AIFCargoWebServices.Error
            //        //{
            //        //    code = "404",
            //        //    description = "Create Orders için Kaynak bulunamadı: \" + response.StatusCode",
            //        //    message = "Sunucu, isteğin hedef kaynağı bulamadı."
            //        //});

            //        error.code = "404";
            //        error.description = "Create Orders için Kaynak bulunamadı: " + response.StatusCode;
            //        error.message = "Sunucu, isteğin hedef kaynağı bulamadı.";

            //        mngCreateOrderResponse.Add(new AIFCargoWebServices.MNGCreateOrderResponse
            //        {
            //            errors = new AIFCargoWebServices.Error[] { error }

            //        });
            //    }
            //    else if (response.StatusCode == HttpStatusCode.Unauthorized)
            //    {
            //        error.code = "401";
            //        error.description = "CreateOrders için Yetkilendirme hatası:" + response.StatusCode;
            //        error.message = "Sunucu, isteği yetkilendirme başarısız olduğu için reddetti.";

            //        mngCreateOrderResponse.Add(new AIFCargoWebServices.MNGCreateOrderResponse
            //        {
            //            errors= new AIFCargoWebServices.Error[] { error }

            //        });
            //    }
            //    else if (response.StatusCode == HttpStatusCode.BadRequest)
            //    {
            //        error.code = "400";
            //        error.description = "CreateOrders için istek hatası:" + response.StatusCode;
            //        error.message = "Sunucu, isteği hatalı olduğu için reddetti.";

            //        mngCreateOrderResponse.Add(new AIFCargoWebServices.MNGCreateOrderResponse
            //        {
            //            errors = new AIFCargoWebServices.Error[] { error }

            //        });
            //    }
            //    else if (response.StatusCode == HttpStatusCode.InternalServerError)
            //    {
            //        error.code = "500";
            //        error.description = "CreateOrders için istek hatası:" + response.StatusCode;
            //        error.message = "Sunucu, isteği server hatalı olduğu için reddetti.";

            //        mngCreateOrderResponse.Add(new AIFCargoWebServices.MNGCreateOrderResponse
            //        {
            //            errors =new AIFCargoWebServices.Error[] { error }

            //        });
            //    }

            //}
            //#endregion 
            #endregion
        }
        public static void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\MNG_CEVA_KARGO";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\MNG_CEVA_KARGO\\MngCevaKargoLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
