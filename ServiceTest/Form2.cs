using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceTest
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //string requestText = "";
                //requestText = richTextBox1.Text;

                //var aa = XmlUtils.SerializeToXml(requestText);

                //#region Stok Transfer
                ////List<ZwillingCevaWebService.Models.StockTransfer> list = new List<ZwillingCevaWebService.Models.StockTransfer>();
                ////ZwillingCevaWebService.Models.StockTransfer ss = new ZwillingCevaWebService.Models.StockTransfer();
                ////ss.TransactionType = "1";
                ////ss.StockTransferDate = "20200119";
                ////ss.CevaTransactionNumber = "111";
                ////ss.Detail = new List<ZwillingCevaWebService.Models.StockTransferDetail>();
                ////ss.Detail.Add(new ZwillingCevaWebService.Models.StockTransferDetail { ProductNumber = "070393240", Quantity = 30 });
                ////ss.Detail.Add(new ZwillingCevaWebService.Models.StockTransferDetail { ProductNumber = "750003490", Quantity = 40 });

                ////list.Add(ss);

                ////ss = new ZwillingCevaWebService.Models.StockTransfer();
                ////ss.TransactionType = "2";
                ////ss.StockTransferDate = "20200119";
                ////ss.CevaTransactionNumber = "222";
                ////ss.Detail = new List<ZwillingCevaWebService.Models.StockTransferDetail>();
                ////ss.Detail.Add(new ZwillingCevaWebService.Models.StockTransferDetail { ProductNumber = "396430160", Quantity = 1 });
                ////ss.Detail.Add(new ZwillingCevaWebService.Models.StockTransferDetail { ProductNumber = "396430200", Quantity = 2 });

                ////list.Add(ss);

                //////var doc = XmlUtils.Deserialize<ZwillingCevaWebService.Models.StockTransfer>(requestText);

                ////ZwillingCevaWebService.ZwillingCeva service = new ZwillingCevaWebService.ZwillingCeva();
                ////service.AddStockTransfer(list);
                ////var resp = service.GetGoodReceipt(doc);

                //#endregion

                //List<ZwillingCevaWebService.SAPLayer.OrderDetail> orderDetails = new List<ZwillingCevaWebService.SAPLayer.OrderDetail>();
                //ZwillingCevaWebService.SAPLayer.OrderDetail orderDetail = new ZwillingCevaWebService.SAPLayer.OrderDetail();
                ////List<ZwillingCevaWebService.SAPLayer.GoodIssue> list = new List<ZwillingCevaWebService.SAPLayer.GoodIssue>();
                //ZwillingCevaWebService.SAPLayer.GoodIssue ss = new ZwillingCevaWebService.SAPLayer.GoodIssue();

                //ss.MovementCode = "201";
                //ss.WarehouseCode = "01";
                //ss.OrderNumber = "201-31779";
                //ss.OrderReferenceNumber = "TR-ON-000045610";
                //ss.IntegrationMovementCode = "201";
                //ss.WaybillNumber = "A49903";
                //ss.ToDealerCode = "C99993";
                //ss.WaybillDate = 20210205;

                //orderDetail.LineNumber = "0";
                //orderDetail.Qty = 1;
                //orderDetail.ProductNumber = "798540010";

                //orderDetails.Add(orderDetail);

                //ss.Detail = orderDetails;

                //ZwillingCevaWebService.ZwillingCeva service = new ZwillingCevaWebService.ZwillingCeva();

                //service.GetGoodIssue(null, ss);

                //label1.Text = resp.errorMsg;
            }
            catch (Exception)
            {
            }
        }
    }
}