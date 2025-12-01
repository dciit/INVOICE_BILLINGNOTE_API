using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using INVOICE_VENDER_API.Contexts;
using INVOICE_VENDER_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace INVOICE_VENDER_API.Controllers
{
    [ApiController]
    [Route("api/authen")]
    public class InvoiceVenderController : ControllerBase
    {
        private ClsHelper oHelper = new ClsHelper();
        private SqlConnectDB dbSCM = new SqlConnectDB("dbSCM");

        [HttpPost]
        [Route("regisuser")]
        public IActionResult Register([FromBody] RegisRequest mParam)
        {
            int res = 0;
            string msg = "";

            if (mParam == null)
            {
                res = -1;
                msg = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return Ok(new { result = res, message = msg });
            }

            string requestRole = (mParam.Role ?? "").Trim().ToUpper();
            string rolRef;
            if (requestRole == "ADMIN") rolRef = "rol_admin";
            else if (requestRole == "ACCOUNTANT") rolRef = "rol_accountant";
            else rolRef = "rol_vender";


            try
            {
                SqlCommand authenCmd = new SqlCommand();
                authenCmd.CommandText = @"
                    SELECT 1 
                    FROM [dbSCM].[dbo].[INV_AuthenRegis] 
                    WHERE USERNAME = @Username AND PASSWORD = @Password AND FNAME = @Fname AND LNAME = @Lname";
                authenCmd.Parameters.AddWithValue("@Username", mParam.Username);
                authenCmd.Parameters.AddWithValue("@Password", mParam.Password);
                authenCmd.Parameters.AddWithValue("@Fname", mParam.Fname);
                authenCmd.Parameters.AddWithValue("@Lname", mParam.Lname);

                DataTable dtauthenRegis = dbSCM.Query(authenCmd);

                if (dtauthenRegis.Rows.Count > 0)
                {
                    res = -2;
                    msg = "มีผู้ใช้งานนี้ในระบบแล้ว";
                    return Ok(new { result = res, message = msg });
                }


                string usercode = oHelper.GenRunningRegis("REG");

                SqlCommand authenregisCmd = new SqlCommand();
                authenregisCmd.CommandText = @"
                    INSERT INTO [dbSCM].[dbo].[INV_AuthenRegis]
                    (USERCODE, USERNAME, PASSWORD, FNAME, LNAME, CRDATE, PASSWORD_EXPIRE, STATUS)
                    VALUES
                    (@Usercode, @Username, @Password, @Fname, @Lname, GETDATE(), @Passwordexpire, @Status)";

                authenregisCmd.Parameters.AddWithValue("@Usercode", usercode);
                authenregisCmd.Parameters.AddWithValue("@Username", mParam.Username);
                authenregisCmd.Parameters.AddWithValue("@Password", mParam.Password);
                authenregisCmd.Parameters.AddWithValue("@Fname", mParam.Fname);
                authenregisCmd.Parameters.AddWithValue("@Lname", mParam.Lname);

                DateTime passwordexp = DateTime.Now.AddMonths(3);
                authenregisCmd.Parameters.AddWithValue("@Passwordexpire", passwordexp);
                authenregisCmd.Parameters.AddWithValue("@Status", "ACTIVE");

                dbSCM.ExecuteCommand(authenregisCmd); 


                SqlCommand roleCmd = new SqlCommand();
                roleCmd.CommandText = @"
                    INSERT INTO [dbSCM].[dbo].[INV_DICT]
                    (DICTTYPE, DICTKEYNO, DICTREFNO)
                    VALUES
                    (@Dicttype, @Dictkeyno, @Dictrefno)";


                roleCmd.Parameters.AddWithValue("@Dicttype", "PV_MSTUSR");
                roleCmd.Parameters.AddWithValue("@Dictkeyno", usercode);
                roleCmd.Parameters.AddWithValue("@Dictrefno", rolRef);

                dbSCM.ExecuteCommand(roleCmd);

                res = 1;
                msg = "success";
            }
            catch (Exception ex)
            {
                res = -3;
                msg = ex.Message;
            }

            return Ok(new { result = res, message = msg });
        }


        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginRequest mParam)
        {
            int res = 0;
            string msg = "";

            if (string.IsNullOrEmpty(mParam.Username) ||
                string.IsNullOrEmpty(mParam.Password))
            {
                res = -1;
                msg = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return Ok(new { result = res, message = msg });
            }

            try
            {
                SqlCommand checkexpirepassCmd = new SqlCommand();
                checkexpirepassCmd.CommandText = @"
                    SELECT PASSWORD_EXPIRE 
                    FROM [dbSCM].[dbo].[INV_AuthenRegis] 
                    WHERE USERNAME = @Username AND PASSWORD = @Password";

                checkexpirepassCmd.Parameters.AddWithValue("@Username", mParam.Username);
                checkexpirepassCmd.Parameters.AddWithValue("@Password", mParam.Password);

                DataTable dtcheckexpirepass = dbSCM.Query(checkexpirepassCmd);

                if (dtcheckexpirepass.Rows.Count == 0)
                {
                    res = -2;
                    msg = "ไม่พบผู้ใช้งานในระบบ";
                    return Ok(new { result = res, message = msg });

                }

                DateTime expirepass = Convert.ToDateTime(dtcheckexpirepass.Rows[0]["PASSWORD_EXPIRE"]);

                if (expirepass <= DateTime.Now)
                {
                    res = -3;
                    msg = "รหัสผ่านของท่านหมดอายุ กรุณาสร้างรหัสใหม่";
                    return Ok(new { result = res, message = msg });
                }

                string token = CreateToken(mParam.Username);
                return Ok(new { result = token });
            }
            catch (Exception ex)
            {
                res = -3;
                msg = ex.Message;
                return Ok(new { result = res, message = msg });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("checkauthen")]
        public IActionResult Checkauthen([FromBody] LoginRequest mParam)
        {
            string tokenKey = "dci.daikin.co.jp";

            string pwd = Encrypt(mParam.Password, tokenKey);

            bool isMatch = (pwd == "KiW1mg/z+3XpGORdA65JxQ==");

            return Ok(new { result = "OK", input = mParam.Password, pwd = pwd, isMatch = isMatch.ToString(), username = mParam.Username});
        }


        private string CreateToken(string username)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };

            string tokenKey = "daikincompressorindustriesthailand";

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private static string Encrypt(string data, string tokenKey)
        {
            string empty = string.Empty;
            try
            {
                Encryptor encryptor = new Encryptor(EncryptionAlgorithm.Rijndael);
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                byte[] bytesKey = (encryptor.IV = Encoding.ASCII.GetBytes(tokenKey));
                byte[] inArray = encryptor.Encrypt(bytes, bytesKey);
                return Convert.ToBase64String(inArray);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string Decrypt(string data, string tokenKey)
        {
            string empty = string.Empty;
            try
            {
                Decryptor decryptor = new Decryptor(EncryptionAlgorithm.Rijndael);
                byte[] inArray = Convert.FromBase64String(data);
                byte[] bytesKey = (decryptor.IV = Encoding.ASCII.GetBytes(tokenKey));
                byte[] bytes = decryptor.Decrypt(inArray, bytesKey);
                return Encoding.ASCII.GetString(bytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("editpassexp")]
        public IActionResult EditPassexp([FromBody] EditPassExpire mParam)
        {
            int res = 0;
            string msg = "";

            if (string.IsNullOrEmpty(mParam.Username) || string.IsNullOrEmpty(mParam.OldPassword))
            {
                res = -1;
                msg = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return Ok(new { result = res, message = msg });
            }

            try
            {
                SqlCommand checkoldpassCmd = new SqlCommand();
                checkoldpassCmd.CommandText = @"
                    SELECT 1 
                    FROM [dbSCM].[dbo].[INV_AuthenRegis]
                    WHERE USERNAME = @Username AND PASSWORD = @OldPassword";
                checkoldpassCmd.Parameters.AddWithValue("@Username", mParam.Username);
                checkoldpassCmd.Parameters.AddWithValue("@OldPassword", mParam.OldPassword);
                DataTable dtcheckoldpass = dbSCM.Query(checkoldpassCmd);

                if (dtcheckoldpass.Rows.Count == 0)
                {
                    res = -2;
                    msg = "ไม่พบ username และ password นี้";
                    return Ok(new { result = res, message = msg });
                }

                SqlCommand editpassexpCmd = new SqlCommand();
                editpassexpCmd.CommandText = @"
                    UPDATE [dbSCM].[dbo].[INV_AuthenRegis] 
                    SET PASSWORD = @NewPassword, PASSWORD_EXPIRE = DATEADD(MONTH, 3, GETDATE())
                    WHERE USERNAME = @Username";
                editpassexpCmd.Parameters.AddWithValue("@NewPassword", mParam.NewPassword);
                editpassexpCmd.Parameters.AddWithValue("@Username", mParam.Username);
                dbSCM.ExecuteCommand(editpassexpCmd);
                res = 1;
                msg = "success";
                return Ok(new { result = res, message = msg });
            }

            catch (Exception ex)
            {
                res = -3;
                msg = ex.Message;
                return Ok(new { result = res, message = msg });
            }


        }
    }
}
