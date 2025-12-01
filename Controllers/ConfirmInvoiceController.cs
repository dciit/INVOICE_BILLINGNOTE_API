using System.Data;
using INVOICE_VENDER_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;

namespace INVOICE_BILLINGNOTE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfirmInvoiceController : ControllerBase
    {
        private OraConnectDB oOraAL02 = new OraConnectDB("ALPHA02");


        [HttpPost]
        [Route("PostInvoiceRequert")]
        public IActionResult PostInvoiceRequert([FromBody] ConfirmInvoiceRequest obj)
        {
            List<DataForConfirmInvoice> Data_list = new List<DataForConfirmInvoice>();


            OracleCommand sqlSelect = new OracleCommand();
            sqlSelect.CommandText = @"SELECT DISTINCT
                                            D.ROWID,
                                            D.VDTYPE, D.ACTYPE, D.DOCTYPE,
                                            D.IVNO, D.BLDATE, D.VENDER,
                                            D.IVDATE, D.ETDDATE, D.RCDATE,
                                            D.CHQDATE, D.IVDESC, D.VDNAME,
                                            D.RQNAME, D.REMARK, D.RSNO,
                                            D.CURR, D.APAMTC, D.APAMTB,
                                            D.ADJMONTH, D.AMTC, D.AMTB,
                                            D.EXPENSE, D.VATUSE, D.VATCALC,
                                            D.VATIN, D.VATOUT, D.VATACCRUED,
                                            D.TOTAMT, D.SLIPNO, D.CHQNO,
                                            D.FILENO, D.APBIT, D.SLBIT,
                                            D.CHQBIT, D.PAYBIT, D.POSTNO,
                                            D.SEKBN, D.HENKU, D.HENRES,
                                            D.HTANTO, D.CDATE, D.UDATE,
                                            D.INCOTERM, D.ETADATE, D.DOCNO_RUNNO,
                                            D.EXRATE, D.DIVCODE, D.REF_IVNO,
                                            D.REF_VENDER, D.ACBIT
                                        FROM MC.DST_ACDAP1 D
                                        --WHERE VENDER = :VENDER AND ACBIT = '1'
                                        WHERE VENDER = 'SG1415' AND ACBIT = '1'";
            sqlSelect.Parameters.Add(new OracleParameter(":VENDER", obj.VenderCode));
            // sqlSelect.CommandTimeout = 180;
            DataTable dtCC = oOraAL02.Query(sqlSelect);
            if (dtCC.Rows.Count > 0)
            {
                int number = 1;

                foreach (DataRow drow in dtCC.Rows)
                {
                    DataForConfirmInvoice MData = new DataForConfirmInvoice();
                    MData.VenderName = drow["VDNAME"].ToString();
                    MData.Reference = drow["IVDATE"].ToString();
                    MData.DocumentNo = drow["IVNO"].ToString();
                    MData.PaymentTerms = drow["VATCALC"].ToString();
                    MData.DocumentDate = drow["IVDATE"].ToString();
                    MData.PostingDate = drow["BLDATE"].ToString();
                    MData.NetDueDate = drow["CHQDATE"].ToString();
                    MData.Amount = drow["APAMTB"].ToString();
                    MData.Vat = drow["VATIN"].ToString();
                    MData.TotalAmount = drow["TOTAMT"].ToString();
                    MData.No = number++;

                    Data_list.Add(MData);
                }
            }


            return Ok(Data_list);
        }



        [HttpPost]
        [Route("PostInvoiceReport")]
        public IActionResult PostInvoiceReport([FromBody] ConfirmInvoiceRequest obj)
        {
            List<DataForConfirmInvoice> Data_list = new List<DataForConfirmInvoice>();


            OracleCommand sqlSelect = new OracleCommand();
            sqlSelect.CommandText = @"SELECT DISTINCT
                                            D.ROWID,
                                            D.VDTYPE, D.ACTYPE, D.DOCTYPE,
                                            D.IVNO, D.BLDATE, D.VENDER,
                                            D.IVDATE, D.ETDDATE, D.RCDATE,
                                            D.CHQDATE, D.IVDESC, D.VDNAME,
                                            D.RQNAME, D.REMARK, D.RSNO,
                                            D.CURR, D.APAMTC, D.APAMTB,
                                            D.ADJMONTH, D.AMTC, D.AMTB,
                                            D.EXPENSE, D.VATUSE, D.VATCALC,
                                            D.VATIN, D.VATOUT, D.VATACCRUED,
                                            D.TOTAMT, D.SLIPNO, D.CHQNO,
                                            D.FILENO, D.APBIT, D.SLBIT,
                                            D.CHQBIT, D.PAYBIT, D.POSTNO,
                                            D.SEKBN, D.HENKU, D.HENRES,
                                            D.HTANTO, D.CDATE, D.UDATE,
                                            D.INCOTERM, D.ETADATE, D.DOCNO_RUNNO,
                                            D.EXRATE, D.DIVCODE, D.REF_IVNO,
                                            D.REF_VENDER, D.ACBIT
                                        FROM MC.DST_ACDAP1 D
                                        --WHERE VENDER = :VENDER AND ACBIT = '1'
                                        WHERE VENDER = 'SG1415' AND ACBIT = 'F'";
            sqlSelect.Parameters.Add(new OracleParameter(":VENDER", obj.VenderCode));
            // sqlSelect.CommandTimeout = 180;
            DataTable dtCC = oOraAL02.Query(sqlSelect);
            if (dtCC.Rows.Count > 0)
            {
                int number = 1;

                foreach (DataRow drow in dtCC.Rows)
                {
                    DataForConfirmInvoice MData = new DataForConfirmInvoice();
                    MData.VenderName = drow["VDNAME"].ToString();
                    MData.Reference = drow["IVDATE"].ToString();
                    MData.DocumentNo = drow["IVNO"].ToString();
                    MData.PaymentTerms = drow["VATCALC"].ToString();
                    MData.DocumentDate = drow["IVDATE"].ToString();
                    MData.PostingDate = drow["BLDATE"].ToString();
                    MData.NetDueDate = drow["CHQDATE"].ToString();
                    MData.Amount = drow["APAMTB"].ToString();
                    MData.Vat = drow["VATIN"].ToString();
                    MData.TotalAmount = drow["TOTAMT"].ToString();
                    MData.No = number++;

                    Data_list.Add(MData);
                }
            }


            return Ok(Data_list);
        }
    }
}
