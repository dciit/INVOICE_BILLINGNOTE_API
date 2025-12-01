namespace INVOICE_VENDER_API.Models
{
    public class DtTable
    {
    }

    public class AuthenRegis
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
    }


    public class DataForConfirmInvoice
    {
        public int No { get; set; }
        public string VenderName { get; set; }
        public string Reference { get; set; }
        public string DocumentNo { get; set; }
        public string PaymentTerms { get; set; }
        public string DocumentDate { get; set; }
        public string PostingDate { get; set; }
        public string NetDueDate { get; set; }
        public string Amount { get; set; }
        public string Vat { get; set; }
        public string TotalAmount { get; set; }
    }
}