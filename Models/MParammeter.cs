namespace INVOICE_VENDER_API.Models
{
    public class MParammeter
    {
    }

    public class RegisRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Role { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EditPassExpire
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }


    public class ConfirmInvoiceRequest
    {
        public string VenderCode { get; set; }
    }

}
