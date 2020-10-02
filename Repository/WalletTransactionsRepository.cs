using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

using Hook.Enums;
using Hook.Models;

namespace Hook.Repository
{
    public class WalletTransactionsRepository
    {
        private string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();
        TransactionRepository transactionRepository = new TransactionRepository();
        CustomerRepository customerRepository = new CustomerRepository();

        public ReverseTransactionResult ReverseWalletTransaction(long transactionId, string updatedByUsername)
        {
            int reversalTransactionTypId = (int)TransactionTypes.ReversalTransaction;
            DateTime transactionDate = GetRealDate();

            try
            {
                string loweredUsername = updatedByUsername.ToLower();

                //if (!_transactionAppSettings.ReversalTransactionActive)
                //    return ReverseTransactionResult.TransactionReversalDisabled;

                MasterTransactionRecord transactionToReverse = null;

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    transactionToReverse = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE MasterTransactionRecordId=@MasterTransactionRecordId", new { MasterTransactionRecordId = transactionId }).SingleOrDefault();
                    connection.Close();
                }

                if (transactionToReverse == null)
                {
                    throw new Exception(string.Format("TRN0011 - The TransactionId '{0}' does not exist", transactionId));
                }

                if (transactionToReverse != null)
                {
                    ReversibleTransaction reversibleTransaction = null;

                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();

                        reversibleTransaction = connection.Query<ReversibleTransaction>("SELECT * FROM ReversibleTransaction WHERE TransactionTypeId=@TransactionTypeId", new { TransactionTypeId = transactionToReverse.TransactionTypeId }).SingleOrDefault();
                        connection.Close();
                    }

                    if (reversibleTransaction == null)
                    {
                        return ReverseTransactionResult.TransactionNotReversible;
                    }

                    // is this tranction type allowed to be reversed
                    if (reversibleTransaction.IsReversible)
                    {
                        //is transaction within the allowable reversal timespan
                        //if ((DateTime.Now.Subtract(TimeSpan.FromMinutes(Convert.ToDouble(reversibleTransaction.ReversiblePeriod))) > transactionToReverse.TransactionDate))
                        //{
                        //TODO implement charges for transaction reversal
                        string masterTransactionRecordString = transactionToReverse.MasterTransactionRecordId.ToString();
                        int count = 0;

                        using (var connection = new SqlConnection(sqlConnectionString))
                        {
                            connection.Open();

                            count = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE TransactionTypeId=@TransactionTypeId AND Text4=@Text4", new { TransactionTypeId = reversalTransactionTypId, Text4 = masterTransactionRecordString }).ToList().Count();
                            connection.Close();
                        }

                        if (count > 0)
                        {
                            return ReverseTransactionResult.TransactionCannotBeReversedTwice;
                        }

                        MasterTransactionRecord newReversalTransaction = new MasterTransactionRecord();
                        newReversalTransaction.TransactionTypeId = reversalTransactionTypId;
                        newReversalTransaction.ShortDescription = transactionToReverse.ShortDescription + "(Rv)";
                        newReversalTransaction.TransactionReference = transactionToReverse.TransactionReference + "(Rv)";
                        newReversalTransaction.PayeeId = transactionToReverse.PayeeId;
                        newReversalTransaction.PayerId = transactionToReverse.PayerId;
                        newReversalTransaction.PayeePaymentInstrumentId = transactionToReverse.PayeePaymentInstrumentId;
                        newReversalTransaction.PayerPaymentInstrumentId = transactionToReverse.PayerPaymentInstrumentId;
                        //newReversalTransaction.TransactioErrorCodeId = 1;
                        newReversalTransaction.Amount = transactionToReverse.Amount + transactionToReverse.Fee;
                        newReversalTransaction.Fee = 0;
                        newReversalTransaction.Tax = 0;
                        newReversalTransaction.IsBankingTransaction = transactionToReverse.IsBankingTransaction;
                        newReversalTransaction.IsTestTransaction = transactionToReverse.IsTestTransaction;
                        newReversalTransaction.FITransactionCode = transactionToReverse.FITransactionCode;
                        newReversalTransaction.TransactionDate = transactionDate;
                        newReversalTransaction.Text = ReversalTransactionNarrative(transactionToReverse.Text);
                        newReversalTransaction.Text2 = transactionToReverse.Fee.ToString();
                        newReversalTransaction.Text3 = transactionToReverse.Text3;
                        newReversalTransaction.Text4 = transactionToReverse.MasterTransactionRecordId.ToString();
                        newReversalTransaction.Text5 = "Transaction ID: " + transactionToReverse.MasterTransactionRecordId;
                        newReversalTransaction.Text6 = loweredUsername;
                        newReversalTransaction.AccessChannelId = 1;
                        newReversalTransaction.ExternalApplicationId = 1;
                        newReversalTransaction.CustomerTypeId = transactionToReverse.CustomerTypeId;
                        newReversalTransaction.TransactionStatusId = TransactionState.Successful;
                        newReversalTransaction.ReversedTransactionOriginalTypeId = transactionToReverse.TransactionTypeId;


                        decimal senderBalanceBeforeTransaction = BalanceEnquiry(transactionToReverse.PayeeId, transactionToReverse.PayeePaymentInstrumentId, string.Empty, false);
                        decimal receiverBalanceBeforeTransaction = BalanceEnquiry(transactionToReverse.PayerId, transactionToReverse.PayerPaymentInstrumentId, string.Empty, false);

                        long actualReversalAmount = newReversalTransaction.Amount;
                        long actualCreditAmount = newReversalTransaction.Amount;

                        if (transactionToReverse.TransactionTypeId == (int)TransactionTypes.AirtimeSale || transactionToReverse.TransactionTypeId == (int)TransactionTypes.AirtimeTopup || transactionToReverse.TransactionTypeId == (int)TransactionTypes.LoyaltyPointBalanceEnquiry || transactionToReverse.TransactionTypeId == (int)TransactionTypes.LoyaltyPointRedemption || transactionToReverse.TransactionTypeId == (int)TransactionTypes.LoyaltyPointTransfer || transactionToReverse.TransactionTypeId == (int)TransactionTypes.WithholdingTax)
                        {
                            actualReversalAmount = newReversalTransaction.Amount - (transactionToReverse.Fee * 2);
                            actualCreditAmount = newReversalTransaction.Amount - transactionToReverse.Fee;

                            newReversalTransaction.Amount = newReversalTransaction.Amount - transactionToReverse.Fee;
                        }
                        else
                        {
                            actualReversalAmount = actualReversalAmount - transactionToReverse.Fee;
                        }

                        HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, actualReversalAmount, 0);

                        newReversalTransaction.PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString());
                        newReversalTransaction.PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString());


                        using (var connection = new SqlConnection(sqlConnectionString))
                        {
                            connection.Open();
                            var affectedRows = connection.Execute("INSERT INTO MasterTransactionRecord (Amount, Fee, IsTestTransaction,ExternalApplicationId,AccessChannelId,ShortDescription,PayerId,PayeeId,PayerPaymentInstrumentId,PayeePaymentInstrumentId,Tax,Text,Text4,TransactionErrorCodeId,TransactionDate,TransactionTypeId,TransactionReference,CustomerTypeId,DestinationUserName,PayerBalanceBeforeTransaction,PayeeBalanceBeforeTransaction,TransactionStatusId) VALUES (@Amount, @Fee, @IsTestTransaction,@ExternalApplicationId,@AccessChannelId,@ShortDescription,@PayerId,@PayeeId,@PayerPaymentInstrumentId,@PayeePaymentInstrumentId,@Tax,@Text,@Text4,@TransactionErrorCodeId,@TransactionDate,@TransactionTypeId@TransactionReference,@CustomerTypeId,@DestinationUserName,@PayerBalanceBeforeTransaction,@PayeeBalanceBeforeTransaction,@TransactionStatusId)", new { newReversalTransaction.Amount, newReversalTransaction.Fee, newReversalTransaction.IsTestTransaction, newReversalTransaction.ExternalApplicationId, newReversalTransaction.AccessChannelId, newReversalTransaction.ShortDescription, newReversalTransaction.PayerId, newReversalTransaction.PayeeId, newReversalTransaction.PayerPaymentInstrumentId, newReversalTransaction.PayeePaymentInstrumentId, newReversalTransaction.Tax, newReversalTransaction.Text, newReversalTransaction.Text4, newReversalTransaction.TransactionErrorCodeId, newReversalTransaction.TransactionDate, newReversalTransaction.TransactionTypeId, newReversalTransaction.TransactionReference, newReversalTransaction.CustomerTypeId, newReversalTransaction.DestinationUserName, newReversalTransaction.PayerBalanceBeforeTransaction, newReversalTransaction.PayeeBalanceBeforeTransaction, newReversalTransaction.TransactionStatusId });

                            connection.Close();
                        }

                        using (var connection = new SqlConnection(sqlConnectionString))
                        {
                            connection.Open();

                            newReversalTransaction = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE TransactionDate=@TransactionDate AND PayeeId=@PayeeId", new { TransactionDate = newReversalTransaction.TransactionDate, PayeeId = newReversalTransaction.PayeeId }).SingleOrDefault();

                            connection.Close();
                        }

                        transactionToReverse.TransactionStatusId = TransactionState.Reversed;

                        //Undo any fees (and commissions) that could have been charged for this transaction
                        ReverseGLEntries(transactionToReverse, newReversalTransaction);

                        // context.SaveChanges();

                        PaymentInstrument payerPaymentInstrument = transactionRepository.GetPaymentInstrumentByPaymentInstrumentId(newReversalTransaction.PayerPaymentInstrumentId);
                        PaymentInstrument payeePaymentInstrument = transactionRepository.GetPaymentInstrumentByPaymentInstrumentId(newReversalTransaction.PayeePaymentInstrumentId);

                        HelperRepository.UpdateCustomerSVABalance(payerPaymentInstrument, actualReversalAmount, 0, false);
                        HelperRepository.UpdateCustomerSVABalance(payeePaymentInstrument, actualCreditAmount, 0, true);

                        decimal senderBalanceAfterTransaction = BalanceEnquiry(newReversalTransaction.PayerId, newReversalTransaction.PayerPaymentInstrumentId, string.Empty, false);
                        decimal receiverBalanceAfterTransaction = BalanceEnquiry(newReversalTransaction.PayeeId, newReversalTransaction.PayeePaymentInstrumentId, string.Empty, false);

                        newReversalTransaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
                        newReversalTransaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

                        //reverse daily cumulated amounts
                        if (reversibleTransaction.IsCumulativeAmountReversible)
                        {
                            new HelperRepository().UpdateCumulativeCustomerTransactionLimit(newReversalTransaction.PayeeId, payeePaymentInstrument, transactionToReverse.Amount * -1);
                        }


                        using (var connection = new SqlConnection(sqlConnectionString))
                        {
                            connection.Open();
                            var affectedRows = connection.Execute("UPDATE MasterTransactionRecord SET PayerBalanceAfterTransaction=@PayerBalanceAfterTransaction, PayeeBalanceAfterTransaction=@PayeeBalanceAfterTransaction WHERE MasterTransactionRecordId = @MasterTransactionRecordId", new { PayerBalanceAfterTransaction = newReversalTransaction.PayerBalanceAfterTransaction, PayeeBalanceAfterTransaction = newReversalTransaction.PayeeBalanceAfterTransaction, MasterTransactionRecordId = newReversalTransaction.MasterTransactionRecordId });

                            connection.Close();
                        }

                        return ReverseTransactionResult.TransactionBookedForReversal;
                    }
                    else
                    {
                        return ReverseTransactionResult.TransactionNotReversible;
                    }
                }
                else
                {
                    return ReverseTransactionResult.NoSuchTransaction;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void ReverseGLEntries(MasterTransactionRecord originalTransaction, MasterTransactionRecord reversalTransaction)
        {
            try
            {
                DateTime transactionDate = GetRealDate();

                List<CustomerTransactionGL> customerGlRecords = null;

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    customerGlRecords = connection.Query<CustomerTransactionGL>("SELECT * FROM MasterTransactionRecord WHERE MasterTransactionRecordId=@MasterTransactionRecordId", new { MasterTransactionRecordId = originalTransaction.MasterTransactionRecordId }).ToList();

                    connection.Close();
                }

                foreach (CustomerTransactionGL customerTransactionGL in customerGlRecords)
                {
                    CustomerTransactionGL newCustomerTransactionGL = new CustomerTransactionGL
                    {
                        CustomerId = customerTransactionGL.CustomerId,
                        MasterTransactionRecordId = reversalTransaction.MasterTransactionRecordId,
                        TransactionDate = transactionDate
                    };

                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("INSERT INTO CustomerTransactionGL (CustomerId, MasterTransactionRecordId, TransactionDate) VALUES (@CustomerId, @MasterTransactionRecordId, @TransactionDate)", new { customerTransactionGL.CustomerId, reversalTransaction.MasterTransactionRecordId, transactionDate });

                        connection.Close();
                    }
                }

                //List<AgentTransactionGL> agentTransactionGLs = context.AgentTransactionGLs.Where(x => x.MasterTransactionRecordId == originalTransaction.MasterTransactionRecordId).ToList();

                //foreach (AgentTransactionGL agentTransactionGL in agentTransactionGLs)
                //{
                //    AgentTransactionGL newAgentTransactionGL = new AgentTransactionGL
                //    {
                //        CommssionEarned = 0 - agentTransactionGL.CommssionEarned,
                //        MasterTransactionRecordId = reversalTransaction.MasterTransactionRecordId,
                //        //OperatorCustomerId= agen
                //        TillCustomerId = agentTransactionGL.TillCustomerId,
                //        TransactionDate = transactionDate,
                //    };
                //    context.AgentTransactionGLs.Add(newAgentTransactionGL);
                //}

                List<BillerTransactionGL> billerTransactionGls = null;

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    billerTransactionGls = connection.Query<BillerTransactionGL>("SELECT * FROM BillerTransactionGL WHERE MasterTransactionRecordId=@MasterTransactionRecordId", new { MasterTransactionRecordId = originalTransaction.MasterTransactionRecordId }).ToList();

                    connection.Close();
                }

                foreach (BillerTransactionGL billerTransactionGL in billerTransactionGls)
                {
                    BillerTransactionGL newBillerTransactionGL = new BillerTransactionGL
                    {
                        TransactionDate = transactionDate,
                        CommssionEarned = 0 - billerTransactionGL.CommssionEarned,
                        Amount = billerTransactionGL.Amount,
                        BillerId = billerTransactionGL.BillerId,
                        MasterTransactionRecordId = reversalTransaction.MasterTransactionRecordId,
                        PaymentInstrumentId = billerTransactionGL.PaymentInstrumentId
                    };

                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("INSERT INTO BillerTransactionGL (TransactionDate,CommssionEarned, Amount,BillerId,MasterTransactionRecordId,PaymentInstrumentId) VALUES (@TransactionDate, @CommssionEarned, @Amount,@BillerId,@MasterTransactionRecordId,@PaymentInstrumentId)", new { newBillerTransactionGL.TransactionDate, newBillerTransactionGL.CommssionEarned, newBillerTransactionGL.Amount, newBillerTransactionGL.BillerId, newBillerTransactionGL.MasterTransactionRecordId, newBillerTransactionGL.PaymentInstrumentId });

                        connection.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string ReversalTransactionNarrative(string originalString)
        {
            try
            {
                string[] splitters = new string[] { "From:", "To:" };
                string[] newStrings = originalString.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

                string a = newStrings[0].Replace("-", string.Empty);
                string b = string.Concat(newStrings[1], " - ");
                return originalString = splitters[0] + b + splitters[1] + a;
            }
            catch
            {
                return originalString;
            }
        }

        public void MatchPaymentInstrumentToCustomer(long customerId, long paymentInstrumentId, out Customer customer, out PaymentInstrument paymentInstrument)
        {
            paymentInstrument = null;
            customer = null;

            if (customerId != 0)
            {
                customer = customerRepository.GetCustomerByCustomerId(customerId);

                if (customer == null)
                {
                    throw new Exception(string.Format("CA0004 - CustomerID '{0}' does not exist", customerId));
                }
            }

            if (paymentInstrumentId != 0)
            {
                paymentInstrument = transactionRepository.GetPaymentInstrumentByPaymentInstrumentId(paymentInstrumentId);

                if (paymentInstrument == null)
                {
                    throw new Exception(string.Format("PI0001 - Payment Instrument '{0}' doesn’t exist", paymentInstrumentId));
                }
            }
            if (customerId != 0 && paymentInstrumentId != 0)
            {
                if (paymentInstrument.CustomerId != customerId)
                {
                    throw new Exception(string.Format("PI0005 - PaymentInstrumentId '{0}' is not Owned by CustomerId '{1}'", paymentInstrumentId, customerId));
                }
            }
        }

        public decimal BalanceEnquiry(long customerId, long paymentInstrumentId, string accountPin, bool chargedRequest)
        {
            decimal customerBalance = -1;
            Customer customer = null;
            PaymentInstrument customerPaymentInstrument = null;

            string allParams = string.Empty;

            if (paymentInstrumentId == 0)
            {
                throw new Exception(string.Format("Customer Payment Instrument not Set"));
                //customer = customerRepository.GetCustomerByCustomerId(customerId);
                //paymentInstrumentId = GetDefaultPaymentInstrument(customerId, customer.CustomerTypeId).PaymentInstrumentId;
            }

            MatchPaymentInstrumentToCustomer(customerId, paymentInstrumentId, out customer, out customerPaymentInstrument);

            try
            {
                if (!customerPaymentInstrument.IsMobileWallet && !chargedRequest)
                {
                    throw new Exception(string.Format("PI0016 - The PaymentInstrumentId '{0}' must be Charged for this transaction because it is not of Wallet Origin.", paymentInstrumentId));
                }

                if (customerPaymentInstrument.IsMobileWallet && !chargedRequest)
                {
                    return Convert.ToDecimal(customerPaymentInstrument.AccountBalance);
                }

                int transactionTypeId = (int)TransactionTypes.BalanceEnquiry;
                DateTime transactionTime = GetRealDate();
                string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

                //bool defaultTransactionFee = false;
                //long tariffItemId = 0;

                PaymentInstrument operatorTransactionsPaymentInstrument = null;
                Customer operatorTransactionCustomer = customerRepository.GetOperatorTransactionsUser(out operatorTransactionsPaymentInstrument);

                //long balanceEnquiryFee = tariffRepository.GetTariffCharge((PITypes)customerPaymentInstrument.PaymentInstrumentTypeId, (int)AccessChannels.USSD, (int)TransactionTypes.WalletBalanceEnquiry, 0, out defaultTransactionFee, out tariffItemId);

                //customerBalance = customerPaymentInstrument.AccountBalance - balanceEnquiryFee;

                customerBalance = customerPaymentInstrument.AccountBalance;

                decimal senderBalanceBeforeTransaction = BalanceEnquiry(customerId, paymentInstrumentId, string.Empty, false);
                decimal receiverBalanceBeforeTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

                allParams = string.Format("CustomerId: {0}<br> PI: {1} <br> Customer Balance (Kes): {2} ", customerId, paymentInstrumentId, senderBalanceBeforeTransaction / 100);

                if (senderBalanceBeforeTransaction == 0)
                {
                    //balanceEnquiryFee = 0;
                    customerBalance = 0;
                }
                //else
                //{
                //    //HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, 0, balanceEnquiryFee);
                //}

                MasterTransactionRecord transactionRecord = new MasterTransactionRecord
                {
                    PayerId = customerId,
                    PayeeId = operatorTransactionCustomer.CustomerId,
                    Amount = 0,
                    Fee = 0,
                    PayeePaymentInstrumentId = operatorTransactionsPaymentInstrument.PaymentInstrumentId,
                    PayerPaymentInstrumentId = paymentInstrumentId,
                    ExternalApplicationId = 1,
                    AccessChannelId = 1,
                    IsTestTransaction = customer.IsTestCustomer,
                    Tax = 0,
                    ShortDescription = "BALANCE_ENQUIRY",
                    Text = null,
                    // TransactioErrorCodeId = 1,
                    TransactionDate = DateTime.Now,
                    TransactionReference = transactionReference,
                    TransactionTypeId = transactionTypeId,
                    CustomerTypeId = customer.CustomerTypeId,
                    PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
                    PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
                    TransactionStatusId = TransactionState.Successful
                };

                CustomerTransactionGL senderTransactionGL = new CustomerTransactionGL
                {
                    CustomerId = customerId,
                    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
                    TransactionDate = transactionTime
                };

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("INSERT INTO CustomerTransactionGL (CustomerId, MasterTransactionRecordId, TransactionDate) VALUES (@CustomerId, @MasterTransactionRecordId, @TransactionDate)", new { senderTransactionGL.CustomerId, senderTransactionGL.MasterTransactionRecordId, senderTransactionGL.TransactionDate });

                    connection.Close();
                }

                CustomerTransactionGL receiverTransactionGL = new CustomerTransactionGL
                {
                    CustomerId = operatorTransactionCustomer.CustomerId,
                    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
                    TransactionDate = transactionTime
                };

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("INSERT INTO CustomerTransactionGL (CustomerId, MasterTransactionRecordId, TransactionDate) VALUES (@CustomerId, @MasterTransactionRecordId, @TransactionDate)", new { receiverTransactionGL.CustomerId, receiverTransactionGL.MasterTransactionRecordId, receiverTransactionGL.TransactionDate });

                    connection.Close();
                }

              //  const long agentCommission = 0;

                //FeesGL feeGl = new FeesGL
                //{
                //    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
                //    CustomerFeeAmount = balanceEnquiryFee,
                //    TransactionCommission = agentCommission,
                //    GrossRevenue = balanceEnquiryFee - agentCommission,
                //    TransactionDate = transactionTime,
                //    TransactionDescription = BALANCE_ENQUIRY
                //};

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("INSERT INTO MasterTransactionRecord (Amount, Fee, IsTestTransaction,ExternalApplicationId,AccessChannelId,ShortDescription,PayerId,PayeeId,PayerPaymentInstrumentId,PayeePaymentInstrumentId,Tax,Text,Text4,TransactionErrorCodeId,TransactionDate,TransactionTypeId,TransactionReference,CustomerTypeId,DestinationUserName,PayerBalanceBeforeTransaction,PayeeBalanceBeforeTransaction,TransactionStatusId) VALUES (@Amount, @Fee, @IsTestTransaction,@ExternalApplicationId,@AccessChannelId,@ShortDescription,@PayerId,@PayeeId,@PayerPaymentInstrumentId,@PayeePaymentInstrumentId,@Tax,@Text,@Text4,@TransactionErrorCodeId,@TransactionDate,@TransactionTypeId@TransactionReference,@CustomerTypeId,@DestinationUserName,@PayerBalanceBeforeTransaction,@PayeeBalanceBeforeTransaction,@TransactionStatusId)", new { transactionRecord.Amount, transactionRecord.Fee, transactionRecord.IsTestTransaction, transactionRecord.ExternalApplicationId, transactionRecord.AccessChannelId, transactionRecord.ShortDescription, transactionRecord.PayerId, transactionRecord.PayeeId, transactionRecord.PayerPaymentInstrumentId, transactionRecord.PayeePaymentInstrumentId, transactionRecord.Tax, transactionRecord.Text, transactionRecord.Text4, transactionRecord.TransactionErrorCodeId, transactionRecord.TransactionDate, transactionRecord.TransactionTypeId, transactionRecord.TransactionReference, transactionRecord.CustomerTypeId, transactionRecord.DestinationUserName, transactionRecord.PayerBalanceBeforeTransaction, transactionRecord.PayeeBalanceBeforeTransaction, transactionRecord.TransactionStatusId });

                    connection.Close();
                }

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    transactionRecord = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE TransactionDate=@TransactionDate AND PayeeId=@PayeeId", new { TransactionDate = transactionRecord.TransactionDate, PayeeId = transactionRecord.PayeeId }).SingleOrDefault();

                    connection.Close();
                }

                if (senderBalanceBeforeTransaction != 0)
                {
                    HelperRepository.UpdateCustomerSVABalance(customerPaymentInstrument, 0, 0, false);
                    HelperRepository.UpdateCustomerSVABalance(operatorTransactionsPaymentInstrument, 0, 0, true);
                }

                decimal senderBalanceAfterTransaction = BalanceEnquiry(customerId, paymentInstrumentId, string.Empty, false);
                decimal receiverBalanceAfterTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

                transactionRecord.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
                transactionRecord.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());


                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("UPDATE MasterTransactionRecord SET PayerBalanceAfterTransaction=@PayerBalanceAfterTransaction, PayeeBalanceAfterTransaction=@PayeeBalanceAfterTransaction WHERE MasterTransactionRecordId = @MasterTransactionRecordId", new { PayerBalanceAfterTransaction = transactionRecord.PayerBalanceAfterTransaction, PayeeBalanceAfterTransaction = transactionRecord.PayeeBalanceAfterTransaction, MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId });

                    connection.Close();
                }

                //Parallel.Invoke(() =>
                //{
                //    if (senderBalanceBeforeTransaction != 0)
                //    {
                //        Task.Factory.StartNew(() => messageRepository.BalanceEnquirySms(customer, transactionTime, transactionRecord.MasterTransactionRecordId, transactionReference, customerBalance, customerPaymentInstrument.LoyaltyPointBalance))
                //        .ContinueWith(previousTask =>
                //        {
                //            previousTask.Exception.Handle(ex =>
                //            {
                //                return true;
                //            });
                //        }, TaskContinuationOptions.OnlyOnFaulted);
                //    }
                //});

                return customerBalance;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DateTime GetRealDate()
        {
            //Set the time zone information to E. Africa Standard Time 
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            //Get date and time in US Mountain Standard Time 
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

            return dateTime;
        }


        //public PaymentInstrument GetDefaultPaymentInstrument(long customerId, int customerType)
        //{
        //    try
        //    {
        //        //if (customerType == (int)CustomerTypes.Agent)
        //        //{
        //        //    AgentOutlet outlet = agentRepository.GetOutletByCustomerId(customerId);

        //        //    if (outlet == null)
        //        //    {
        //        //        throw new Exception(String.Format("AG0002 - The specified tillCustomerId '{0}' does not match any till", customerId));
        //        //    }

        //        //    return context.PaymentInstruments.Find(outlet.PaymentInstrumentId);
        //        //}

        //        //if (customerType == (int)CustomerTypes.Biller)
        //        //{
        //        //    BillPaymentRepository billerRepository = new BillPaymentRepository();
        //        //    List<Biller> billers = billerRepository.GetBillersByCustomerId(customerId);

        //        //    return context.PaymentInstruments.Find(billers[0].OperationPaymentInstrumentId);
        //        //}

        //        PaymentInstrument paymentInstrument = context.PaymentInstruments.FirstOrDefault(x => x.CustomerId == customerId && x.PaymentInstrumentTypeId == (int)PITypes.MPESA && x.Delinked == false && x.IsActive == true);
        //        return paymentInstrument;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public MasterTransactionRecord GetTransactionByTransactionId(long masterTransactionRecordId)
        //{
        //    return context.MasterTransactionRecords.Find(masterTransactionRecordId);
        //}

        //public MasterTransactionRecord ReverseTransactionChargesFromReversedTransaction(long transactionId, string reversedByUsername)
        //{
        //    int reversedTransactionTypeId = (int)TransactionTypes.LoyaltyPointTransfer;
        //    DateTime transactionTime = DateTime.Now;
        //    try
        //    {

        //        new HelperRepository().ValidateUsername(reversedByUsername, false);
        //        MasterTransactionRecord reversedTransaction = context.MasterTransactionRecords.Find(transactionId);

        //        if (reversedTransaction == null)
        //        {
        //            throw new Exception(String.Format("TRN0011 - The TransactionId '{0}' does not exist", transactionId));
        //        }

        //        Customer customerToDebit = context.Customers.Find(reversedTransaction.PayeeId);
        //        Customer customerToCredit = context.Customers.Find(reversedTransaction.PayerId);

        //        if (customerToDebit == null)
        //        {
        //            throw new Exception(String.Format("CA0004 - CustomerId '{0}' does not exist.", reversedTransaction.PayeeId));
        //        }
        //        if (customerToCredit == null)
        //        {
        //            throw new Exception(String.Format("CA0004 - CustomerId '{0}' does not exist.", reversedTransaction.PayerId));
        //        }

        //        string reversibleCharge = reversedTransaction.Text2;//text 2 contains the transactional fee or charge 

        //        if (string.IsNullOrEmpty(reversibleCharge))
        //        {
        //            throw new Exception("The Adjustment Amount must be set.");
        //        }

        //        decimal senderBalanceBeforeTransaction = BalanceEnquiry(reversedTransaction.PayeeId, reversedTransaction.PayeePaymentInstrumentId, string.Empty, false);
        //        decimal receiverBalanceBeforeTransaction = BalanceEnquiry(reversedTransaction.PayerId, reversedTransaction.PayerPaymentInstrumentId, string.Empty, false);

        //        long transactionAmount = long.Parse(reversibleCharge);

        //        AgentOutlet receivingOutlet = context.AgentOutlets.SingleOrDefault(x => x.TillCustomerId == reversedTransaction.PayerId);

        //        PaymentInstrument receivingPaymentInstrument = context.PaymentInstruments.Find(reversedTransaction.PayerPaymentInstrumentId);
        //        PaymentInstrument sendingPaymentInstrument = context.PaymentInstruments.Find(reversedTransaction.PayeePaymentInstrumentId);

        //        // MasterTransactionRecord originalTransaction = context.MasterTransactionRecords.Find(long.Parse(reversedTransaction.Text4));
        //        //new HelperRepository().ValidateAgentTransactionLimits(customerToCredit.CustomerTypeId, receivingPaymentInstrument, originalTransaction.TransactionTypeId, transactionAmount, receivingOutlet.OutletTypeId, receiverBalanceBeforeTransaction, false);

        //        HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, transactionAmount, 0);

        //        MasterTransactionRecord reverseTransaction = new MasterTransactionRecord
        //        {
        //            TransactionTypeId = reversedTransactionTypeId,
        //            PayerId = reversedTransaction.PayeeId,
        //            PayeeId = reversedTransaction.PayerId,
        //            PayerPaymentInstrumentId = reversedTransaction.PayeePaymentInstrumentId,
        //            PayeePaymentInstrumentId = reversedTransaction.PayerPaymentInstrumentId,
        //            ExternalApplicationId = 1,
        //            AccessChannelId = 1,
        //            Amount = transactionAmount,
        //            Fee = 0,
        //            CustomerTypeId = customerToDebit.CustomerTypeId,
        //            Tax = 0,
        //            Text = ReversalTransactionNarrative(reversedTransaction.Text),
        //            Text2 = reversedByUsername,
        //            Text3 = reversedTransaction.TransactionReference,
        //            Text4 = reversedTransaction.Text4,//original transaction Id
        //            Text5 = "Reversed TransactionId: " + reversedTransaction.MasterTransactionRecordId,
        //            Text6 = reversedByUsername,
        //            Text7 = "Fee Charged",
        //            TransactioErrorCodeId = 1,
        //            IsTestTransaction = customerToDebit.IsTestCustomer,
        //            TransactionDate = transactionTime,
        //            TransactionReference = reversedTransaction.TransactionReference + "(Rv)",
        //            SourceUserName = string.IsNullOrEmpty(customerToDebit.UserName) ? null : customerToDebit.UserName,
        //            DestinationUserName = string.IsNullOrEmpty(customerToCredit.UserName) ? null : customerToCredit.UserName,
        //            ShortDescription = TransactionTypes.ReversalTransaction.ToString(),
        //            PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //            PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //            TransactionStatusId = TransactionState.Successful
        //        };


        //        //AgentTransactionGL senderGLRecord = new AgentTransactionGL
        //        //{
        //        //    TillCustomerId = customerToDebit.CustomerId,
        //        //    CommssionEarned = 0,
        //        //    MasterTransactionRecordId = reverseTransaction.MasterTransactionRecordId,
        //        //    //OperatorCustomerId = senderCustomerId,
        //        //    TransactionDate = transactionTime,
        //        //};

        //        //AgentTransactionGL recipientGLRecord = new AgentTransactionGL
        //        //{
        //        //    TillCustomerId = customerToCredit.CustomerId,
        //        //    CommssionEarned = 0,
        //        //    MasterTransactionRecordId = reverseTransaction.MasterTransactionRecordId,
        //        //    //OperatorCustomerId = recipientCustomerId,
        //        //    TransactionDate = transactionTime
        //        //};

        //        //FeesGL feeGl = new FeesGL
        //        //{
        //        //    MasterTransactionRecordId = reverseTransaction.MasterTransactionRecordId,
        //        //    CustomerFeeAmount = 0,
        //        //    TransactionCommission = 0,
        //        //    GrossRevenue = 0,
        //        //    TransactionDate = transactionTime,
        //        //    TransactionDescription = TransactionTypes.ReversalTransaction .ToString()
        //        //};

        //        HelperRepository.UpdateCustomerSVABalance(context, reversedTransaction.PayeeId, sendingPaymentInstrument, transactionAmount, 0, false);
        //        HelperRepository.UpdateCustomerSVABalance(context, reversedTransaction.PayerId, receivingPaymentInstrument, transactionAmount, 0, true);

        //        context.MasterTransactionRecords.Add(reverseTransaction);
        //        //context.AgentTransactionGLs.Add(senderGLRecord);
        //        //context.AgentTransactionGLs.Add(recipientGLRecord);
        //        //context.FeesGLs.Add(feeGl);
        //        context.SaveChanges();


        //        decimal senderBalanceAfterTransaction = BalanceEnquiry(reversedTransaction.PayeeId, reversedTransaction.PayeePaymentInstrumentId, string.Empty, false);
        //        decimal receiverBalanceAfterTransaction = BalanceEnquiry(reversedTransaction.PayerId, reversedTransaction.PayerPaymentInstrumentId, string.Empty, false);

        //        reverseTransaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //        reverseTransaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //        context.SaveChanges();


        //        long payerMsisdn = customerRepository.GetCustomerPhoneNumber(customerToDebit.CustomerId);
        //        long payeeMsisdn = customerRepository.GetCustomerPhoneNumber(customerToCredit.CustomerId);

        //        //string payerMessage = String.Format("Confirmed {0}. The extra amount in transaction reference {1} of KSh.{2} was reclaimed without charges on {3} at {4}. New MobiKash Balance is KSh.{5}", reverseTransaction.TransactionReference, reversedTransaction.TransactionReference, GetBalanceFromDecimal(reverseTransaction.Amount), DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), GetBalanceFromDecimal(senderBalanceAfterTransaction));
        //        //string payeeMessage = String.Format("Confirmed {0}. Your {1} for transaction reference {2} of KSh.{3} reversed without charges on {4} at {5}. New MobiKash Balance is KSh.{6}", reverseTransaction.TransactionReference, reverseTransaction.Text7, reversedTransaction.TransactionReference, GetBalanceFromDecimal(reverseTransaction.Amount), DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), GetBalanceFromDecimal(receiverBalanceAfterTransaction));

        //        //Parallel.Invoke(() =>
        //        //                {
        //        //                    messageRepository.SendTransactionReversalSms(reverseTransaction.MasterTransactionRecordId, payerMsisdn.ToString(), payeeMsisdn.ToString(), payerMessage, payeeMessage);
        //        //                });

        //        Task.Factory.StartNew(() =>
        //        {
        //            messageRepository.ReverseTransactionChargesFromReversedTransactionSms(customerToDebit, customerToCredit, reversedTransaction);
        //        }).ContinueWith(previousTask => { previousTask.Exception.Handle(ex => { return true; }); }, TaskContinuationOptions.OnlyOnFaulted);



        //        return reverseTransaction;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public PaymentInstrument ResolveFIPaymentInstrument(string fiCode, string fiAccount)
        //{
        //    FinancialInstitution fi = FiCodeExists(fiCode);

        //    if (context.PaymentInstruments.Count(x => x.FinancialInstitutionId == fi.FinancialInstitutionId && x.AccountNumber == fiAccount) > 0)
        //    {
        //        return context.PaymentInstruments.SingleOrDefault(x => x.FinancialInstitutionId == fi.FinancialInstitutionId && x.AccountNumber == fiAccount);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //private FinancialInstitution FiCodeExists(string fiCode)
        //{
        //    if (context.FinancialInstitutions.Count(x => x.FinancialInstitutionCode == fiCode) > 0)
        //    {
        //        return context.FinancialInstitutions.Single(x => x.FinancialInstitutionCode == fiCode);
        //    }
        //    else
        //    {
        //        throw new Exception("FI0001 - Financial Institution does not exist");
        //    }
        //}

        //private PITypes GetPIType(long paymentInstrumentId)
        //{
        //    PaymentInstrument pi = context.PaymentInstruments.Find(paymentInstrumentId);

        //    if (pi == null)
        //    {
        //        throw new Exception("PI0001", new Exception("Payment Instrument doesn’t exist"));
        //    }

        //    return (PITypes)pi.PaymentInstrumentTypeId;
        //}

        //public List<MasterTransactionRecord> GetAllSystemTransactions()
        //{
        //    //using (ILoyalDataModel context = new ILoyalDataModel(_))
        //    //using (var scope = new TransactionScope(TransactionScopeOption.Required,options))
        //    //{
        //    try
        //    {
        //        //DataLoadOptions dboptions = new DataLoadOptions();
        //        //dboptions.LoadWith<MasterTransactionRecord>(p => p.UserType);
        //        //// dboptions.LoadWith<MasterTransactionRecord>(p => p.);
        //        //context.LoadOptions = dboptions;

        //        return context.MasterTransactionRecords.Take(100).OrderByDescending(x => x.TransactionDate).ToList();
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}


        //public List<MasterTransactionRecord> GetConsumerMiniStatement(long customerId, long paymentInstrumentId, int numberOfTransactions, DateTime startDate, DateTime endDate)
        //{
        //    List<MasterTransactionRecord> customerRecords = null;

        //    try
        //    {
        //        DateTime preciseStartDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day);
        //        DateTime preciseEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day).AddDays(1).AddMilliseconds(-1);

        //        if (numberOfTransactions > 0)
        //        {
        //            customerRecords = context.MasterTransactionRecords.Where(x => x.PayerId == customerId || x.PayeeId == customerId)
        //                .Where(x => x.PayerPaymentInstrumentId == paymentInstrumentId || x.PayeePaymentInstrumentId == paymentInstrumentId)
        //                .Where(x => x.TransactionDate >= preciseStartDateTime && x.TransactionDate <= preciseEndDate)
        //                .Where(x => x.TransactioErrorCodeId == 1)
        //                .OrderByDescending(x => x.TransactionDate)
        //                .Take(numberOfTransactions).ToList();

        //            // customerRecords = context.MasterTransactionRecords.Where(x => x.PayerId == customerId || x.PayeeId == customerId && x.TransactionDate >= startDate && x.TransactionDate <= endDate).OrderByDescending(x => x.TransactionDate).Take(numberOfTransactions).ToList();
        //        }
        //        else
        //        {
        //            customerRecords = context.MasterTransactionRecords.Where(x => x.PayerId == customerId || x.PayeeId == customerId)
        //                .Where(x => x.PayerPaymentInstrumentId == paymentInstrumentId || x.PayeePaymentInstrumentId == paymentInstrumentId)
        //                .Where(x => x.TransactionDate >= preciseStartDateTime && x.TransactionDate <= preciseEndDate)
        //                .Where(x => x.TransactioErrorCodeId == 1)
        //                .OrderByDescending(x => x.TransactionDate).ToList();
        //        }
        //        return customerRecords;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public MasterTransactionRecord ChangePIN(long customerId, long paymentInstrumentId, string oldPin, string newPin)
        //{
        //    DateTime transactionTime = DateTime.Now;
        //    //bool changePinResult = false;

        //    Customer customer = null;
        //    PaymentInstrument paymentInstrument = null;

        //    MatchPaymentInstrumentToCustomer(customerId, paymentInstrumentId, out customer, out paymentInstrument);

        //    int loginAttempts = 0;
        //    customer = customerRepository.CustomerLogin(customerId, oldPin, out loginAttempts);

        //    if (customer != null)
        //    {
        //        try
        //        {
        //            if (customer.Nonce == null)
        //            {
        //                throw new Exception("CA0001 - Customer PIN not set");
        //            }

        //            if (!customer.TermsAccepted)
        //            {
        //                throw new Exception("CA0005 - Customer has not accepted Terms And Conditions");
        //            }

        //            if (customer.BlacklistReasonId > (int)BlacklistReasons.Active)
        //            {
        //                throw new Exception("CA0006 - Customer is blacklisted therefore cannot login.");
        //            }

        //            TransactionType transactionType = new HelperRepository().GetTransactionTypeByTransactionTypeId((int)TransactionTypes.ChangePin);

        //            if (!transactionType.IsActive)
        //                return null;



        //            //TODO get the correct payee for the change pin txn and other txns where the payee is the mcommerce firm. possibly an SVA is needed for this
        //            // TODO get a way of passing in the appid and app secret of apps so as to determine the access channels and external app id                                                        

        //            long currentPasswordHashKeyId = long.Parse(EncDec.Decrypt(customer.Nonce, customer.Salt));
        //            PasswordHashKey currentPhk = context.PasswordHashKeys.Find(currentPasswordHashKeyId);
        //            context.PasswordHashKeys.Remove(currentPhk);
        //            customer.Nonce = null;
        //            context.SaveChanges();

        //            TransactionResult changePinTransaction = customerRepository.SetCustomerPin(customerId, newPin, false);

        //            long feeAmount = 0;
        //            bool defaultTransactionFee = false;

        //            //PaymentInstrument paymentInstrument = GetDefaultPaymentInstrument(customerId, customer.CustomerTypeId);

        //            PaymentInstrument operatorTransactionsPaymentInstrument = null;

        //            Customer operatorTransactionCustomer = customerRepository.GetOperatorTransactionsUser(out operatorTransactionsPaymentInstrument);

        //            decimal senderBalanceBeforeTransaction = BalanceEnquiry(customerId, paymentInstrument.PaymentInstrumentId, string.Empty, false);
        //            decimal receiverBalanceBeforeTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //            long tariffItemId = 0;
        //            int billerTypeId = 0;

        //            if (paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.MPESA)
        //            {
        //                Biller biller = context.Billers.FirstOrDefault(x => x.CustomerId == customerId && x.OperationPaymentInstrumentId == paymentInstrument.PaymentInstrumentId);
        //                if (biller == null || transactionTime >= biller.TerminationDate)
        //                {
        //                    throw new Exception("BL0010 - The Biller is not active. The termination date is passed.");
        //                }
        //                billerTypeId = biller.BillerTypeId;
        //            }


        //            //feeAmount = tariffRepository.GetTariffCharge((PITypes)paymentInstrument.PaymentInstrumentTypeId, (int)AccessChannels.USSD, transactionType.TransactionTypeId, 0, out defaultTransactionFee, out tariffItemId, billerTypeId);

        //            string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //            //HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, 0, feeAmount);

        //            MasterTransactionRecord newChangePinTransaction = new MasterTransactionRecord
        //            {
        //                TransactionTypeId = transactionType.TransactionTypeId,
        //                PayerId = customerId,
        //                PayeeId = operatorTransactionCustomer.CustomerId,
        //                PayerPaymentInstrumentId = paymentInstrument.PaymentInstrumentId,
        //                PayeePaymentInstrumentId = operatorTransactionsPaymentInstrument.PaymentInstrumentId,
        //                ExternalApplicationId = 1,
        //                AccessChannelId = 1,
        //                Amount = 0,
        //                Fee = feeAmount,
        //                CustomerTypeId = customer.CustomerTypeId,
        //                Tax = 0,
        //                Text = transactionType.FriendlyName,
        //                TransactioErrorCodeId = 1,
        //                IsTestTransaction = customer.IsTestCustomer,
        //                TransactionDate = transactionTime,
        //                TransactionReference = transactionReference,
        //                ShortDescription = transactionType.FriendlyName,
        //                PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //                PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //                TransactionStatusId = TransactionState.Successful
        //            };

        //            HelperRepository.UpdateCustomerSVABalance(context, customerId, paymentInstrument, 0, feeAmount, false);
        //            HelperRepository.UpdateCustomerSVABalance(context, operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument, feeAmount, 0, true);

        //            CustomerTransactionGL customerGlTransaction = new CustomerTransactionGL
        //            {
        //                CustomerId = customerId,
        //                TransactionDate = transactionTime,
        //                MasterTransactionRecordId = newChangePinTransaction.MasterTransactionRecordId
        //            };

        //            CustomerTransactionGL receiverGlTransaction = new CustomerTransactionGL
        //            {
        //                CustomerId = operatorTransactionCustomer.CustomerId,
        //                TransactionDate = transactionTime,
        //                MasterTransactionRecordId = newChangePinTransaction.MasterTransactionRecordId
        //            };

        //            //TODO cheque what the tariff table says about the amount below
        //            //long transactionCommission = agentRepository.CalculateAgentCommission(1, transactionTypeId, 10);
        //            long transactionCommission = 0;

        //            //FeesGL feeGl = new FeesGL
        //            //{
        //            //    MasterTransactionRecordId = newChangePinTransaction.MasterTransactionRecordId,
        //            //    TransactionDescription = transactionType.FriendlyName,
        //            //    CustomerFeeAmount = feeAmount,
        //            //    TransactionCommission = transactionCommission,
        //            //    GrossRevenue = feeAmount - transactionCommission,
        //            //    TransactionDate = transactionTime
        //            //};

        //            context.MasterTransactionRecords.Add(newChangePinTransaction);
        //            context.CustomerTransactionGLs.Add(customerGlTransaction);
        //            context.CustomerTransactionGLs.Add(receiverGlTransaction);
        //            //context.FeesGLs.Add(feeGl);
        //            context.SaveChanges();

        //            decimal senderBalanceAfterTransaction = BalanceEnquiry(customerId, paymentInstrument.PaymentInstrumentId, string.Empty, false);
        //            decimal receiverBalanceAfterTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //            newChangePinTransaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //            newChangePinTransaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //            context.SaveChanges();

        //            //Task.Factory.StartNew(() =>
        //            //{
        //            messageRepository.ChangePinSms(customer, newPin, newChangePinTransaction.MasterTransactionRecordId, transactionReference, senderBalanceAfterTransaction, transactionTime);
        //            //})
        //            //.ContinueWith(previousTask =>
        //            //{
        //            //    previousTask.Exception.Handle(ex =>
        //            //    {
        //            //        return true;
        //            //    });
        //            //}, TaskContinuationOptions.OnlyOnFaulted);

        //            return newChangePinTransaction;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //    else
        //    {
        //        //changePinResult = false;
        //        throw new Exception("Invalid customer login");
        //    }


        //}

        //public void CheckConsumerUpperLimit(decimal customerBalance, long transactionAmount)
        //{
        //    PaymentInstrumentLimit paymentInstrumentLimit = context.PaymentInstrumentLimits.SingleOrDefault(limit => limit.OutletTypeId == 0 && limit.CustomerTypeId == (int)CustomerTypes.Consumer);
        //    if ((customerBalance + transactionAmount) > paymentInstrumentLimit.MaximumLimit)
        //    {
        //        throw new Exception("The customer Upper limit will be hit with this transaction");
        //    }
        //}

        //public void PurchaseFloat(long customerId, int amount)
        //{
        //    DateTime transactionTime = DateTime.Now;
        //    int transactionTypeId = 28;
        //    MasterTransactionRecord newSendMoneyTransaction = null;

        //    // Customer sender =customerRepository.getcustomerby

        //    decimal feeAmount = 0;
        //    bool defaultTransactionFee = false;

        //    //TODO sort this out, purchase float is still not fully implemented.

        //    //feeAmount = tariffRepository.GetTariffCharge(1, transactionTypeId, amount, out defaultTransactionFee);
        //    feeAmount = 20;

        //    string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //    //newSendMoneyTransaction = new MasterTransactionRecord
        //    //{
        //    //    TransactionTypeId = transactionTypeId,
        //    //    PayerId = senderCustomerId,
        //    //    PayeeId = customerId,
        //    //    PayerPaymentInstrumentId = senderPaymentInstrumentId,
        //    //    PayeePaymentInstrumentId = receiverpaymentInstrumentId,
        //    //    ExternalApplicationId = 1,
        //    //    AccessChannelId = 1,
        //    //    Amount = amount,
        //    //    Fee = feeAmount,
        //    //    UserTypeId = sender.UserTypeId,
        //    //    Tax = 0,
        //    //    Text = text,
        //    //    Text2 = text2,
        //    //    Text3 = text3,
        //    //    TransactioErrorCodeId = 1,
        //    //    IsTestTransaction = sender.IsTestCustomer,
        //    //    TransactionDate = transactionTime,
        //    //    TransactionReference = transactionReference,
        //    //    ShortDescription = AGENT_CASH_IN
        //    //};

        //    context.MasterTransactionRecords.Add(newSendMoneyTransaction);
        //    context.SaveChanges();
        //    //  }
        //}
        //public string ResetPIN(long customerId)
        //{
        //    DateTime transactionTime = DateTime.Now;
        //    try
        //    {

        //        TransactionType transactionType = new HelperRepository().GetTransactionTypeByTransactionTypeId((int)TransactionTypes.PinReset);
        //        if (!transactionType.IsActive)
        //            return null;

        //        Customer customer = context.Customers.Find(customerId);

        //        if (customer == null)
        //        {
        //            throw new Exception(string.Format("CA0004 - CustomerID '{0}' does not exist", customerId));
        //        }

        //        //if (customer.SybasePin == null && customer.SybasePassword == null)
        //        //{
        //        //    if (customer.Nonce == null)
        //        //    {
        //        //        throw new Exception("CA0001 - Customer PIN not set");
        //        //    }
        //        //}

        //        if (customer.DeactivatedAccount)
        //        {
        //            throw new Exception("Customer Is Deactivated");
        //        }
        //        if (customer.IsBlacklisted)
        //        {
        //            throw new Exception("Customer Is Blacklisted");
        //        }
        //        if (!customer.TermsAccepted)
        //        {
        //            throw new Exception("The customer has not accepted terms and conditions");
        //        }

        //        //bool defaultTransactionFee = false;

        //        //PaymentInstrument paymentInstrument = GetDefaultPaymentInstrument(customerId, customer.CustomerTypeId);
        //        //PaymentInstrument operatorTransactionsPaymentInstrument = null;
        //        //ILoyal.DataLayer.Customer operatorTransactionCustomer = customerRepository.GetOperatorTransactionsUser(out operatorTransactionsPaymentInstrument);

        //        ////long paymentInstrumentId = context.PaymentInstruments.First(x => x.CustomerId == customerId).PaymentInstrumentId;

        //        //decimal senderBalanceBeforeTransaction = BalanceEnquiry(customerId, paymentInstrument.PaymentInstrumentId, string.Empty, false);
        //        //decimal receiverBalanceBeforeTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //        //long tariffItemId = 0;

        //        //long resetPinFee = tariffRepository.GetTariffCharge(PITypes.MPESA, (int)AccessChannels.USSD, transactionType.TransactionTypeId, 0, out defaultTransactionFee, out tariffItemId);

        //        //HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, 0, resetPinFee);

        //        //string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //        long passwordHashKeyId = 0;

        //        //if (customer.Nonce != null)
        //        //{
        //        //    string pin = customer.Nonce; string salt = customer.Salt;

        //        //    string result = EncDec.Decrypt(customer.Nonce, customer.Salt);

        //        //    passwordHashKeyId = long.Parse(result);

        //        //    PasswordHashKey hashKey = context.PasswordHashKeys.Find(passwordHashKeyId);

        //        //    if (hashKey != null)
        //        //    {
        //        //        context.PasswordHashKeys.Remove(hashKey);
        //        //    }
        //        //}
        //        ManagePin managePin = new ManagePin();
        //        string newCustomerPin = HelperRepository.GenerateRandomPIN();
        //        long newPasswordHashKeyId = managePin.EncryptPin(newCustomerPin, true);
        //        string newCustomerSalt = randomStringGenerator.NextString(256, true, true, true, true);

        //        customer.Salt = newCustomerSalt;
        //        customer.Nonce = EncDec.Encrypt(newPasswordHashKeyId.ToString(), newCustomerSalt);

        //        // TODO think well about the PayeeID, and payer instrumentId

        //        //MasterTransactionRecord transactionRecord = new MasterTransactionRecord
        //        //{
        //        //    PayerId = customerId,
        //        //    PayeeId = operatorTransactionCustomer.CustomerId,
        //        //    Amount = 0,
        //        //    Fee = resetPinFee,
        //        //    PayeePaymentInstrumentId = operatorTransactionsPaymentInstrument.PaymentInstrumentId,
        //        //    PayerPaymentInstrumentId = paymentInstrument.PaymentInstrumentId,
        //        //    ExternalApplicationId = 1,
        //        //    AccessChannelId = 1,
        //        //    IsTestTransaction = customer.IsTestCustomer,
        //        //    Tax = 0,
        //        //    ShortDescription = transactionType.FriendlyName,
        //        //    Text = transactionType.FriendlyName,
        //        //    TransactioErrorCodeId = 1,
        //        //    TransactionDate = DateTime.Now,
        //        //    TransactionReference = transactionReference,
        //        //    TransactionTypeId = transactionType.TransactionTypeId,
        //        //    CustomerTypeId = customer.UserTypeId,
        //        //    PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //        //    PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //        //    TransactionStatusId = TransactionState.Successful
        //        //};

        //        //CustomerTransactionGL customerTransactionGL = new CustomerTransactionGL
        //        //{
        //        //    CustomerId = customerId,
        //        //    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //        //    TransactionDate = transactionTime
        //        //};
        //        //CustomerTransactionGL receiverTransactionGL = new CustomerTransactionGL
        //        //{
        //        //    CustomerId = operatorTransactionCustomer.CustomerId,
        //        //    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //        //    TransactionDate = transactionTime
        //        //};

        //        ////long agentCommission = agentRepository.CalculateAgentCommission(1, transactionTypeId, 0);
        //        //long agentCommission = 0;

        //        //FeesGL feeGl = new FeesGL
        //        //{
        //        //    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //        //    CustomerFeeAmount = resetPinFee,
        //        //    TransactionCommission = agentCommission,
        //        //    GrossRevenue = resetPinFee - agentCommission,
        //        //    TransactionDate = transactionTime,
        //        //    TransactionDescription = transactionType.FriendlyName
        //        //};

        //        //context.MasterTransactionRecords.Add(transactionRecord);

        //        //HelperRepository.UpdateCustomerSVABalance(context, customerId, paymentInstrument, 0, resetPinFee, false);
        //        //HelperRepository.UpdateCustomerSVABalance(context, operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument, resetPinFee, 0, true);

        //        //context.CustomerTransactionGLs.Add(customerTransactionGL);
        //        //context.CustomerTransactionGLs.Add(receiverTransactionGL);

        //        //context.FeesGLs.Add(feeGl);
        //        //context.SaveChanges();

        //        //decimal senderBalanceAfterTransaction = BalanceEnquiry(customerId, paymentInstrument.PaymentInstrumentId, string.Empty, false);
        //        //decimal receiverBalanceAfterTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //        //transactionRecord.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //        //transactionRecord.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //        context.SaveChanges();

        //        //TODO Send SMS for RESET PIN

        //        Parallel.Invoke(() =>
        //        {
        //            messageRepository.ResetPinSms(customer, newCustomerPin, transactionTime);
        //            //messageRepository.ResetPinSms(customer, newCustomerPin, transactionRecord.MasterTransactionRecordId, transactionReference, transactionTime);
        //        },
        //                        () =>
        //                        {
        //                            // analytics
        //                        });

        //        return newCustomerPin;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        //public bool ChangeLanguage(long customerId, int languageId)
        //{
        //    DateTime transactionTime = DateTime.Now;
        //    try
        //    {
        //        //TODO modify the Change Language to allow non-chargeable txns . Esp useful for call center scenarios

        //        int transactionTypeId = (int)TransactionTypes.ChangeLanguage;
        //        ILoyal.DataLayer.Customer customer = context.Customers.Find(customerId);
        //        customer.LanguageId = languageId;

        //        // long paymentInstrumentId = context.PaymentInstruments.First(x => x.CustomerId == customerId).PaymentInstrumentId;
        //        PaymentInstrument defaultPaymentInstrument = GetDefaultPaymentInstrument(customerId, customer.CustomerTypeId);

        //        int billerTypeId = 0;
        //        if (defaultPaymentInstrument.PaymentInstrumentTypeId == (int)PITypes.MPESA)
        //        {
        //            Biller biller = context.Billers.FirstOrDefault(x => x.CustomerId == customerId && x.OperationPaymentInstrumentId == defaultPaymentInstrument.PaymentInstrumentId);
        //            if (biller == null || transactionTime >= biller.TerminationDate)
        //            {
        //                throw new Exception("BL0010 - The Biller is not active. The termination date is passed.");
        //            }
        //            billerTypeId = biller.BillerTypeId;
        //        }

        //        PaymentInstrument operatorTransactionsPaymentInstrument = null;

        //        ILoyal.DataLayer.Customer operatorTransactionCustomer = customerRepository.GetOperatorTransactionsUser(out operatorTransactionsPaymentInstrument);

        //        decimal senderBalanceBeforeTransaction = BalanceEnquiry(customerId, defaultPaymentInstrument.PaymentInstrumentId, string.Empty, false);
        //        decimal receiverBalanceBeforeTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //        long tariffItemId = 0;
        //        bool defaultTransactionFee = false;

        //        //long changeLanguageFee = tariffRepository.GetTariffCharge((PITypes)defaultPaymentInstrument.PaymentInstrumentTypeId, (int)AccessChannels.USSD, transactionTypeId, 0, out defaultTransactionFee, out tariffItemId, billerTypeId);

        //        HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, 0, 0);
        //        string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //        MasterTransactionRecord transactionRecord = new MasterTransactionRecord
        //        {
        //            PayerId = customerId,
        //            PayeeId = operatorTransactionsPaymentInstrument.CustomerId,
        //            Amount = 0,
        //            Fee = 0,
        //            PayeePaymentInstrumentId = operatorTransactionsPaymentInstrument.PaymentInstrumentId,
        //            PayerPaymentInstrumentId = defaultPaymentInstrument.PaymentInstrumentId,
        //            ExternalApplicationId = 1,
        //            AccessChannelId = 1,
        //            IsTestTransaction = customer.IsTestCustomer,
        //            Tax = 0,
        //            ShortDescription = CHANGE_LANGUAGE,
        //            Text = null,
        //            TransactioErrorCodeId = 1,
        //            TransactionDate = DateTime.Now,
        //            TransactionReference = transactionReference,
        //            TransactionTypeId = transactionTypeId,
        //            PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //            PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //            CustomerTypeId = customer.CustomerTypeId,
        //            TransactionStatusId = TransactionState.Successful
        //        };

        //        CustomerTransactionGL senderTransactionGL = new CustomerTransactionGL
        //        {
        //            CustomerId = customerId,
        //            MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //            TransactionDate = transactionTime
        //        };

        //        CustomerTransactionGL receiverTransactionGL = new CustomerTransactionGL
        //        {
        //            CustomerId = operatorTransactionCustomer.CustomerId,
        //            MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //            TransactionDate = transactionTime
        //        };

        //        long agentCommission = 0;

        //        //FeesGL feeGl = new FeesGL
        //        //{
        //        //    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //        //    CustomerFeeAmount = changeLanguageFee,
        //        //    TransactionCommission = agentCommission,
        //        //    GrossRevenue = changeLanguageFee - agentCommission,
        //        //    TransactionDate = transactionTime,
        //        //    TransactionDescription = CHANGE_LANGUAGE
        //        //};

        //        context.MasterTransactionRecords.Add(transactionRecord);
        //        HelperRepository.UpdateCustomerSVABalance(context, customerId, defaultPaymentInstrument, 0, 0, false);
        //        HelperRepository.UpdateCustomerSVABalance(context, operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument, 0, 0, true);

        //        context.CustomerTransactionGLs.Add(senderTransactionGL);
        //        context.CustomerTransactionGLs.Add(receiverTransactionGL);
        //        //context.FeesGLs.Add(feeGl);

        //        context.SaveChanges();

        //        decimal senderBalanceAfterTransaction = BalanceEnquiry(customerId, defaultPaymentInstrument.PaymentInstrumentId, string.Empty, false);
        //        decimal receiverBalanceAfterTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //        transactionRecord.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //        transactionRecord.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //        context.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        //public List<MasterTransactionRecord> GetTransactionByTransactionReference(string transactionReference)
        //{
        //    try
        //    {
        //        return context.MasterTransactionRecords.Where(x => x.TransactionReference == transactionReference).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        //public List<MasterTransactionRecord> GetCustomerTransactionsBetweenRange(long mainCustomerId, long mainCustomerPaymentInstrumentId, long otherPartyCustomerId, int numberOfTransactions, int pageNumber,
        //    DateTime startDate, DateTime endDate, bool onlyFaultyTransactions, out int recordCount)
        //{
        //    List<MasterTransactionRecord> customerTransactions = null;
        //    try
        //    {
        //        DateTime preciseStartDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day);
        //        DateTime preciseEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day).AddDays(1).AddMilliseconds(-1);

        //        if (otherPartyCustomerId == 0)
        //        {
        //            //.Skip(pageNumber * numberOfTransactions).Take(numberOfTransactions)
        //            if (mainCustomerPaymentInstrumentId > 0)
        //            {
        //                customerTransactions = context.MasterTransactionRecords.Where(x => (x.PayerId == mainCustomerId && x.PayerPaymentInstrumentId == mainCustomerPaymentInstrumentId) || (x.PayeeId == mainCustomerId && x.PayeePaymentInstrumentId == mainCustomerPaymentInstrumentId))
        //                                                                       .Where(x => x.TransactionDate >= preciseStartDateTime && x.TransactionDate <= preciseEndDate)
        //                                                                       .OrderByDescending(x => x.TransactionDate).ToList();
        //            }
        //            else
        //            {
        //                customerTransactions = context.MasterTransactionRecords.Where(x => (x.PayerId == mainCustomerId || x.PayeeId == mainCustomerId))
        //                                                                    .Where(x => x.TransactionDate >= preciseStartDateTime && x.TransactionDate <= preciseEndDate)
        //                                                                    .OrderByDescending(x => x.TransactionDate).ToList();
        //            }

        //            customerTransactions = numberOfTransactions == 0 ? customerTransactions : customerTransactions.Take(numberOfTransactions).ToList();
        //            recordCount = customerTransactions == null ? 0 : customerTransactions.Count;
        //        }
        //        else
        //        {
        //            if (mainCustomerPaymentInstrumentId > 0)
        //            {
        //                customerTransactions = context.MasterTransactionRecords.Where(x => (x.PayerId == mainCustomerId && x.PayerPaymentInstrumentId == mainCustomerPaymentInstrumentId && x.PayeeId == otherPartyCustomerId) || (x.PayerId == otherPartyCustomerId && x.PayeeId == mainCustomerId && x.PayeePaymentInstrumentId == mainCustomerPaymentInstrumentId))
        //                                                                       //.Where(x => x.PayeeId == otherPartyCustomerId || x.PayeeId == mainCustomerId)
        //                                                                       .Where(x => x.TransactionDate >= preciseStartDateTime && x.TransactionDate <= preciseEndDate)
        //                                                                       .OrderByDescending(x => x.TransactionDate).ToList();
        //            }
        //            else
        //            {
        //                customerTransactions = context.MasterTransactionRecords.Where(x => (x.PayerId == mainCustomerId && x.PayeeId == otherPartyCustomerId) || (x.PayerId == otherPartyCustomerId && x.PayeeId == mainCustomerId))
        //                                                                       .Where(x => x.TransactionDate >= preciseStartDateTime && x.TransactionDate <= preciseEndDate)
        //                                                                       .OrderByDescending(x => x.TransactionDate).ToList();
        //            }

        //            customerTransactions = numberOfTransactions == 0 ? customerTransactions : customerTransactions.Take(numberOfTransactions).ToList();
        //            recordCount = customerTransactions == null ? 0 : customerTransactions.Count;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    return customerTransactions;
        //}
        //public List<MasterTransactionRecord> GetAllTransactions()
        //{
        //    if (applicationSettings.EnableGetAllTransactions)
        //    {
        //        return context.MasterTransactionRecords.ToList();
        //    }
        //    else
        //    {
        //        throw new Exception("This Method has been disabled");
        //    }
        //}

        //public bool GetFullStatement(long customerId, long paymentInstrumentId, int numberOfMonths)
        //{
        //    DateTime transactionTime = DateTime.Now;

        //    int transactionTypeId = (int)TransactionTypes.StatementRequest;

        //    if (numberOfMonths < 1)
        //    {
        //        //         return false;
        //        throw new Exception(String.Format("CA0018 - Cannot show Full Statement for '{0}' months", numberOfMonths));
        //    }

        //    // List<MasterTransactionRecord> transactionRecords = new List<MasterTransactionRecord>();

        //    Customer customer = null;

        //    PaymentInstrument paymentInstrument = null;

        //    MatchPaymentInstrumentToCustomer(customerId, paymentInstrumentId, out customer, out paymentInstrument);

        //    HelperRepository.ValidateEmailAddress(customer.EmailAddress, throwsIsNullException: true, throwsIsValidException: true);

        //    if (numberOfMonths > applicationSettings.MaximumMonthsQueryableInFullStatement)
        //    {
        //        throw new Exception(string.Format("CA0018 - Cannot show Full Statement for '{0}' months", numberOfMonths));
        //    }

        //    PaymentInstrument operatorTransactionsPaymentInstrument = null;
        //    Customer operatorTransactionCustomer = customerRepository.GetOperatorTransactionsUser(out operatorTransactionsPaymentInstrument);

        //    bool defaultTransactionFee = false;
        //    long tariffItemId = 0;

        //    int billerTypeId = 0;
        //    if (paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.MPESA)
        //    {
        //        Biller biller = context.Billers.FirstOrDefault(x => x.CustomerId == customerId && x.OperationPaymentInstrumentId == paymentInstrument.PaymentInstrumentId);
        //        if (biller == null || transactionTime >= biller.TerminationDate)
        //        {
        //            throw new Exception("BL0010 - The Biller is not active. The termination date is passed.");
        //        }
        //        billerTypeId = biller.BillerTypeId;
        //    }

        //    //long fullStatementFee = tariffRepository.GetTariffCharge((PITypes)paymentInstrument.PaymentInstrumentTypeId, (int)AccessChannels.USSD, transactionTypeId, 0, out defaultTransactionFee, out tariffItemId, billerTypeId);
        //    long fullStatementFee = 0;
        //    decimal senderBalanceBeforeTransaction = BalanceEnquiry(customerId, paymentInstrument.PaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceBeforeTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //    HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, 0, fullStatementFee);

        //    string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //    MasterTransactionRecord transactionRecord = new MasterTransactionRecord
        //    {
        //        PayerId = customerId,
        //        PayeeId = operatorTransactionCustomer.CustomerId,
        //        Amount = 0,
        //        Fee = fullStatementFee,
        //        PayerPaymentInstrumentId = paymentInstrument.PaymentInstrumentId,
        //        PayeePaymentInstrumentId = operatorTransactionsPaymentInstrument.PaymentInstrumentId,
        //        ExternalApplicationId = 1,
        //        AccessChannelId = 1,
        //        IsTestTransaction = customer.IsTestCustomer,
        //        Tax = 0,
        //        ShortDescription = FULL_WALLET_STATEMENT,
        //        Text = numberOfMonths.ToString() + " Months",
        //        Text2 = numberOfMonths.ToString(),
        //        TransactioErrorCodeId = 1,
        //        TransactionDate = DateTime.Now,
        //        TransactionReference = transactionReference,
        //        TransactionTypeId = transactionTypeId,
        //        CustomerTypeId = customer.CustomerTypeId,
        //        PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //        PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //        TransactionStatusId = TransactionState.Successful
        //    };

        //    CustomerTransactionGL senderTransactionGL = new CustomerTransactionGL
        //    {
        //        CustomerId = customerId,
        //        TransactionDate = transactionTime,
        //        MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //    };

        //    CustomerTransactionGL receiverTransactionGL = new CustomerTransactionGL
        //    {
        //        CustomerId = operatorTransactionCustomer.CustomerId,
        //        MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //        TransactionDate = transactionTime
        //    };

        //    long agentCommission = 0;

        //    //FeesGL feesGl = new FeesGL
        //    //{
        //    //    MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //    //    CustomerFeeAmount = fullStatementFee,
        //    //    TransactionDate = transactionTime,
        //    //    TransactionCommission = agentCommission,
        //    //    GrossRevenue = fullStatementFee - agentCommission,
        //    //    TransactionDescription = FULL_WALLET_STATEMENT
        //    //};

        //    HelperRepository.UpdateCustomerSVABalance(context, customerId, paymentInstrument, 0, fullStatementFee, false);
        //    HelperRepository.UpdateCustomerSVABalance(context, operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument, fullStatementFee, 0, true);

        //    context.MasterTransactionRecords.Add(transactionRecord);
        //    context.CustomerTransactionGLs.Add(senderTransactionGL);
        //    context.CustomerTransactionGLs.Add(receiverTransactionGL);
        //    //context.FeesGLs.Add(feesGl);

        //    context.SaveChanges();

        //    decimal senderBalanceAfterTransaction = BalanceEnquiry(customerId, paymentInstrument.PaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceAfterTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //    transactionRecord.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //    transactionRecord.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //    context.SaveChanges();

        //    //int recordCount = 0;
        //    //DateTime startDate = transactionTime.AddMonths(-numberOfMonths);
        //    //List<MasterTransactionRecord> transactionRecords = GetCustomerTransactionsBetweenRange(customer.CustomerId, 0, customer.CustomerId, 100000, 1000, startDate, transactionTime, false, out recordCount);

        //    //Parallel.Invoke(() =>
        //    //                {
        //    //FullStatementEmail(Customer customer, List<MasterTransactionRecord> transactionList, DateTime startDate, DateTime transactionDate, long masterTransactionRecordId, string transactionReference, int numberOfMonths)
        //    messageRepository.FullStatementEmail(customer, paymentInstrument, transactionTime, transactionRecord.MasterTransactionRecordId, transactionReference, numberOfMonths);
        //    //},
        //    //() =>
        //    //{
        //    //McommerceAnalytics.Entities.Transaction fullStatementAnalyticsTransaction = new McommerceAnalytics.Entities.Transaction
        //    //{
        //    //    TransactionType = "FullStatement",
        //    //    Amount = 0,
        //    //    DestinationCustomerType = operatorTransactionCustomer.CustomerType.CustomerTypeName,
        //    //    IsbankingTransaction = transactionRecord.IsBankingTransaction,
        //    //    IsTestTransacion = transactionRecord.IsTestTransaction,
        //    //    Fee = fullStatementFee / baseConversionUnit,
        //    //    SourceCustomerType = customer.CustomerType.CustomerTypeName,
        //    //    TransactionTime = transactionTime,
        //    //    AccessChannel = "USSD",
        //    //    ExternalApplication = "USSD App"
        //    //};

        //    //McommerceAnalytics.Entities.FullStatement fullStatement = new McommerceAnalytics.Entities.FullStatement
        //    //{
        //    //    CustomerId = customerId,
        //    //    Months = numberOfMonths,
        //    //    Transaction = fullStatementAnalyticsTransaction
        //    //};

        //    //ManageAnalytics manageAnalytics = new ManageAnalytics(this.);
        //    //manageAnalytics.SaveRecord(AnalyticTypes.FullStatement, customerId, JsonConvert.SerializeObject(fullStatement));

        //    //});

        //    return true;
        //}
        //public List<FullStatementRequest> GetFullStatementRequests(FullStatementRequestStates status)
        //{
        //    return context.FullStatementRequests.Where(fsr => fsr.FullStatementRequestStatusId == (int)status).ToList();
        //}
        //public void UpdateFullStatementStatus(long fullStatementRequestId, FullStatementRequestStates status)
        //{
        //    FullStatementRequest fullStatementRequest = context.FullStatementRequests.Find(fullStatementRequestId);

        //    if (fullStatementRequest == null) throw new Exception("Full statement request id " + fullStatementRequestId + " does not exist");

        //    fullStatementRequest.FullStatementRequestStatusId = (int)status;
        //    context.SaveChanges();
        //}
        //public void PurgeOldFullStatementRequests(int ageDays)
        //{
        //    IEnumerable<FullStatementRequest> fullStatementRequests = context.FullStatementRequests.Where(fsr => fsr.DateModified <= DateTime.Now.AddDays(ageDays));
        //    context.FullStatementRequests.RemoveRange(fullStatementRequests);
        //    context.SaveChanges();
        //}

        //public FeesGL GetTransactionCommission(long masterTransactionRecordId)
        //{
        //    return context.FeesGLs.SingleOrDefault(x => x.MasterTransactionRecordId == masterTransactionRecordId);
        //}

        // TODO remove this method after the importation process is done
        //public MasterTransactionRecord ImportTransaction(MasterTransactionRecord transactionRecord)
        //{
        //    // MasterTransactionRecord record = transactionRecord;
        //    try
        //    {
        //        context.MasterTransactionRecords.Add(transactionRecord);
        //        context.SaveChanges();
        //        return transactionRecord;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //public bool ResetSecurityCode(long customerId)
        //{
        //    DateTime transactionTime = DateTime.Now;
        //    int transactionTypeId = (int)TransactionTypes.ResetSecurityCode;
        //    //TODO get this working as soon as SMS is working

        //    ILoyal.DataLayer.Customer customer = context.Customers.Find(customerId);

        //    //TODO check if customer is not Deactivated and blacklisted.

        //    if (customer == null)
        //    {
        //        throw new Exception(String.Format("CA0004 - CustomerID '{0}' does not exist", customerId));
        //    }

        //    string currentSecurityCode = customer.SecurityCode;

        //    string newSecurityCode = customerRepository.GetSecurityCode();

        //    while (currentSecurityCode == newSecurityCode)
        //    {
        //        newSecurityCode = new CustomerRepository().GetSecurityCode();
        //    }

        //    customer.SecurityCode = newSecurityCode;
        //    PaymentInstrument customerDefaultWallet = context.PaymentInstruments.First(x => x.CustomerId == customer.CustomerId);
        //    PaymentInstrument operatorTransactionsPaymentInstrument = null;
        //    ILoyal.DataLayer.Customer operatorTransactionCustomer = customerRepository.GetOperatorTransactionsUser(out operatorTransactionsPaymentInstrument);

        //    bool defaultTransactionFee = false;
        //    long tariffItemId = 0;
        //    long feeChargeable = 0;
        //    //long feeChargeable = tariffRepository.GetTariffCharge((PITypes)customerDefaultWallet.PaymentInstrumentTypeId, 1, transactionTypeId, 0, out defaultTransactionFee, out tariffItemId);

        //    decimal senderBalanceBeforeTransaction = BalanceEnquiry(customerId, customerDefaultWallet.PaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceBeforeTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //    HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, 0, feeChargeable);

        //    string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //    MasterTransactionRecord transaction = new MasterTransactionRecord
        //    {
        //        Amount = 0,
        //        Fee = feeChargeable,
        //        ExternalApplicationId = 1,
        //        AccessChannelId = 1,
        //        IsTestTransaction = customer.IsTestCustomer,
        //        ShortDescription = RESET_SECURITY_CODE,
        //        PayerId = customerId,
        //        PayerPaymentInstrumentId = customerDefaultWallet.PaymentInstrumentId,
        //        PayeeId = operatorTransactionCustomer.CustomerId,
        //        PayeePaymentInstrumentId = operatorTransactionsPaymentInstrument.PaymentInstrumentId,
        //        Tax = 0,
        //        Text = string.Empty,
        //        TransactioErrorCodeId = 1,
        //        TransactionDate = DateTime.Now,
        //        TransactionTypeId = transactionTypeId,
        //        TransactionReference = transactionReference,
        //        CustomerTypeId = customer.CustomerTypeId,
        //        //SourceUserName = agentCustomer.UserName
        //        PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //        PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //        TransactionStatusId = TransactionState.Successful
        //    };

        //    context.MasterTransactionRecords.Add(transaction);
        //    long agentCommission = 0;

        //    CustomerTransactionGL senderTransactionGL = new CustomerTransactionGL
        //    {
        //        CustomerId = customerId,
        //        MasterTransactionRecordId = transaction.MasterTransactionRecordId,
        //        TransactionDate = transactionTime,
        //    };

        //    CustomerTransactionGL receiverTransactionGL = new CustomerTransactionGL
        //    {
        //        CustomerId = operatorTransactionCustomer.CustomerId,
        //        MasterTransactionRecordId = transaction.MasterTransactionRecordId,
        //        TransactionDate = transactionTime,
        //    };

        //    //long agentCommission = CalculateAgentCommission(1, transactionTypeId, 0);

        //    //FeesGL feeGl = new FeesGL
        //    //{
        //    //    MasterTransactionRecordId = transaction.MasterTransactionRecordId,
        //    //    CustomerFeeAmount = feeChargeable,
        //    //    TransactionCommission = agentCommission,
        //    //    GrossRevenue = feeChargeable - agentCommission,
        //    //    TransactionDate = transactionTime,
        //    //    TransactionDescription = RESET_SECURITY_CODE
        //    //};

        //    // HelperRepository.UpdateCustomerSVABalance(context, customerId, customerDefaultWallet, feeChargeable, transactionReference, false, applicationSettings.InstanceName + " " + RESET_SECURITY_CODE);

        //    HelperRepository.UpdateCustomerSVABalance(context, customerId, customerDefaultWallet, 0, feeChargeable, false);
        //    HelperRepository.UpdateCustomerSVABalance(context, operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument, feeChargeable, 0, true);

        //    context.CustomerTransactionGLs.Add(senderTransactionGL);
        //    context.CustomerTransactionGLs.Add(receiverTransactionGL);
        //    //context.FeesGLs.Add(feeGl);

        //    context.SaveChanges();

        //    decimal senderBalanceAfterTransaction = BalanceEnquiry(customerId, customerDefaultWallet.PaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceAfterTransaction = BalanceEnquiry(operatorTransactionCustomer.CustomerId, operatorTransactionsPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //    transaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //    transaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //    context.SaveChanges();

        //    Parallel.Invoke(() =>
        //    {
        //        messageRepository.ResetSecurityCodeSms(customer, transactionReference, newSecurityCode, transaction.MasterTransactionRecordId, transactionTime);
        //    },
        //    () =>
        //    {
        //        // analytics
        //    });

        //    return true;
        //}
        //public string PatchAgentOutletPIs()
        //{
        //    List<AgentOutlet> allAgentOutlets = context.AgentOutlets.ToList();
        //    int count = 0;
        //    //List<string> resultString = new List<string>
        //    StringBuilder resultString = new StringBuilder();
        //    foreach (AgentOutlet agentOutlet in allAgentOutlets)
        //    {
        //        //63385
        //        PaymentInstrument pi = GetAgentSva(agentOutlet.TillCustomerId);
        //        if (pi != null)
        //        {
        //            agentOutlet.PaymentInstrumentId = pi.PaymentInstrumentId;
        //            count++;
        //            //if (piCount != 1)
        //            //{//  agentOutlet.OutletName
        //            //    resultString.AppendLine(string.Format("Agent Name: {0}, Agent Number: {1}, Agent PI Count {2} <br>", agentOutlet.OutletName, agentOutlet.OutletNumber, piCount.ToString()));
        //            //}
        //        }
        //        //if (count > 5)
        //        //    break;
        //        //return resultString.ToString();
        //    }
        //    context.SaveChanges();
        //    return count.ToString();
        //}
        //public string SetCustomersAsActive()
        //{
        //    List<Customer> customers = context.Customers.ToList();
        //    int count = 0;
        //    int totalCount = 0;

        //    foreach (Customer customer in customers)
        //    {
        //        totalCount++;
        //        if (customer.CustomerId > 2 && customer.BlacklistReasonId == 1)
        //        {
        //            count++;
        //            customer.TermsAccepted = true;
        //            customer.TermsAcceptedDate = customer.CreatedDate;
        //        }
        //    }
        //    context.SaveChanges();
        //    return "Total Count :" + totalCount.ToString() + "<br>Updated: " + count.ToString();
        //}
        //public PaymentInstrument GetAgentSva(long customerId)
        //{
        //    PaymentInstrument pi = context.PaymentInstruments.SingleOrDefault(x => x.CustomerId == customerId && x.PaymentInstrumentTypeId == 2);

        //    return pi;

        //    //if (pi.Count > 1)
        //    //{
        //    //    return false;
        //    //    //throw new Exception("Customer has more than 1 AgentSva");
        //    //}
        //    //if (pi.Count == 0)
        //    //{
        //    //    throw new Exception("Customer has 0 AgentSva");
        //    //}
        //}

        //public ConsumerTransactionLimit GetConsumerTransactionLimit(int transactionTypeId)
        //{
        //    ConsumerTransactionLimit consumerLimit = context.ConsumerTransactionLimits.SingleOrDefault(x => x.TransactionTypeId == transactionTypeId);

        //    if (consumerLimit == null)
        //    {
        //        throw new Exception(String.Format("TRN0006 - The specified TransactionTypeId '{0}' has no limits set up", transactionTypeId));
        //    }
        //    return consumerLimit;
        //}

        //public long GetAirtimeBillerBalance()
        //{
        //    Customer customer = context.Customers.FirstOrDefault(x => x.FirstName == "DirectCore Airtime Biller");

        //    if (customer != null)
        //    {
        //        PaymentInstrument pi = context.PaymentInstruments.Single(x => x.CustomerId == customer.CustomerId && x.PaymentInstrumentTypeId == (int)PITypes.MPESA);
        //        return pi.AccountBalance;
        //    }
        //    return 999999;
        //}
        //public long GetMKTxnsUserBalance()
        //{
        //    Customer customer = context.Customers.SingleOrDefault(x => x.FirstName == "DirectCore Transactions User");

        //    if (customer != null)
        //    {
        //        PaymentInstrument pi = context.PaymentInstruments.Single(x => x.CustomerId == customer.CustomerId && x.PaymentInstrumentTypeId == (int)PITypes.MPESA);
        //        return pi.AccountBalance;
        //    }
        //    return 999999;
        //}
        ////TODO Remove after Import
        //public List<long> GetPIIDWithTypeTen()
        //{
        //    List<PaymentInstrument> paymentInstruments = context.PaymentInstruments.Where(x => x.PaymentInstrumentTypeId == 10).ToList();
        //    List<long> piids = new List<long>();
        //    //foreach (PaymentInstrument pi in paymentInstruments)
        //    //{
        //    //    piids.Add(pi.PaymentInstrumentId);
        //    //}
        //    //         return
        //    paymentInstruments.ForEach(pi => piids.Add(pi.PaymentInstrumentId));
        //    return piids;
        //}
        ////TODO Remove after Import
        //public PaymentInstrument GetMKPIbyPId(long pid)
        //{
        //    return context.PaymentInstruments.SingleOrDefault(x => x.PaymentInstrumentId == pid);
        //}
        //public int MKPIAlreadyExists(long pid)
        //{
        //    PaymentInstrument pi = context.PaymentInstruments.SingleOrDefault(x => x.PaymentInstrumentId == pid);
        //    int count = 0;
        //    if (pi != null)
        //    {
        //        count = context.PaymentInstruments.Count(x => x.CustomerId == pi.CustomerId && x.PaymentInstrumentTypeId == 1);
        //    }

        //    return count;
        //}
        //public bool UpdateCustomersWithSinglePiType10(long pid)
        //{
        //    string accountName = "ILoyal";
        //    PaymentInstrument pi = context.PaymentInstruments.Find(pid);

        //    if (pi != null)
        //    {
        //        List<PaymentInstrument> allPis = context.PaymentInstruments.Where(x => x.CustomerId == pi.CustomerId).ToList();

        //        if (allPis.Count == 1 && allPis[0].PaymentInstrumentTypeId == 10)
        //        {
        //            allPis[0].PaymentInstrumentTypeId = 1;
        //            allPis[0].AccountNumber = accountName;
        //            allPis[0].PaymentIntrumentAlias = accountName;
        //            allPis[0].IsActive = true;
        //            context.SaveChanges();
        //            return true;
        //        }
        //        else
        //        { return false; }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        //public bool UpdateCustomersWithPiType10(long pid)
        //{
        //    string accountName = "ILoyal";

        //    PaymentInstrument pi = context.PaymentInstruments.Find(pid);
        //    pi.PaymentInstrumentTypeId = 1;
        //    pi.AccountNumber = accountName;
        //    pi.PaymentIntrumentAlias = accountName;
        //    pi.IsActive = true;
        //    context.SaveChanges();
        //    return true;
        //}
        //private string GetBalanceFromDecimal(decimal amount)
        //{
        //    if (amount == 0)
        //    {
        //        return "0";
        //    }

        //    amount = amount / 100;
        //    return amount.ToString("#,###");
        //}

        //public bool TempFloatPurchase(long amount)
        //{
        //    int transactionTypeId = 1;
        //    DateTime transactionTime = DateTime.Now;

        //    Customer customer = context.Customers.Find(6273);

        //    PaymentInstrument destinationPaymentInstrument = context.PaymentInstruments.Find(8913);
        //    PaymentInstrument sourcePaymentInstrument = context.PaymentInstruments.Find(8916);

        //    decimal senderMcommerceBalanceBeforeTransaction = BalanceEnquiry(customer.CustomerId, destinationPaymentInstrument.PaymentInstrumentId, string.Empty, false); ;
        //    decimal receiverBalanceBeforeTransaction = BalanceEnquiry(customer.CustomerId, destinationPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //    long feeAmount = 0;
        //    bool defaultTransactionFee = false;
        //    long tariffItemId = 0;

        //    //  feeAmount = tariffRepository.GetTariffCharge(PITypes.MPESA, 1, transactionTypeId, amount, out defaultTransactionFee, out tariffItemId);
        //    feeAmount = 3000;
        //    string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //    long customerMsisdn = new CustomerRepository().GetCustomerPhoneNumber(customer.CustomerId);

        //    string fullName = string.Empty;

        //    if (customer.FullyRegistered)
        //    {
        //        fullName = customer.FirstName + " " + customer.LastName;
        //    }
        //    else
        //    {
        //        fullName = "Unregistered User";
        //    }

        //    MasterTransactionRecord newSendMoneyTransaction = new MasterTransactionRecord
        //    {
        //        TransactionTypeId = transactionTypeId,
        //        PayerId = customer.CustomerId,
        //        PayeeId = customer.CustomerId,
        //        PayerPaymentInstrumentId = sourcePaymentInstrument.PaymentInstrumentId,
        //        PayeePaymentInstrumentId = destinationPaymentInstrument.PaymentInstrumentId,
        //        ExternalApplicationId = 1,
        //        AccessChannelId = 1,
        //        Amount = amount,
        //        IsBankingTransaction = true,
        //        Fee = feeAmount,
        //        CustomerTypeId = customer.CustomerTypeId,
        //        Tax = 0,
        //        Text = fullName + "( " + customerMsisdn.ToString() + " )",
        //        Text2 = "Master Float Topup",
        //        Text3 = "",
        //        TransactioErrorCodeId = 1,
        //        IsTestTransaction = customer.IsTestCustomer,
        //        TransactionDate = transactionTime,
        //        TransactionReference = transactionReference,
        //        ShortDescription = SEND_MONEY,
        //        PayerBalanceBeforeTransaction = null,
        //        PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //        PayerBalanceAfterTransaction = null,
        //        TransactionStatusId = TransactionState.Successful
        //    };

        //    context.MasterTransactionRecords.Add(newSendMoneyTransaction);

        //    #region Revenue Assuarance Obligations

        //    // HelperRepository.UpdateCustomerSVABalance(context, senderCustomerId, amount, feeAmount, false);
        //    HelperRepository.UpdateCustomerSVABalance(context, customer.CustomerId, destinationPaymentInstrument, amount, feeAmount, true);

        //    CustomerTransactionGL senderGlTransaction = new CustomerTransactionGL()
        //    {
        //        CustomerId = customer.CustomerId,
        //        MasterTransactionRecordId = newSendMoneyTransaction.MasterTransactionRecordId,
        //        TransactionDate = transactionTime
        //    };

        //    //CustomerTransactionGL recipientGlTransaction = new CustomerTransactionGL()
        //    //{
        //    //    CustomerId = customer.CustomerId,
        //    //    MasterTransactionRecordId = newSendMoneyTransaction.MasterTransactionRecordId,
        //    //    TransactionDate = transactionTime,
        //    //};

        //    //long agentCommssion = agentRepository.CalculateAgentCommission(1, transactionTypeId, feeAmount);
        //    long agentCommssion = 0;

        //    //FeesGL feeGl = new FeesGL
        //    //{
        //    //    MasterTransactionRecordId = newSendMoneyTransaction.MasterTransactionRecordId,
        //    //    CustomerFeeAmount = feeAmount,
        //    //    TransactionCommission = agentCommssion,
        //    //    GrossRevenue = feeAmount - agentCommssion,
        //    //    TransactionDate = transactionTime,
        //    //    TransactionDescription = SEND_MONEY
        //    //};

        //    #endregion Revenue Assuarance Obligations

        //    context.CustomerTransactionGLs.Add(senderGlTransaction);
        //    // context.CustomerTransactionGLs.Add(recipientGlTransaction);
        //    //context.FeesGLs.Add(feeGl);
        //    context.SaveChanges();

        //    decimal receiverBalanceAfterTransaction = BalanceEnquiry(customer.CustomerId, destinationPaymentInstrument.PaymentInstrumentId, string.Empty, false);
        //    decimal senderMcommerceBalanceAfterTransaction = BalanceEnquiry(customer.CustomerId, destinationPaymentInstrument.PaymentInstrumentId, string.Empty, false);

        //    newSendMoneyTransaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());
        //    context.SaveChanges();

        //    Parallel.Invoke(() =>
        //    {
        //        messageRepository.SendMoneyBankToWalletSms(customer, customer, amount, newSendMoneyTransaction, "Postbank", "", "");
        //    }
        //    //() =>
        //    //{
        //    //    ManageAnalytics manageAnalytics = new ManageAnalytics();

        //    //    Analytics.Entities.Transaction sendMoneyMetrics = new Analytics.Entities.Transaction
        //    //    {
        //    //        TransactionType = TransactionGroups.SendMoney.ToString(),
        //    //        TransactionSubType = TransactionTypes.SendMoney.ToString(),
        //    //        TransactionTime = transactionTime,
        //    //        SourceCustomerType = customer.CustomerType.CustomerTypeName,
        //    //        DestinationCustomerType = customer.CustomerType.CustomerTypeName,
        //    //        AccessChannel = "USSD",
        //    //        ExternalApplication = "USSD App",
        //    //        Amount = amount,
        //    //        Fee = feeAmount,
        //    //        IsbankingTransaction = newSendMoneyTransaction.IsBankingTransaction,
        //    //        IsTestTransacion = newSendMoneyTransaction.IsTestTransaction,
        //    //        FIName = "PB"
        //    //    };
        //    //    manageAnalytics.SaveRecord(AnalyticTypes.SendMoney, customer.CustomerId, JsonConvert.SerializeObject(sendMoneyMetrics));
        //    //}

        //    );
        //    return true;
        //}
        //public ReverseTransactionResult ReverseFloatWalletTransaction(long transactionId)
        //{
        //    try
        //    {
        //        if (transactionId != 5862)
        //        {
        //            throw new Exception("This is not a valid Transaction");
        //        }

        //        if (!_transactionAppSettings.ReversalTransactionActive)
        //            return ReverseTransactionResult.TransactionReversalDisabled;

        //        MasterTransactionRecord transactionToReverse = context.MasterTransactionRecords.Find(transactionId);

        //        if (transactionToReverse == null)
        //        {
        //            throw new Exception("TRN0011 - The TransactionId '" + transactionId.ToString() + "' does not exist");
        //        }

        //        if (transactionToReverse != null)
        //        {
        //            ReversibleTransaction reversibleTransaction = context.ReversibleTransactions.SingleOrDefault(x => x.TransactionTypeId == transactionToReverse.TransactionTypeId);

        //            if (reversibleTransaction == null)
        //            {
        //                return ReverseTransactionResult.TransactionNotReversible;
        //            }

        //            // is this tranction type allowed to be reversed
        //            if (reversibleTransaction.IsReversible)
        //            {
        //                //is transaction within the allowable reversal timespan
        //                //if ((DateTime.Now.Subtract(TimeSpan.FromMinutes(Convert.ToDouble(reversibleTransaction.ReversiblePeriod))) > transactionToReverse.TransactionDate))
        //                //{
        //                // check if payer has funds in wallet then proceed.

        //                //TODO implement charges for transaction reversal

        //                MasterTransactionRecord newReversalTransaction = new MasterTransactionRecord();
        //                // newReversalTransaction = transactionToReverse;
        //                newReversalTransaction.TransactionTypeId = 14;
        //                newReversalTransaction.ShortDescription = transactionToReverse.ShortDescription + "(Rv)";
        //                newReversalTransaction.TransactionReference = transactionToReverse.TransactionReference + "(Rv)";
        //                newReversalTransaction.PayeeId = transactionToReverse.PayerId;
        //                newReversalTransaction.PayerId = transactionToReverse.PayeeId;
        //                newReversalTransaction.PayeePaymentInstrumentId = transactionToReverse.PayerPaymentInstrumentId;
        //                newReversalTransaction.PayerPaymentInstrumentId = transactionToReverse.PayeePaymentInstrumentId;
        //                newReversalTransaction.TransactioErrorCodeId = 1;
        //                newReversalTransaction.Amount = transactionToReverse.Amount + transactionToReverse.Fee;
        //                newReversalTransaction.Fee = 0;
        //                newReversalTransaction.Tax = 0;
        //                newReversalTransaction.IsBankingTransaction = transactionToReverse.IsBankingTransaction;
        //                newReversalTransaction.IsTestTransaction = transactionToReverse.IsTestTransaction;
        //                newReversalTransaction.FITransactionCode = transactionToReverse.FITransactionCode;
        //                newReversalTransaction.TransactionDate = DateTime.Now;
        //                newReversalTransaction.Text = transactionToReverse.Text;
        //                newReversalTransaction.Text2 = "Transaction ID: " + transactionToReverse.MasterTransactionRecordId;
        //                newReversalTransaction.Text3 = transactionToReverse.Text3;
        //                newReversalTransaction.AccessChannelId = 1;
        //                newReversalTransaction.ExternalApplicationId = 1;
        //                newReversalTransaction.CustomerTypeId = transactionToReverse.CustomerTypeId;
        //                newReversalTransaction.TransactionStatusId = TransactionState.Successful;

        //                if (newReversalTransaction.TransactionTypeId == 2)
        //                {
        //                    AgentOutlet sendingAgent = context.AgentOutlets.Single(x => x.TillCustomerId == transactionToReverse.PayeeId);
        //                    AgentOutlet receivingAgent = context.AgentOutlets.Single(x => x.TillCustomerId == transactionToReverse.PayerId);

        //                    newReversalTransaction.Text = "From: " + sendingAgent.OutletNumber + "(" + sendingAgent.OutletName + ") - To: " + receivingAgent.OutletNumber + "(" + receivingAgent.OutletName + ")";
        //                }

        //                decimal senderBalanceBeforeTransaction = BalanceEnquiry(transactionToReverse.PayeeId, transactionToReverse.PayeePaymentInstrumentId, string.Empty, false);
        //                //decimal receiverBalanceBeforeTransaction = BalanceEnquiry(transactionToReverse.PayerId, transactionToReverse.PayerPaymentInstrumentId, string.Empty, false);

        //                HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, transactionToReverse.Amount + transactionToReverse.Fee, 0);

        //                newReversalTransaction.PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString());
        //                newReversalTransaction.PayeeBalanceBeforeTransaction = null;

        //                context.MasterTransactionRecords.Add(newReversalTransaction);
        //                transactionToReverse.TransactionStatusId = TransactionState.Reversed;
        //                context.SaveChanges();

        //                PaymentInstrument payerPaymentInstrument = context.PaymentInstruments.Find(newReversalTransaction.PayerPaymentInstrumentId);
        //                PaymentInstrument payeePaymentInstrument = context.PaymentInstruments.Find(newReversalTransaction.PayeePaymentInstrumentId);

        //                HelperRepository.UpdateCustomerSVABalance(context, newReversalTransaction.PayerId, payerPaymentInstrument, newReversalTransaction.Amount - transactionToReverse.Fee, 0, false);
        //                //  HelperRepository.UpdateCustomerSVABalance(context, newReversalTransaction.PayeeId, payeePaymentInstrument, newReversalTransaction.Amount, 0, true);

        //                decimal senderBalanceAfterTransaction = BalanceEnquiry(newReversalTransaction.PayerId, newReversalTransaction.PayerPaymentInstrumentId, string.Empty, false);
        //                //decimal receiverBalanceAfterTransaction = BalanceEnquiry(newReversalTransaction.PayeeId, newReversalTransaction.PayeePaymentInstrumentId, string.Empty, false);

        //                newReversalTransaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //                newReversalTransaction.PayeeBalanceAfterTransaction = null;

        //                context.SaveChanges();

        //                long payerMsisdn = customerRepository.GetCustomerPhoneNumber(newReversalTransaction.PayerId);
        //                long payeeMsisdn = customerRepository.GetCustomerPhoneNumber(newReversalTransaction.PayeeId);

        //                //long payerMsisdn = 254723505347;
        //                //long payeeMsisdn = 254722781404;

        //                string payerMessage = "Confirmed " + newReversalTransaction.TransactionReference + ". Your " + transactionToReverse.ShortDescription + " transaction reference " + transactionToReverse.TransactionReference + " of KSh. " + GetBalanceFromDecimal(newReversalTransaction.Amount - transactionToReverse.Fee) + " reversed without charges on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + ". New DirectCore Balance is KSh." + GetBalanceFromDecimal(senderBalanceAfterTransaction);
        //                string payeeMessage = "Confirmed " + newReversalTransaction.TransactionReference + ". Your " + transactionToReverse.ShortDescription + " transaction reference " + transactionToReverse.TransactionReference + " of KSh. " + GetBalanceFromDecimal(newReversalTransaction.Amount) + " reversed without charges on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + ". New DirectCore Balance is KSh." + 0;

        //                //MCommerceTextService textService = new MCommerceTextService();

        //                //  textService.SendInfobipSms(payeeMessage.ToString(), payeeMessage);
        //                //   textService.SendInfobipSms(payerMsisdn.ToString(), payerMessage);

        //                Parallel.Invoke(() =>
        //                {
        //                    new MessageRepository().SendTransactionReversalSms(newReversalTransaction.MasterTransactionRecordId, payerMsisdn.ToString(), payeeMsisdn.ToString(), payerMessage, payeeMessage);
        //                },
        //                                () =>
        //                                {
        //                                    // analytics
        //                                });

        //                return ReverseTransactionResult.TransactionBookedForReversal;
        //                //}
        //                //else
        //                //{
        //                //    return ReverseTransactionResult.ReversalTimespanExceeded;
        //                //}
        //            }
        //            else
        //            {
        //                return ReverseTransactionResult.TransactionNotReversible;
        //            }
        //        }
        //        else
        //        {
        //            return ReverseTransactionResult.NoSuchTransaction;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    // }
        //    //  return true;
        //}
        //TODO to be removed after the first run
        //public void UpdateAirtimeRequestsNeedingReversal()
        //{
        //    List<MasterTransactionRecord> failedAirtimeRequests = context.MasterTransactionRecords.Where(x => x.Text5 == "Escalated Case").ToList();
        //    List<EscalatedAirtimeRequest> escalatedRequests = new List<EscalatedAirtimeRequest>();

        //    foreach (MasterTransactionRecord transactionRecord in failedAirtimeRequests)
        //    {
        //        escalatedRequests.Add(new EscalatedAirtimeRequest
        //        {
        //            MasterTransactionRecordId = transactionRecord.MasterTransactionRecordId,
        //            TimeQueued = transactionRecord.TransactionDate,
        //            AirtimeEscalationStatusId = (int)AirtimeEscalation.Queued,
        //            UpdatedTime = null
        //        });
        //    }
        //    context.EscalatedAirtimeRequests.AddRange(escalatedRequests);
        //    //   context.SaveChanges();
        //}

        //public int UpdateClientPaybillOtherParty()
        //{
        //    //client paybill
        //    int transactionTypeId = 5;

        //    List<MasterTransactionRecord> transactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == transactionTypeId).ToList();

        //    foreach (MasterTransactionRecord transaction in transactions)
        //    {
        //        string shortDesc = transaction.ShortDescription;
        //        ILoyal.DataLayer.Customer customer = customerRepository.GetCustomerByCustomerId(transaction.PayerId);
        //        long customerMsisdn = customerRepository.GetCustomerPhoneNumber(transaction.PayeeId);

        //        Biller biller = context.Billers.First(x => x.CustomerId == transaction.PayeeId);
        //        transaction.Text = "From: " + customer.FirstName + " " + customer.LastName + "( " + customerMsisdn.ToString() + " ) - To: " + biller.BillerName + " ( " + biller.BillerNumber + " )";
        //    }

        //    context.SaveChanges();
        //    return transactions.Count;
        //}
        //public int UpdateAgentPaybillOtherParty()
        //{
        //    //client paybill
        //    int transactionTypeId = 6;

        //    List<MasterTransactionRecord> transactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == transactionTypeId).ToList();

        //    foreach (MasterTransactionRecord transaction in transactions)
        //    {
        //        string shortDesc = transaction.ShortDescription;

        //        //AgentOutlet outlet = agentRepository.GetOutletByCustomerId(transaction.PayerId);
        //        //long customerMsisdn = customerRepository.GetCustomerPhoneNumber(transaction.PayeeId);

        //        Biller biller = context.Billers.First(x => x.CustomerId == transaction.PayeeId);
        //        //transaction.Text = "From: " + outlet.OutletName + " ( " + outlet.OutletNumber + " ) - To: " + biller.BillerName + " ( " + biller.BillerNumber + " )";
        //    }

        //    context.SaveChanges();
        //    return transactions.Count;
        //}
        //public void GetAllCashoutRecords()
        //{
        //    List<MasterTransactionRecord> cashOutRecords = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == 2).ToList();
        //    int count = 0;
        //    foreach (MasterTransactionRecord transaction in cashOutRecords)
        //    {
        //        long payerPid = context.PaymentInstruments.Single(x => x.CustomerId == transaction.PayerId && x.AccountNumber == "DirectCore" && x.PaymentInstrumentTypeId == 1).PaymentInstrumentId;

        //        transaction.PayerPaymentInstrumentId = payerPid;
        //        count++;
        //    }
        //    context.SaveChanges();
        //}
        //public int UpdateCashInOtherParty()
        //{
        //    int transactionTypeId = 3;

        //    List<MasterTransactionRecord> transactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == transactionTypeId && x.TransactionDate > new DateTime(2014, 3, 20) && x.TransactionDate < new DateTime(2014, 3, 28)).ToList();

        //    foreach (MasterTransactionRecord transaction in transactions)
        //    {
        //        ILoyal.DataLayer.Customer customer = customerRepository.GetCustomerByCustomerId(transaction.PayeeId);
        //        long customerMsisdn = customerRepository.GetCustomerPhoneNumber(transaction.PayeeId);
        //        AgentOutlet agentOutlet = context.AgentOutlets.Single(x => x.TillCustomerId == transaction.PayerId);

        //        transaction.Text = "From: " + agentOutlet.OutletName + "(" + agentOutlet.OutletNumber + ") To: " + customer.FirstName + " " + customer.LastName + "( " + customerMsisdn.ToString() + " )";
        //    }
        //    context.SaveChanges();
        //    return transactions.Count;
        //}
        //public int UpdateCashOutOtherParty()
        //{
        //    int transactionTypeId = 2;

        //    List<MasterTransactionRecord> transactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == transactionTypeId && x.TransactionDate > new DateTime(2014, 3, 20) && x.TransactionDate < new DateTime(2014, 3, 28)).ToList();

        //    foreach (MasterTransactionRecord transaction in transactions)
        //    {
        //        ILoyal.DataLayer.Customer customer = customerRepository.GetCustomerByCustomerId(transaction.PayerId);
        //        long customerMsisdn = customerRepository.GetCustomerPhoneNumber(transaction.PayerId);
        //        AgentOutlet agentOutlet = context.AgentOutlets.Single(x => x.TillCustomerId == transaction.PayeeId);

        //        transaction.Text = "From: " + customer.FirstName + " " + customer.LastName + "( " + customerMsisdn.ToString() + " ) To: " + agentOutlet.OutletName + "(" + agentOutlet.OutletNumber + ")";
        //    }
        //    context.SaveChanges();
        //    return transactions.Count;
        //}
        //public int UpdateSendMoneyOtherParty()
        //{
        //    int transactionTypeId = 1;

        //    List<MasterTransactionRecord> transactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == transactionTypeId).ToList();

        //    foreach (MasterTransactionRecord transaction in transactions)
        //    {
        //        try
        //        {
        //            ILoyal.DataLayer.Customer receiver = customerRepository.GetCustomerByCustomerId(transaction.PayeeId);
        //            ILoyal.DataLayer.Customer sender = customerRepository.GetCustomerByCustomerId(transaction.PayerId);

        //            string fullName = string.Empty;
        //            if (receiver.FullyRegistered)
        //            {
        //                fullName = receiver.FirstName + " " + receiver.LastName;
        //            }
        //            else
        //            {
        //                fullName = "Unregistered User";
        //            }

        //            long receiverMsisdn = customerRepository.GetCustomerPhoneNumber(transaction.PayeeId);
        //            long senderMsisdn = customerRepository.GetCustomerPhoneNumber(transaction.PayerId);

        //            string senderDetails = "From: " + sender.FirstName + " " + sender.LastName + "( " + senderMsisdn.ToString() + " ) To: ";

        //            transaction.Text = senderDetails + fullName + "( " + receiverMsisdn.ToString() + " )";
        //        }
        //        catch (Exception ex)
        //        {
        //            throw;
        //        }
        //    }
        //    context.SaveChanges();
        //    return transactions.Count;
        //}
        //public int UpdateAirtimeSalesId()
        //{
        //    List<MasterTransactionRecord> airtimeTransactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == 4).ToList();
        //    int overallCount = 0;
        //    int agentTxnsCount = 0;

        //    foreach (MasterTransactionRecord transactionRecord in airtimeTransactions)
        //    {
        //        ILoyal.DataLayer.Customer customer = context.Customers.Find(transactionRecord.PayerId);

        //        long customerMsisdn = customerRepository.GetCustomerPhoneNumber(customer.CustomerId);

        //        if (customer.CustomerTypeId == 2 && (!string.IsNullOrEmpty(transactionRecord.Text2)))
        //        {
        //            if (customerMsisdn.ToString() != transactionRecord.Text2)
        //            {
        //                transactionRecord.TransactionTypeId = 34;
        //                transactionRecord.ShortDescription = AIRTIME_SALE;
        //                context.SaveChanges();
        //                agentTxnsCount++;
        //            }
        //        }
        //        overallCount++;
        //    }
        //    return airtimeTransactions.Count;
        //}
        //public void UpdateTransactionStatusOnTxn()
        //{
        //    List<MasterTransactionRecord> reversalTransactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == 14).ToList();
        //    int totalCount = 0;
        //    //     int emptyText4Count = 0;

        //    foreach (MasterTransactionRecord transactionRecord in reversalTransactions)
        //    {
        //        long originalTransactionId = Convert.ToInt64(transactionRecord.Text4);

        //        MasterTransactionRecord originalTransaction = context.MasterTransactionRecords.Find(originalTransactionId);

        //        if (originalTransaction == null)
        //        {
        //            string ikoNull = "";
        //            return;
        //        }

        //        originalTransaction.TransactionStatusId = TransactionState.Reversed;
        //        context.SaveChanges();

        //        totalCount++;
        //    }
        //}
        ////TODO Temp methods to be removed.
        //public string SortAgentswithMultipleSva()
        //{
        //    List<AgentOutlet> allActiveOutlets = context.AgentOutlets.Where(x => x.IsActive == true).ToList();
        //    StringBuilder sb = new StringBuilder();

        //    foreach (AgentOutlet outlet in allActiveOutlets)
        //    {
        //        int asvaPiCount = context.PaymentInstruments.Count(x => x.CustomerId == outlet.TillCustomerId && x.PaymentInstrumentTypeId == (int)PITypes.MPESA);

        //        if (asvaPiCount > 1)
        //        {
        //            //customer
        //            sb.AppendLine(outlet.OutletName + " Outlet Number: " + outlet.OutletNumber + " Outlet PI Count:" + asvaPiCount + "<br>");
        //        }

        //        int mkPiCount = context.PaymentInstruments.Count(x => x.CustomerId == outlet.TillCustomerId && x.PaymentInstrumentTypeId == (int)PITypes.MPESA);

        //        if (mkPiCount > 1)
        //        {
        //            sb.AppendLine(outlet.OutletName + " Outlet Number: " + outlet.OutletNumber + " MK Account Count:" + asvaPiCount + "<br>");
        //        }
        //    }

        //    return sb.ToString();
        //}
        //public List<MasterTransactionRecord> GetAllReversedTransactions(DateTime startDate)
        //{
        //    DateTime preciseStartDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day);
        //    return context.MasterTransactionRecords.Where(x => x.TransactionTypeId == 14 && x.TransactionDate >= preciseStartDateTime).ToList();
        //}
        ////TODO remove- patch   run before paying may commissions
        //public void UpdateReversedTransactionsFees()
        //{
        //    List<MasterTransactionRecord> allReversalTransactions = context.MasterTransactionRecords.Where(x => x.TransactionTypeId == 14).OrderByDescending(x => x.MasterTransactionRecordId).ToList();
        //    int count = 1;

        //    foreach (MasterTransactionRecord transactionRecord in allReversalTransactions)
        //    {
        //        MasterTransactionRecord originalTransaction = GetTransactionByTransactionId(long.Parse(transactionRecord.Text4));

        //        //if (originalTransaction.Fee > 0)
        //        //{
        //        transactionRecord.Text5 = transactionRecord.Text2;
        //        transactionRecord.Text2 = originalTransaction.Fee.ToString();
        //        //}
        //        count++;
        //    }
        //    context.SaveChanges();
        //    // MasterTransactionRecord transactionRecord = allReversalTransactions.First();
        //}
        ////Todo delete
        ////public void RedistributeReversals()
        ////{
        ////    DateTime date = DateTime.Now.AddDays(1);
        ////    List<FeesGL> feesGls = context.FeesGLs.Where(x => x.TransactionDate > date).ToList();
        ////    int count = 1;
        ////    foreach (FeesGL feesGl in feesGls)
        ////    {
        ////        MasterTransactionRecord transaction = GetTransactionByTransactionId(feesGl.MasterTransactionRecordId);

        ////        if (transaction.TransactionTypeId == 14)
        ////        {
        ////            feesGl.TransactionDate = transaction.TransactionDate;
        ////        }
        ////        count++;
        ////    }

        ////    //   context.SaveChanges();
        ////}

        ////TODO delete - was done to resend the agent commissions in may 2014
        //public void ResendMayCommissionPaymentSms()
        //{
        //    //List<MessageQueue> messageQueueItems = context.MessagesQueue.Where(x => x.MessageQueueId > 142641 && x.MessageQueueId < 143182).ToList();
        //    //MCommerceTextService textService = new MCommerceTextService();

        //    //foreach (MessageQueue queueItem in messageQueueItems)
        //    //{
        //    //    //queueItem.MessageQueueId

        //    //    bool deliveryNote = textService.SendSingleSms(queueItem.MessageRecipients, queueItem.ActualMessageBody, queueItem.MessageQueueId.ToString());
        //    //    queueItem.TimeSent = DateTime.Now;
        //    //    queueItem.FullDeliveryNote = deliveryNote.ToString();
        //    //    queueItem.MessageProcessingStatusId = (int)MessageProcessingState.QueuedToProvider;
        //    //    context.SaveChanges();
        //    //}
        //}
        //public List<TransactionSummary> GetTransactionSummary(DateTime startDate, DateTime endDate)
        //{
        //    DateTime preciseStartDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day);
        //    DateTime preciseEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day).AddDays(1).AddMilliseconds(-1);

        //    List<MasterTransactionRecord> transactionRecords = context.MasterTransactionRecords.Where(x => x.TransactionDate > preciseStartDateTime && x.TransactionDate < preciseEndDate).ToList(); //&& x.TransactionTypeId != (int)TransactionTypes.ReversalTransaction

        //    List<MasterTransactionRecord> transactionRecordsWithoutReversals = transactionRecords.Where(x => x.TransactionTypeId != (int)TransactionTypes.ReversalTransaction).ToList();
        //    List<MasterTransactionRecord> transactionRecordsReversals = transactionRecords.Where(x => x.TransactionTypeId == (int)TransactionTypes.ReversalTransaction).ToList();

        //    var records = from sd in transactionRecordsWithoutReversals
        //                  group sd by sd.TransactionTypeId into g
        //                  select new { TransactionTypeId = g.Key, TransactionCount = g.Count(), TotalAmount = g.Sum(x => x.Amount), TotalFee = g.Sum(x => x.Fee) };

        //    var recordReversals = from sd in transactionRecordsReversals
        //                          group sd by sd.TransactionTypeId into g
        //                          select new { TransactionTypeId = g.Key, TransactionCount = g.Count(), TotalAmount = g.Sum(x => x.Amount), TotalFee = g.Sum(x => Convert.ToInt64(x.Text2)) };

        //    List<TransactionSummary> transactionSummary = new List<TransactionSummary>();

        //    foreach (var g in records)
        //    {
        //        TransactionSummary summary = new TransactionSummary

        //        {
        //            TransactionTypeId = g.TransactionTypeId,
        //            TotalAmount = g.TotalAmount,
        //            TotalFee = g.TotalFee,
        //            TransactionCount = g.TransactionCount,
        //            //TransactionName = g.
        //        };
        //        transactionSummary.Add(summary);
        //    }
        //    recordReversals.ToList().ForEach(rev => transactionSummary.Add(
        //                                            new TransactionSummary
        //                                            {
        //                                                TransactionTypeId = rev.TransactionTypeId,
        //                                                TotalAmount = rev.TotalAmount,
        //                                                TotalFee = rev.TotalFee,
        //                                                TransactionCount = rev.TransactionCount
        //                                            }));


        //    return transactionSummary;
        //}


        //public bool DisableTransactionType(int transactionTypeId)
        //{
        //    TransactionType result = context.TransactionTypes.Find(transactionTypeId);

        //    if (result == null)
        //    {
        //        throw new Exception(String.Format("TRN0011 - The TransactionId '{0}' does not exist.", transactionTypeId));
        //    }

        //    result.IsActive = false;
        //    context.SaveChanges();

        //    return true;
        //}
        //public bool EnableTransactionType(int transactionTypeId)
        //{
        //    TransactionType result = context.TransactionTypes.Find(transactionTypeId);

        //    if (result == null)
        //    {
        //        throw new Exception(String.Format("TRN0011 - The TransactionId '{0}' does not exist.", transactionTypeId));
        //    }

        //    result.IsActive = true;
        //    context.SaveChanges();

        //    return true;
        //}
        //public MasterTransactionRecord UpdateFiTrustAccountBalance(long financialInstitutionId, long paymentInstrumentId, long amount, int transactionTypeId, int accessChannelId, int customerTypeId, bool credit, MasterTransactionRecord transactionToUpdate = null)
        //{
        //    try
        //    {
        //        DateTime transactionTime = DateTime.Now;
        //        HelperRepository helperRepository = new HelperRepository();

        //        PaymentInstrument trustAccountPaymentInstrument = new PaymentInstrument();


        //        trustAccountPaymentInstrument = context.PaymentInstruments.FirstOrDefault(pi => pi.FinancialInstitutionId == financialInstitutionId && pi.PaymentInstrumentTypeId == (int)PITypes.MPESA);

        //        if (trustAccountPaymentInstrument == null) throw new Exception("FI0004 - The Financial Institution Id " + financialInstitutionId + " doesn’t have an Trust Account set");

        //        ILoyal.DataLayer.Customer trustAccountCustomer = context.Customers.Find(trustAccountPaymentInstrument.CustomerId);

        //        PaymentInstrument customerPaymentInstrument = context.PaymentInstruments.Find(paymentInstrumentId);

        //        ILoyal.DataLayer.Customer customer = customerPaymentInstrument.Customer;

        //        TransactionType transactionType = helperRepository.GetTransactionTypeByTransactionTypeId(transactionTypeId);

        //        if (transactionType == null) throw new Exception("TRN0024 - The specified TransactionTypeId " + transactionType.TransactionTypeId + " does not exist.");

        //        //if (!transactionType.IsActive) throw new Exception("");           

        //        string trustAccountCustomerName = String.Format("{0} {1}", trustAccountCustomer.FirstName, trustAccountCustomer.LastName);
        //        string customerName = String.Format("{0} {1}", customer.FirstName, customer.LastName);

        //        string textString = String.Format("From: {0} To: {1}", credit ? customerName : trustAccountCustomerName, credit ? trustAccountCustomerName : customerName);
        //        string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //        long payerId = credit ? customerPaymentInstrument.CustomerId : trustAccountCustomer.CustomerId;
        //        long payeeId = credit ? trustAccountPaymentInstrument.CustomerId : customerPaymentInstrument.CustomerId;
        //        long payerBalanceBeforeTransaction = credit ? customerPaymentInstrument.AccountBalance : trustAccountPaymentInstrument.AccountBalance;
        //        long payeeBalanceBeforeTransaction = credit ? trustAccountPaymentInstrument.AccountBalance : customerPaymentInstrument.AccountBalance;
        //        long payerPaymentInstrumentId = credit ? customerPaymentInstrument.PaymentInstrumentId : trustAccountPaymentInstrument.PaymentInstrumentId;
        //        long payeePaymentInstrumentId = credit ? trustAccountPaymentInstrument.PaymentInstrumentId : customerPaymentInstrument.PaymentInstrumentId;
        //        string sourceUserName = credit ? customer.UserName : trustAccountCustomer.UserName;
        //        string destinationUserName = credit ? trustAccountCustomer.UserName : customer.UserName;

        //        amount = credit ? amount : (amount * -1);

        //        trustAccountPaymentInstrument.AccountBalance += amount;
        //        //if (completeTransaction) customerPaymentInstrument.AccountBalance -= amount;

        //        //context.SaveChanges();

        //        long payerBalanceAfterTransaction = credit ? customerPaymentInstrument.AccountBalance : trustAccountPaymentInstrument.AccountBalance;
        //        long payeeBalanceAfterTransaction = credit ? trustAccountPaymentInstrument.AccountBalance : customerPaymentInstrument.AccountBalance;

        //        if (transactionToUpdate != null)
        //        {
        //            if (credit)
        //            {
        //                transactionToUpdate.PayeeId = payeeId;
        //                transactionToUpdate.PayeePaymentInstrumentId = payeePaymentInstrumentId;
        //                transactionToUpdate.PayeeBalanceBeforeTransaction = payeeBalanceBeforeTransaction;
        //                transactionToUpdate.DestinationUserName = destinationUserName;
        //                transactionToUpdate.PayeeBalanceAfterTransaction = payeeBalanceAfterTransaction;

        //                context.SaveChanges();

        //                return transactionToUpdate;
        //            }

        //            transactionToUpdate.PayerId = payerId;
        //            transactionToUpdate.PayerPaymentInstrumentId = payerPaymentInstrumentId;
        //            transactionToUpdate.PayerBalanceBeforeTransaction = payerBalanceBeforeTransaction;
        //            transactionToUpdate.SourceUserName = sourceUserName;
        //            transactionToUpdate.PayerBalanceAfterTransaction = payerBalanceAfterTransaction;

        //            context.SaveChanges();

        //            return transactionToUpdate;
        //        }

        //        MasterTransactionRecord settlementTransaction = new MasterTransactionRecord
        //        {
        //            AccessChannelId = accessChannelId,
        //            Amount = Math.Abs(amount),
        //            CustomerTypeId = customerTypeId,
        //            Fee = 0,
        //            PayerId = payerId,
        //            PayeeId = payeeId,
        //            PayerBalanceBeforeTransaction = payerBalanceBeforeTransaction,
        //            PayeeBalanceBeforeTransaction = payeeBalanceBeforeTransaction,
        //            PayerPaymentInstrumentId = payerPaymentInstrumentId,
        //            PayeePaymentInstrumentId = payeePaymentInstrumentId,
        //            ShortDescription = transactionType.FriendlyName,
        //            SourceUserName = sourceUserName,
        //            DestinationUserName = destinationUserName,
        //            Text = textString,
        //            TransactionDate = transactionTime,
        //            TransactionReference = transactionReference,
        //            TransactionStatusId = TransactionState.Successful,
        //            TransactionTypeId = transactionType.TransactionTypeId,
        //            PayerBalanceAfterTransaction = payerBalanceAfterTransaction,
        //            PayeeBalanceAfterTransaction = payeeBalanceAfterTransaction,
        //            ExternalApplicationId = 1
        //        };

        //        context.MasterTransactionRecords.Add(settlementTransaction);
        //        context.SaveChanges();


        //        return settlementTransaction;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}


        //public void UpdateCustomerLedger(ILoyalDataModel context, long paymentInstrumentId, long customerId, Biller biller, MasterTransactionRecord masterTransactionRecord, DateTime transactionTime, long agentCommission)
        //{
        //    PaymentInstrument paymentInstrument = context.PaymentInstruments.Find(paymentInstrumentId);
        //    PITypes paymentInstrumentType = (PITypes)paymentInstrument.PaymentInstrumentTypeId;

        //    switch (paymentInstrumentType)
        //    {
        //        case PITypes.MPESA:
        //            CustomerTransactionGL customerGlTransaction = new CustomerTransactionGL()
        //            {
        //                CustomerId = customerId,
        //                MasterTransactionRecordId = masterTransactionRecord.MasterTransactionRecordId,
        //                TransactionDate = transactionTime
        //            };
        //            context.CustomerTransactionGLs.Add(customerGlTransaction);
        //            break;
        //        //case PITypes.AgentSVA:
        //        //    AgentTransactionGL agentGlTransaction = new AgentTransactionGL()
        //        //    {
        //        //        CommssionEarned = agentCommission,
        //        //        MasterTransactionRecordId = masterTransactionRecord.MasterTransactionRecordId,
        //        //        TillCustomerId = customerId,
        //        //        TransactionDate = transactionTime
        //        //    };
        //        //    context.AgentTransactionGLs.Add(agentGlTransaction);
        //        //    break;
        //        //case PITypes.BillerSVA:
        //        //    BillerTransactionGL billerTransactionGL = new BillerTransactionGL
        //        //    {
        //        //        Amount = masterTransactionRecord.Amount,
        //        //        BillerId = biller.BillerId,
        //        //        CommssionEarned = 0,
        //        //        MasterTransactionRecordId = masterTransactionRecord.MasterTransactionRecordId,
        //        //        PaymentInstrumentId = biller.OperationPaymentInstrumentId,
        //        //        TransactionDate = transactionTime
        //        //    };
        //        //    context.BillerTransactionGLs.Add(billerTransactionGL);
        //        //    break;
        //        default:
        //            break;
        //    }

        //    context.SaveChanges();
        //}

        //public decimal CheckPaymentInstrumentUpperLimit(PaymentInstrument paymentInstrument, long amount, Biller biller = null)
        //{
        //    HelperRepository helperRepository = new HelperRepository();

        //    long walletUpperLimit = 0;

        //    //if (paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.AgentSVA)
        //    //{
        //    //    AgentOutlet agentOutlet = context.AgentOutlets.FirstOrDefault(ao => ao.TillCustomerId == paymentInstrument.CustomerId);

        //    //    walletUpperLimit = helperRepository.GetWalletUpperLimit((int)CustomerTypes.Agent, agentOutlet.OutletTypeId);
        //    //}
        //    //if (biller != null && paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.BillerSVA)
        //    //{
        //    //    //Biller biller = context.Billers.Find(billerId);
        //    //    walletUpperLimit = helperRepository.GetWalletUpperLimit((int)CustomerTypes.Biller, biller.BillerTypeId);
        //    //}

        //    if (paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.MPESA)
        //    {
        //        walletUpperLimit = helperRepository.GetWalletUpperLimit((int)CustomerTypes.Consumer, null);
        //    }

        //    decimal balance = BalanceEnquiry(paymentInstrument.CustomerId, paymentInstrument.PaymentInstrumentId, string.Empty, false);

        //    if (balance + amount > walletUpperLimit)
        //    {
        //        throw new Exception("TRN0012 - The PaymentInstrumntId  has hit its upper limit and cannot accommodate any more funds. Maximum permissible amount at this time is " + (walletUpperLimit - (balance + amount)));
        //    }

        //    return balance;
        //}

    }
}