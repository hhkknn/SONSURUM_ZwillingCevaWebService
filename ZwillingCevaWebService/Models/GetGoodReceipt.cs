using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ZwillingCevaWebService.Models
{
    public class GetGoodReceipt
    {
        public Body Body { get; set; } 
        public GetGoodReceipt()
        {
            Body = new Body();
        }
    }

    [XmlRoot(ElementName = "Body")]
    public class Body
    { 
        public Login login { get; set; }

        public List<GoodReceipt> GoodReceipt { get; set; }
    }

    [XmlRoot(ElementName = "Login")]
    public class Login
    {
        [XmlElement(ElementName = "Company")]
        public string Company { get; set; }
        [XmlElement(ElementName = "UserName")]
        public string UserName { get; set; }
        [XmlElement(ElementName = "Password")]
        public string Password { get; set; }
    }


    [XmlRoot(ElementName = "GoodReceipt")]
    public class GoodReceipt
    {
        [XmlElement(ElementName = "AsnNumber")]
        public string AsnNumber { get; set; }
        [XmlElement(ElementName = "AtfNumber")]
        public string AtfNumber { get; set; }
        [XmlElement(ElementName = "CompanyCode")]
        public string CompanyCode { get; set; }
        [XmlElement(ElementName = "CompleteDate")]
        public string CompleteDate { get; set; }
        [XmlElement(ElementName = "CompleteTime")]
        public string CompleteTime { get; set; }
        [XmlElement(ElementName = "DealerCode")]
        public string DealerCode { get; set; }
        [XmlElement(ElementName = "Desi")]
        public string Desi { get; set; }
        [XmlElement(ElementName = "DocumentDate")]
        public string DocumentDate { get; set; }
        [XmlElement(ElementName = "DocumentNumber")]
        public string DocumentNumber { get; set; }
        [XmlElement(ElementName = "ExpirationDate")]
        public string ExpirationDate { get; set; }
        [XmlElement(ElementName = "FacilityCode")]
        public string FacilityCode { get; set; }
        [XmlElement(ElementName = "GIN")]
        public string GIN { get; set; }
        [XmlElement(ElementName = "IntegrationMovementCode")]
        public string IntegrationMovementCode { get; set; }
        [XmlElement(ElementName = "LineNumber")]
        public string LineNumber { get; set; }
        [XmlElement(ElementName = "LotNumber1")]
        public string LotNumber1 { get; set; }
        [XmlElement(ElementName = "LotNumber2")]
        public string LotNumber2 { get; set; }
        [XmlElement(ElementName = "LotNumber3")]
        public string LotNumber3 { get; set; }
        [XmlElement(ElementName = "MachineCode")]
        public string MachineCode { get; set; }
        [XmlElement(ElementName = "MovementCode")]
        public string MovementCode { get; set; }
        [XmlElement(ElementName = "Ownership")]
        public string Ownership { get; set; }
        [XmlElement(ElementName = "PalletNumber")]
        public string PalletNumber { get; set; }
        [XmlElement(ElementName = "PlacementPalletNumber")]
        public string PlacementPalletNumber { get; set; }
        [XmlElement(ElementName = "Plate")]
        public string Plate { get; set; }
        [XmlElement(ElementName = "ProductionDate")]
        public string ProductionDate { get; set; }
        [XmlElement(ElementName = "ProductionPlace")]
        public string ProductionPlace { get; set; }
        [XmlElement(ElementName = "ProductionTime")]
        public string ProductionTime { get; set; }
        [XmlElement(ElementName = "ProductNumber")]
        public string ProductNumber { get; set; }
        [XmlElement(ElementName = "Qty")]
        public string Qty { get; set; }
        [XmlElement(ElementName = "ReceiptNumber")]
        public string ReceiptNumber { get; set; }
        [XmlElement(ElementName = "ReceiptYear")]
        public string ReceiptYear { get; set; }
        [XmlElement(ElementName = "ReferencePalletNumber")]
        public string ReferencePalletNumber { get; set; }
        [XmlElement(ElementName = "ReturnReason")]
        public string ReturnReason { get; set; }
        [XmlElement(ElementName = "SequenceNumber")]
        public string SequenceNumber { get; set; }
        [XmlElement(ElementName = "Shift")]
        public string Shift { get; set; }
        [XmlElement(ElementName = "Sim")]
        public string Sim { get; set; }
        [XmlElement(ElementName = "TotalQty")]
        public string TotalQty { get; set; }
        [XmlElement(ElementName = "Volume")]
        public string Volume { get; set; }
        [XmlElement(ElementName = "WarehouseCode")]
        public string WarehouseCode { get; set; }
        [XmlElement(ElementName = "Weight")]
        public string Weight { get; set; }
        [XmlElement(ElementName = "GoodReceiptProperties")]
        public GoodReceiptProperties GoodReceiptProperties { get; set; }
    }

    [XmlRoot(ElementName = "GoodReceiptProperties")]
    public class GoodReceiptProperties
    {
        [XmlElement(ElementName = "PropertyCode")]
        public int PropertyCode { get; set; }
        [XmlElement(ElementName = "PropertyValue1")]
        public string PropertyValue1 { get; set; }
        [XmlElement(ElementName = "PropertyValue2")]
        public DateTime PropertyValue2 { get; set; }
        [XmlElement(ElementName = "PropertyValue3")]
        public double PropertyValue3 { get; set; }
    }
}