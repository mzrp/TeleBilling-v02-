using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using TeleBilling_v02_.Models;
using TeleBilling_v02_.Models.Navision;
using TeleBilling_v02_.NAVSalesInvoice;

namespace TeleBilling_v02_.Repository.Navision
{
    public class InvoiceGenerator
    {
        static SalesInvoice_Service_Service salesInvoiceService;

        private static string ConnectToNav()
        {
            string strServiceLogin = "rpnavapi";
            string strServicePassword = "Telefon1";
            string result = string.Empty;
            try
            {
                salesInvoiceService = new SalesInvoice_Service_Service(); //Yes, _Service_Service is intentional
                salesInvoiceService.UseDefaultCredentials = true;
                salesInvoiceService.Credentials = new NetworkCredential(strServiceLogin, strServicePassword);
                result = "True";

            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }

        public static List<string> Bill(IEnumerable<InvoiceModel> billableList)
        {
            List<string> errorMsg = new List<string>();
            string msg = string.Empty;
            if (ConnectToNav() == "True")
            {
                foreach (InvoiceModel invoice in billableList)
                {
                    SalesInvoice_Service order = new SalesInvoice_Service()
                    {
                        Sell_to_Customer_No = invoice.CVR
                    };
                    List<Sales_Invoice_Line> lisInvoiceLine = new List<Sales_Invoice_Line>();
                    foreach (InvoiceLineCollectionModel line in invoice.LineCollections)
                    {
                        //Time period line
                        Sales_Invoice_Line timeSpanLine = new Sales_Invoice_Line();
                        timeSpanLine.Type = NAVSalesInvoice.Type._blank_;
                        timeSpanLine.Description = "Periode " + line.StartDate.ToString("dd/MM/yyyy") + " til " + line.EndDate.ToString("dd/MM/yyyy");
                        timeSpanLine.Total_Amount_Excl_VATSpecified = false;
                        timeSpanLine.Total_Amount_Incl_VATSpecified = false;
                        timeSpanLine.Total_VAT_AmountSpecified = false;
                        lisInvoiceLine.Add(timeSpanLine);

                        //Subscriber range line
                        Sales_Invoice_Line subscriberRangeLine = new Sales_Invoice_Line();
                        subscriberRangeLine.Type = NAVSalesInvoice.Type._blank_;
                        subscriberRangeLine.Description = "Nummerserie " + line.Subscriber_Range_Start + " - " + line.Subscriber_Range_End;
                        subscriberRangeLine.Total_Amount_Excl_VATSpecified = false;
                        subscriberRangeLine.Total_Amount_Incl_VATSpecified = false;
                        subscriberRangeLine.Total_VAT_AmountSpecified = false;
                        lisInvoiceLine.Add(subscriberRangeLine);

                        //Agreement description line(s)
                        if (line.Agreement_Description.Length > 0)
                        {
                            if (line.Agreement_Description.Length < 51)
                            {
                                Sales_Invoice_Line agreementDescriptionLines = new Sales_Invoice_Line();
                                agreementDescriptionLines.Type = NAVSalesInvoice.Type._blank_;
                                agreementDescriptionLines.Description = line.Agreement_Description;
                                agreementDescriptionLines.Total_Amount_Excl_VATSpecified = false;
                                agreementDescriptionLines.Total_Amount_Incl_VATSpecified = false;
                                agreementDescriptionLines.Total_VAT_AmountSpecified = false;
                                lisInvoiceLine.Add(agreementDescriptionLines);
                            }
                            else
                            {
                                int intRunTimesAgreementDescription = 0;
                                while ((double)intRunTimesAgreementDescription < (((double)line.Agreement_Description.Length) / 50)) //Dodgy
                                {
                                    int intRemainingChars = line.Agreement_Description.Length - intRunTimesAgreementDescription * 50;
                                    if (intRemainingChars == 0)
                                    {
                                        break;
                                    }
                                    Sales_Invoice_Line agreementDescriptionLines = new Sales_Invoice_Line();
                                    agreementDescriptionLines.Type = NAVSalesInvoice.Type._blank_;
                                    agreementDescriptionLines.Description = line.Agreement_Description.Substring((intRunTimesAgreementDescription * 50), (((double)intRemainingChars / 50) >= 1 ? 50 : intRemainingChars % 50));
                                    agreementDescriptionLines.Total_Amount_Excl_VATSpecified = false;
                                    agreementDescriptionLines.Total_Amount_Incl_VATSpecified = false;
                                    agreementDescriptionLines.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(agreementDescriptionLines);
                                    intRunTimesAgreementDescription++;
                                }
                            }
                        }

                        foreach (AccumulatedModel zoneLines in line.Accumulated)
                        {
                            if (zoneLines.Minute_price == 0)
                            {
                                string strNewDescription = "Ingen minuttakst for " + zoneLines.ZoneName;
                                if (strNewDescription.Length < 51)
                                {
                                    Sales_Invoice_Line zoneTimeLine = new Sales_Invoice_Line();
                                    zoneTimeLine.Type = NAVSalesInvoice.Type._blank_;
                                    zoneTimeLine.Description = "Ingen minuttakst for " + zoneLines.ZoneName;
                                    zoneTimeLine.Total_Amount_Excl_VATSpecified = false;
                                    zoneTimeLine.Total_Amount_Incl_VATSpecified = false;
                                    zoneTimeLine.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneTimeLine);
                                }
                                else
                                {
                                    Sales_Invoice_Line zoneTimeLine = new Sales_Invoice_Line();
                                    zoneTimeLine.Type = NAVSalesInvoice.Type._blank_;
                                    zoneTimeLine.Description = "Ingen minuttakst for ";
                                    zoneTimeLine.Total_Amount_Excl_VATSpecified = false;
                                    zoneTimeLine.Total_Amount_Incl_VATSpecified = false;
                                    zoneTimeLine.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneTimeLine);
                                    Sales_Invoice_Line zoneTimeLine2 = new Sales_Invoice_Line();
                                    zoneTimeLine2.Type = NAVSalesInvoice.Type._blank_;
                                    zoneTimeLine2.Description = zoneLines.ZoneName;
                                    zoneTimeLine2.Total_Amount_Excl_VATSpecified = false;
                                    zoneTimeLine2.Total_Amount_Incl_VATSpecified = false;
                                    zoneTimeLine2.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneTimeLine2);
                                }
                            }
                            else if (zoneLines.Seconds > 0)
                            {
                                Sales_Invoice_Line zoneTimeLine = new Sales_Invoice_Line();
                                zoneTimeLine.Type = NAVSalesInvoice.Type.Item;
                                zoneTimeLine.No = zoneLines.Minute_No; //Find på nummer
                                zoneTimeLine.Description = zoneLines.ZoneName + " minutter"; //Zone navn + tid

                                zoneTimeLine.Total_Amount_Incl_VATSpecified = false;
                                zoneTimeLine.Total_Amount_Excl_VATSpecified = false;
                                zoneTimeLine.Total_VAT_AmountSpecified = false;
                                zoneTimeLine.Allow_Invoice_Disc = true;
                                zoneTimeLine.Allow_Item_Charge_Assignment = true;

                                //Beregn pris fra zonelines
                                zoneTimeLine.Quantity = Math.Round(Convert.ToDecimal(zoneLines.Seconds) / 60, 2, MidpointRounding.AwayFromZero);
                                zoneTimeLine.Line_Amount = Math.Round((Convert.ToDecimal(zoneLines.Seconds) / 60) * zoneLines.Minute_price, 4, MidpointRounding.AwayFromZero);
                                zoneTimeLine.Total_Amount_Excl_VAT = Math.Round((Convert.ToDecimal(zoneLines.Seconds) / 60) * zoneLines.Minute_price, 4, MidpointRounding.AwayFromZero);
                                zoneTimeLine.Unit_Price = Math.Round((zoneTimeLine.Total_Amount_Excl_VAT / zoneTimeLine.Quantity), 4, MidpointRounding.AwayFromZero);

                                lisInvoiceLine.Add(zoneTimeLine);
                            }
                            else
                            {
                                string strNewDescription = "0 minutter for opkald til " + zoneLines.ZoneName;
                                if (strNewDescription.Length < 51)
                                {
                                    Sales_Invoice_Line zoneTimeLine = new Sales_Invoice_Line();
                                    zoneTimeLine.Type = NAVSalesInvoice.Type._blank_;
                                    zoneTimeLine.Description = "0 minutter for opkald til " + zoneLines.ZoneName;
                                    zoneTimeLine.Total_Amount_Excl_VATSpecified = false;
                                    zoneTimeLine.Total_Amount_Incl_VATSpecified = false;
                                    zoneTimeLine.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneTimeLine);
                                }
                                else
                                {
                                    Sales_Invoice_Line zoneTimeLine = new Sales_Invoice_Line();
                                    zoneTimeLine.Type = NAVSalesInvoice.Type._blank_;
                                    zoneTimeLine.Description = "0 minutter for opkald til ";
                                    zoneTimeLine.Total_Amount_Excl_VATSpecified = false;
                                    zoneTimeLine.Total_Amount_Incl_VATSpecified = false;
                                    zoneTimeLine.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneTimeLine);
                                    Sales_Invoice_Line zoneTimeLine2 = new Sales_Invoice_Line();
                                    zoneTimeLine2.Type = NAVSalesInvoice.Type._blank_;
                                    zoneTimeLine2.Description = zoneLines.ZoneName;
                                    zoneTimeLine2.Total_Amount_Excl_VATSpecified = false;
                                    zoneTimeLine2.Total_Amount_Incl_VATSpecified = false;
                                    zoneTimeLine2.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneTimeLine2);
                                }
                            }

                            //Do zone calls line
                            if (zoneLines.Call_price == 0)
                            {
                                string strNewDescription = "Ingen opkaldsafgift for " + zoneLines.ZoneName;
                                if (strNewDescription.Length < 51)
                                {
                                    Sales_Invoice_Line zoneCallsLine = new Sales_Invoice_Line();
                                    zoneCallsLine.Type = NAVSalesInvoice.Type._blank_;
                                    zoneCallsLine.Description = "Ingen opkaldsafgift for " + zoneLines.ZoneName;
                                    zoneCallsLine.Total_Amount_Excl_VATSpecified = false;
                                    zoneCallsLine.Total_Amount_Incl_VATSpecified = false;
                                    zoneCallsLine.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneCallsLine);
                                }
                                else
                                {
                                    Sales_Invoice_Line zoneCallsLine = new Sales_Invoice_Line();
                                    zoneCallsLine.Type = NAVSalesInvoice.Type._blank_;
                                    zoneCallsLine.Description = "Ingen opkaldsafgift for ";
                                    zoneCallsLine.Total_Amount_Excl_VATSpecified = false;
                                    zoneCallsLine.Total_Amount_Incl_VATSpecified = false;
                                    zoneCallsLine.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneCallsLine);
                                    Sales_Invoice_Line zoneCallsLine2 = new Sales_Invoice_Line();
                                    zoneCallsLine2.Type = NAVSalesInvoice.Type._blank_;
                                    zoneCallsLine2.Description = zoneLines.ZoneName;
                                    zoneCallsLine2.Total_Amount_Excl_VATSpecified = false;
                                    zoneCallsLine2.Total_Amount_Incl_VATSpecified = false;
                                    zoneCallsLine2.Total_VAT_AmountSpecified = false;
                                    lisInvoiceLine.Add(zoneCallsLine2);
                                }
                            }
                            else
                            {
                                Sales_Invoice_Line zoneCallsLine = new Sales_Invoice_Line();
                                zoneCallsLine.Type = NAVSalesInvoice.Type.Item;
                                zoneCallsLine.No = zoneLines.Call_No;
                                zoneCallsLine.Description = zoneLines.ZoneName + " opkald"; //Zone navn + opkald

                                zoneCallsLine.Total_Amount_Incl_VATSpecified = false;
                                zoneCallsLine.Total_Amount_Excl_VATSpecified = false;
                                zoneCallsLine.Total_VAT_AmountSpecified = false;
                                zoneCallsLine.Allow_Invoice_Disc = true;
                                zoneCallsLine.Allow_Item_Charge_Assignment = true;

                                //Beregn pris fra zonelines
                                zoneCallsLine.Quantity = (decimal)zoneLines.styk;
                                zoneCallsLine.Line_Amount = Math.Round((decimal)zoneLines.styk * zoneLines.Call_price, 4, MidpointRounding.AwayFromZero);
                                zoneCallsLine.Total_Amount_Excl_VAT = Math.Round((decimal)zoneLines.styk * zoneLines.Call_price, 4, MidpointRounding.AwayFromZero);
                                zoneCallsLine.Unit_Price = Math.Round((zoneCallsLine.Total_Amount_Excl_VAT / zoneCallsLine.Quantity), 4, MidpointRounding.AwayFromZero);

                                lisInvoiceLine.Add(zoneCallsLine);
                            }

                        }

                        //Filler line
                        Sales_Invoice_Line fillerLine = new Sales_Invoice_Line();
                        fillerLine.Type = NAVSalesInvoice.Type._blank_;
                        fillerLine.Description = "******";
                        fillerLine.Total_Amount_Excl_VATSpecified = false;
                        fillerLine.Total_Amount_Incl_VATSpecified = false;
                        fillerLine.Total_VAT_AmountSpecified = false;
                        lisInvoiceLine.Add(fillerLine);
                    }

                    int i = 0;
                    order.SalesLines = new Sales_Invoice_Line[lisInvoiceLine.Count];
                    foreach (Sales_Invoice_Line line in lisInvoiceLine)
                    {
                        order.SalesLines[i] = new Sales_Invoice_Line();
                        i++;
                    }

                    try
                    {
                        salesInvoiceService.Create(ref order);
                    }
                    catch (Exception ex)
                    {
                        msg = "Error for CVR: " + invoice.CVR + "\n" + ex.Message + "\n" + ex.ToString();
                        errorMsg.Add(msg);
                    }

                    i = 0;
                    foreach (Sales_Invoice_Line line in lisInvoiceLine)
                    {
                        if (line.Type == NAVSalesInvoice.Type.Item)
                        {
                            try
                            {
                                order.SalesLines[i].Description = line.Description;
                                order.SalesLines[i].Type = line.Type;
                                order.SalesLines[i].No = line.No;
                                order.SalesLines[i].Quantity = line.Quantity;
                                order.SalesLines[i].Line_Amount = line.Line_Amount;
                                order.SalesLines[i].Allow_Invoice_Disc = line.Allow_Invoice_Disc;
                                order.SalesLines[i].Allow_Item_Charge_Assignment = line.Allow_Item_Charge_Assignment;
                                order.SalesLines[i].Unit_Price = line.Unit_Price;
                                order.SalesLines[i].Total_Amount_Excl_VAT = line.Total_Amount_Excl_VAT;
                                order.SalesLines[i].Total_Amount_Incl_VATSpecified = line.Total_Amount_Incl_VATSpecified;
                                order.SalesLines[i].Total_Amount_Excl_VATSpecified = line.Total_Amount_Excl_VATSpecified;
                                order.SalesLines[i].Total_VAT_AmountSpecified = line.Total_VAT_AmountSpecified;
                            }
                            catch (Exception ex)
                            {
                                msg = "Error for CVR: " + invoice.CVR + "\n" + ex.Message + "\n" + ex.ToString();
                                errorMsg.Add(msg);
                            }
                        }
                        else if (line.Type == NAVSalesInvoice.Type._blank_)
                        {
                            order.SalesLines[i].Description = line.Description;
                            order.SalesLines[i].Type = line.Type;
                            order.SalesLines[i].Total_Amount_Incl_VATSpecified = line.Total_Amount_Incl_VATSpecified;
                            order.SalesLines[i].Total_Amount_Excl_VATSpecified = line.Total_Amount_Excl_VATSpecified;
                            order.SalesLines[i].Total_VAT_AmountSpecified = line.Total_VAT_AmountSpecified;
                        }
                        i++;
                    }

                    try
                    {
                        salesInvoiceService.Update(ref order);
                        //msg = "done";
                    }
                    catch (Exception ex)
                    {
                        msg = "Error for CVR: " + invoice.CVR + "\n" + ex.Message + "\n" + ex.ToString();
                        errorMsg.Add(msg);
                    }
                    //msg = "Invoice for CVR: " + invoice.CVR + " done.";
                }
            }
            else
            {
                msg = "Can not connect to the NAV";
                errorMsg.Add(msg);
            }

            return errorMsg;
        }
    }
}