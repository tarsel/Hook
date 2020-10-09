using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

using Hook.Models;
using Hook.Repository;
using Hook.Request;
using Hook.Response;

namespace Hook.Controllers
{
    /// <summary>
    /// Transaction Module
    /// </summary>
    public class TransactionController : ApiController
    {
        TransactionRepository transactionRepository = new TransactionRepository();

        /// <summary>
        /// Buy Airtime
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerLoyaltyPointResponse))]
        public IHttpActionResult BuyAirtime(BuyAirtimeRequest request)
        {
            try
            {
                CustomerLoyaltyPoint dto = transactionRepository.BuyAirtime(long.Parse(request.Amount), long.Parse(request.Msisdn));

                return Ok(new CustomerLoyaltyPointResponse { CumulativeFeeAmount = dto.CumulativeFeeAmount, CumulativePoints = dto.CumulativePoints, CumulativeTransactionAmount = dto.CumulativeTransactionAmount, CustomerLoyaltyPointId = dto.CustomerLoyaltyPointId, IsFrozen = dto.IsFrozen, Message = "Processed Successfully!", OrganizationId = dto.OrganizationId, PaymentInstrumentId = dto.PaymentInstrumentId, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return Ok(new CustomerLoyaltyPointResponse { Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Sell Airtime
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerLoyaltyPointResponse))]
        public IHttpActionResult SellAirtime(BuyAirtimeRequest request)
        {
            try
            {
                CustomerLoyaltyPoint dto = transactionRepository.SellAirtime(long.Parse(request.Amount), long.Parse(request.Msisdn));

                return Ok(new CustomerLoyaltyPointResponse { CumulativeFeeAmount = dto.CumulativeFeeAmount, CumulativePoints = dto.CumulativePoints, CumulativeTransactionAmount = dto.CumulativeTransactionAmount, CustomerLoyaltyPointId = dto.CustomerLoyaltyPointId, IsFrozen = dto.IsFrozen, Message = "Processed Successfully!", OrganizationId = dto.OrganizationId, PaymentInstrumentId = dto.PaymentInstrumentId, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return Ok(new CustomerLoyaltyPointResponse { Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }

        }

        /// <summary>
        /// Redeem Points
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(MasterTransactionRecordResponse))]
        public IHttpActionResult RedeemPoints(RedeemPointsRequest request)
        {
            try
            {
                MasterTransactionRecord dto = transactionRepository.RedeemPoints(request.OrganizationId, request.CustomerId, request.PaymentInstrumentId, request.PointsToRedeem);

                return Ok(new MasterTransactionRecordResponse { AccessChannelId = dto.AccessChannelId, Amount = dto.Amount, CustomerTypeId = dto.CustomerTypeId, DestinationUserName = dto.DestinationUserName, Fee = dto.Fee, IsTestTransaction = dto.IsTestTransaction, MasterTransactionRecordId = dto.MasterTransactionRecordId, PayeeBalanceAfterTransaction = dto.PayeeBalanceAfterTransaction, PayeeBalanceBeforeTransaction = dto.PayeeBalanceBeforeTransaction, PayeeId = dto.PayeeId, PayeePaymentInstrumentId = dto.PayeePaymentInstrumentId, PayerBalanceAfterTransaction = dto.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = dto.PayerBalanceBeforeTransaction, PayerId = dto.PayerId, PayerPaymentInstrumentId = dto.PayerPaymentInstrumentId, ReversedTransactionOriginalTypeId = dto.ReversedTransactionOriginalTypeId, ShortDescription = dto.ShortDescription, SourceUserName = dto.SourceUserName, Tax = dto.Tax, ThirdPartyTransactionId = dto.ThirdPartyTransactionId, TransactionDate = dto.TransactionDate, TransactionReference = dto.TransactionReference, TransactionStatusId = dto.TransactionStatusId, TransactionTypeId = dto.TransactionTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return Ok(new MasterTransactionRecordResponse { Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }

        }

        /// <summary>
        /// Check Points Balance
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerLoyaltyPointResponse))]
        public IHttpActionResult CheckPointsBalance(CheckPointsBalanceRequest request)
        {
            try
            {
                CustomerLoyaltyPoint dto = transactionRepository.CheckPointsBalance(request.PaymentInstrumentId);

                return Ok(new CustomerLoyaltyPointResponse { CumulativeFeeAmount = dto.CumulativeFeeAmount, CumulativePoints = dto.CumulativePoints / 100, CumulativeTransactionAmount = dto.CumulativeTransactionAmount, CustomerLoyaltyPointId = dto.CustomerLoyaltyPointId, IsFrozen = dto.IsFrozen, Message = "Processed Successfully!", OrganizationId = dto.OrganizationId, PaymentInstrumentId = dto.PaymentInstrumentId, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return Ok(new CustomerLoyaltyPointResponse
                {
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }

        }



        /// <summary>
        /// Create Transaction Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreateTransactionType(TransactionTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = transactionRepository.CreateTransactionType(request.TransactionTypeName, request.FriendlyName, request.Amount);

                if (result > 0)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }

        }

        /// <summary>
        /// Update Transaction Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult UpdateTransactionType(TransactionTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                TransactionType result = transactionRepository.UpdateTransactionType(request.TransactionTypeId, request.TransactionTypeName, request.FriendlyName, request.Amount);

                if (result != null)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transaction Type By Transaction Type Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(TransactionTypeResponse))]
        public IHttpActionResult GetTransactionTypeByTransactionTypeId(TransactionTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                TransactionType result = transactionRepository.GetTransactionTypeByTransactionTypeId(request.TransactionTypeId);

                if (result != null)
                {
                    return Ok(new TransactionTypeResponse { Amount = result.Amount, TransactionTypeId = result.TransactionTypeId, FriendlyName = result.FriendlyName, IsActive = result.IsActive, TransactionTypeName = result.TransactionTypeName, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new TransactionTypeResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Make Transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult MakeTransaction(MakeTransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = transactionRepository.MakeTransaction(request.CustomerId, request.TransactionTypeId, request.TransactionAmount, request.CustomerTypeId);

                if (result == 1)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get All Transactions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<MinifiedTransactionRecordResponse>))]
        public IHttpActionResult GetAllTransactions()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<MinifiedTransactionRecordResponse> response = new List<MinifiedTransactionRecordResponse>();
                List<MinifiedTransactionRecord> result = new List<MinifiedTransactionRecord>();

                result = transactionRepository.GetAllTransactions();

                if (result != null)
                {

                    foreach (var item in result)
                    {
                        response.Add(new MinifiedTransactionRecordResponse { TransactionDate = item.TransactionDate, AmountPaid = item.AmountPaid, FirstName = item.FirstName, IdNumber = item.IdNumber, LastName = item.LastName, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, SubCountyName = item.SubCountyName, TownName = item.TownName, TransactionTypeName = item.TransactionTypeName, TransactionTypeCategoryName = item.TransactionTypeCategoryName });
                    }
                }
                else
                {
                    return Ok(new MinifiedTransactionRecordResponse { });
                }

                return Ok(response);

            }
            catch (Exception)
            {
                return Ok(new MinifiedTransactionRecordResponse { });
            }
        }

        /// <summary>
        /// Get All Default Transactions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<MakeTransactionResponse>))]
        public IHttpActionResult GetAllDefaultTransactions()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<MakeTransactionResponse> response = new List<MakeTransactionResponse>();
                List<MasterTransactionRecord> result = new List<MasterTransactionRecord>();

                result = transactionRepository.GetAllDefaultTransactions();

                if (result != null)
                {

                    foreach (var item in result)
                    {
                        response.Add(new MakeTransactionResponse { AccessChannelId = item.AccessChannelId, Amount = item.Amount, CustomerTypeId = item.CustomerTypeId, DestinationUserName = item.DestinationUserName, Fee = item.Fee, IsSuccessful = true, IsTestTransaction = item.IsTestTransaction, MasterTransactionRecordId = item.MasterTransactionRecordId, Message = "Processed Successfully!", PayeeBalanceAfterTransaction = item.PayeeBalanceAfterTransaction, PayeeBalanceBeforeTransaction = item.PayeeBalanceBeforeTransaction, PayeeId = item.PayeeId, PayeePaymentInstrumentId = item.PayeePaymentInstrumentId, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, PayerId = item.PayerId, PayerPaymentInstrumentId = item.PayerPaymentInstrumentId, ReversedTransactionOriginalTypeId = item.ReversedTransactionOriginalTypeId, ShortDescription = item.ShortDescription, SourceUserName = item.SourceUserName, StatusCode = (int)HttpStatusCode.OK, Tax = item.Tax, ThirdPartyTransactionId = item.ThirdPartyTransactionId, TransactionDate = item.TransactionDate, TransactionErrorCodeId = item.TransactionErrorCodeId, TransactionReference = item.TransactionReference, TransactionStatusId = item.TransactionStatusId, TransactionTypeId = item.TransactionTypeId });
                    }
                }
                else
                {
                    return Ok(new MakeTransactionResponse { StatusCode = (int)HttpStatusCode.OK, Message = "Processed Successfully!", IsSuccessful = false });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new MakeTransactionResponse { StatusCode = (int)HttpStatusCode.InternalServerError, Message = ex.Message, IsSuccessful = false });
            }
        }

        /// <summary>
        /// Create Payment Instrument Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreatePaymentInstrumentType(PaymentInstrumentTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = transactionRepository.CreatePaymentInstrumentType(request.PaymentInstrumentTypeName);

                if (result > 0)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Update Payment Instrument Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PaymentInstrumentTypeResponse))]
        public IHttpActionResult UpdatePaymentInstrumentType(PaymentInstrumentTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                PaymentInstrumentType result = transactionRepository.UpdatePaymentInstrumentType(request.PaymentInstrumentTypeId, request.PaymentInstrumentTypeName);

                if (result != null)
                {
                    return Ok(new PaymentInstrumentTypeResponse { PaymentInstrumentTypeId = result.PaymentInstrumentTypeId, PaymentInstrumentTypeName = result.PaymentInstrumentTypeName, IsActive = result.IsActive, IsWallet = result.IsWallet, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new PaymentInstrumentTypeResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentTypeResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Payment Instrument Type By Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PaymentInstrumentTypeResponse))]
        public IHttpActionResult GetPaymentInstrumentTypeById(PaymentInstrumentTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                PaymentInstrumentType result = transactionRepository.GetPaymentInstrumentTypeById(request.PaymentInstrumentTypeId);

                if (result != null)
                {
                    return Ok(new PaymentInstrumentTypeResponse { PaymentInstrumentTypeId = result.PaymentInstrumentTypeId, PaymentInstrumentTypeName = result.PaymentInstrumentTypeName, IsActive = result.IsActive, IsWallet = result.IsWallet, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new PaymentInstrumentTypeResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentTypeResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Create Payment Instrument
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreatePaymentInstrument(PaymentInstrumentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                PaymentInstrument result = transactionRepository.CreatePaymentInstrument(request.CustomerId);

                if (result != null)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Payment Instrument By Customer Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PaymentInstrumentResponse))]
        public IHttpActionResult GetPaymentInstrumentByCustomerId(PaymentInstrumentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                PaymentInstrument result = transactionRepository.GetPaymentInstrumentByCustomerId(request.CustomerId);

                if (result != null)
                {
                    return Ok(new PaymentInstrumentResponse { CustomerId = result.CustomerId, IsActive = result.IsActive, AccountBalance = result.AccountBalance, AccountNumber = result.AccountNumber, AllowCredit = result.AllowCredit, AllowDebit = result.AllowDebit, BranchCode = result.BranchCode, BranchName = result.BranchName, CardExpiryDate = result.CardExpiryDate, CardNumber = result.CardNumber, DateLinked = result.DateLinked, DateVerified = result.DateVerified, Delinked = result.Delinked, IdNumber = result.IdNumber, IsDefaultFIAccount = result.IsDefaultFIAccount, IsMobileWallet = result.IsMobileWallet, IsSuspended = result.IsSuspended, LoyaltyPointBalance = result.LoyaltyPointBalance, PaymentInstrumentId = result.PaymentInstrumentId, PaymentInstrumentTypeId = result.PaymentInstrumentTypeId, PaymentIntrumentAlias = result.PaymentIntrumentAlias, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Update Payment Instrument
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PaymentInstrumentResponse))]
        public IHttpActionResult UpdatePaymentInstrument(PaymentInstrumentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                PaymentInstrument result = transactionRepository.GetPaymentInstrumentByCustomerId(request.CustomerId);

                if (result != null)
                {
                    return Ok(new PaymentInstrumentResponse { CustomerId = result.CustomerId, IsActive = result.IsActive, AccountBalance = result.AccountBalance, AccountNumber = result.AccountNumber, AllowCredit = result.AllowCredit, AllowDebit = result.AllowDebit, BranchCode = result.BranchCode, BranchName = result.BranchName, CardExpiryDate = result.CardExpiryDate, CardNumber = result.CardNumber, DateLinked = result.DateLinked, DateVerified = result.DateVerified, Delinked = result.Delinked, IdNumber = result.IdNumber, IsDefaultFIAccount = result.IsDefaultFIAccount, IsMobileWallet = result.IsMobileWallet, IsSuspended = result.IsSuspended, LoyaltyPointBalance = result.LoyaltyPointBalance, PaymentInstrumentId = result.PaymentInstrumentId, PaymentInstrumentTypeId = result.PaymentInstrumentTypeId, PaymentIntrumentAlias = result.PaymentIntrumentAlias, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transactions By Msisdn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<MakeTransactionResponse>))]
        public IHttpActionResult GetTransactionsByMsisdn(TransactionsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<MakeTransactionResponse> response = new List<MakeTransactionResponse>();

                List<MasterTransactionRecord> result = transactionRepository.GetTransactionsByMsisdn(request.Msisdn);

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new MakeTransactionResponse { CustomerTypeId = item.CustomerTypeId, AccessChannelId = item.AccessChannelId, Amount = item.Amount, DestinationUserName = item.DestinationUserName, Fee = item.Fee, IsTestTransaction = item.IsTestTransaction, MasterTransactionRecordId = item.MasterTransactionRecordId, PayeeBalanceAfterTransaction = item.PayeeBalanceAfterTransaction, PayeeBalanceBeforeTransaction = item.PayeeBalanceBeforeTransaction, PayeeId = item.PayeeId, PayeePaymentInstrumentId = item.PayeePaymentInstrumentId, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, PayerId = item.PayerId, PayerPaymentInstrumentId = item.PayerPaymentInstrumentId, ReversedTransactionOriginalTypeId = item.ReversedTransactionOriginalTypeId, ShortDescription = item.ShortDescription, SourceUserName = item.SourceUserName, Tax = item.Tax, ThirdPartyTransactionId = item.ThirdPartyTransactionId, TransactionDate = item.TransactionDate, TransactionErrorCodeId = item.TransactionErrorCodeId, TransactionReference = item.TransactionReference, TransactionStatusId = item.TransactionStatusId, TransactionTypeId = item.TransactionTypeId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transactions By Id Number
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<MakeTransactionResponse>))]
        public IHttpActionResult GetTransactionsByIdNumber(TransactionsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<MakeTransactionResponse> response = new List<MakeTransactionResponse>();

                List<MasterTransactionRecord> result = transactionRepository.GetTransactionsByIdNumber(request.IdNumber);

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new MakeTransactionResponse { CustomerTypeId = item.CustomerTypeId, AccessChannelId = item.AccessChannelId, Amount = item.Amount, DestinationUserName = item.DestinationUserName, Fee = item.Fee, IsTestTransaction = item.IsTestTransaction, MasterTransactionRecordId = item.MasterTransactionRecordId, PayeeBalanceAfterTransaction = item.PayeeBalanceAfterTransaction, PayeeBalanceBeforeTransaction = item.PayeeBalanceBeforeTransaction, PayeeId = item.PayeeId, PayeePaymentInstrumentId = item.PayeePaymentInstrumentId, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, PayerId = item.PayerId, PayerPaymentInstrumentId = item.PayerPaymentInstrumentId, ReversedTransactionOriginalTypeId = item.ReversedTransactionOriginalTypeId, ShortDescription = item.ShortDescription, SourceUserName = item.SourceUserName, Tax = item.Tax, ThirdPartyTransactionId = item.ThirdPartyTransactionId, TransactionDate = item.TransactionDate, TransactionErrorCodeId = item.TransactionErrorCodeId, TransactionReference = item.TransactionReference, TransactionStatusId = item.TransactionStatusId, TransactionTypeId = item.TransactionTypeId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transactions By Transaction Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<MakeTransactionResponse>))]
        public IHttpActionResult GetTransactionsByTransactionType(TransactionsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<MakeTransactionResponse> response = new List<MakeTransactionResponse>();

                List<MasterTransactionRecord> result = transactionRepository.GetTransactionsByTransactionType(request.TransactionTypeId);

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new MakeTransactionResponse { CustomerTypeId = item.CustomerTypeId, AccessChannelId = item.AccessChannelId, Amount = item.Amount, DestinationUserName = item.DestinationUserName, Fee = item.Fee, IsTestTransaction = item.IsTestTransaction, MasterTransactionRecordId = item.MasterTransactionRecordId, PayeeBalanceAfterTransaction = item.PayeeBalanceAfterTransaction, PayeeBalanceBeforeTransaction = item.PayeeBalanceBeforeTransaction, PayeeId = item.PayeeId, PayeePaymentInstrumentId = item.PayeePaymentInstrumentId, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, PayerId = item.PayerId, PayerPaymentInstrumentId = item.PayerPaymentInstrumentId, ReversedTransactionOriginalTypeId = item.ReversedTransactionOriginalTypeId, ShortDescription = item.ShortDescription, SourceUserName = item.SourceUserName, Tax = item.Tax, ThirdPartyTransactionId = item.ThirdPartyTransactionId, TransactionDate = item.TransactionDate, TransactionErrorCodeId = item.TransactionErrorCodeId, TransactionReference = item.TransactionReference, TransactionStatusId = item.TransactionStatusId, TransactionTypeId = item.TransactionTypeId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get All Transaction Types
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<TransactionTypeResponse>))]
        public IHttpActionResult GetAllTransactionTypes()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<TransactionTypeResponse> response = new List<TransactionTypeResponse>();
                List<TransactionType> result = new List<TransactionType>();

                result = transactionRepository.GetAllTransactionTypes();

                if (result != null)
                {

                    foreach (var item in result)
                    {
                        response.Add(new TransactionTypeResponse { Amount = item.Amount, TransactionTypeId = item.TransactionTypeId, FriendlyName = item.FriendlyName, IsActive = item.IsActive, TransactionTypeName = item.TransactionTypeName, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new TransactionTypeResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new TransactionTypeResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get All Payment Instrument Types
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<PaymentInstrumentTypeResponse>))]
        public IHttpActionResult GetAllPaymentInstrumentTypes()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<PaymentInstrumentTypeResponse> response = new List<PaymentInstrumentTypeResponse>();
                List<PaymentInstrumentType> result = new List<PaymentInstrumentType>();

                result = transactionRepository.GetAllPaymentInstrumentTypes();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new PaymentInstrumentTypeResponse { PaymentInstrumentTypeId = item.PaymentInstrumentTypeId, PaymentInstrumentTypeName = item.PaymentInstrumentTypeName, IsActive = item.IsActive, IsWallet = item.IsWallet, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new PaymentInstrumentTypeResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentTypeResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get All Transaction Report
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<TransactionReportResponse>))]
        public IHttpActionResult GetAllTransactionReport()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<TransactionReportResponse> response = new List<TransactionReportResponse>();
                List<TransactionReport> result = new List<TransactionReport>();

                result = transactionRepository.GetTransactionsReport();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new TransactionReportResponse { FirstName = item.FirstName, IdNumber = item.IdNumber, MasterTransactionRecordId = item.MasterTransactionRecordId, Msisdn = item.Msisdn, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, TransactionDate = item.TransactionDate, TransactionReference = item.TransactionReference, TransactionTypeName = item.TransactionTypeName, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new TransactionReportResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new TransactionReportResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Create Transaction Type Category
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreateTransactionTypeCategory(TransactionTypeCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = transactionRepository.CreateTransactionTypeCategory(request.TransactionTypeId, request.TransactionTypeCategoryName, request.Amount);

                if (result > 0)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Update Transaction Type Category
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(TransactionTypeCategoryResponse))]
        public IHttpActionResult UpdateTransactionTypeCategory(TransactionTypeCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                TransactionTypeCategory result = transactionRepository.UpdateTransactionTypeCategory(request.TransactionTypeCategoryId, request.TransactionTypeId, request.TransactionTypeCategoryName, request.Amount);

                if (result != null)
                {
                    return Ok(new TransactionTypeCategoryResponse { Amount = result.Amount, TransactionTypeCategoryName = result.TransactionTypeCategoryName, TransactionTypeId = result.TransactionTypeId, FriendlyName = result.FriendlyName, IsActive = result.IsActive, TransactionTypeCategoryId = result.TransactionTypeCategoryId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transaction Type Category By Transaction Type Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<TransactionTypeCategoryResponse>))]
        public IHttpActionResult GetTransactionTypeCategoryByTransactionTypeId(TransactionTypeCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<TransactionTypeCategoryResponse> response = new List<TransactionTypeCategoryResponse>();

                List<TransactionTypeCategory> result = transactionRepository.GetTransactionTypeCategoryByTransactionTypeId(request.TransactionTypeId);

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new TransactionTypeCategoryResponse { TransactionTypeId = item.TransactionTypeId, TransactionTypeCategoryId = item.TransactionTypeCategoryId, IsActive = item.IsActive, FriendlyName = item.FriendlyName, TransactionTypeCategoryName = item.TransactionTypeCategoryName, Amount = item.Amount, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transaction Type Category By Category Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(TransactionTypeCategoryResponse))]
        public IHttpActionResult GetTransactionTypeCategoryByCategoryId(TransactionTypeCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                TransactionTypeCategory result = transactionRepository.GetTransactionTypeCategoryByCategoryId(request.TransactionTypeCategoryId);

                if (result != null)
                {
                    return Ok(new TransactionTypeCategoryResponse { TransactionTypeCategoryId = result.TransactionTypeCategoryId, Amount = result.Amount, TransactionTypeCategoryName = result.TransactionTypeCategoryName, FriendlyName = result.FriendlyName, IsActive = result.IsActive, TransactionTypeId = result.TransactionTypeId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get All Transaction Type Category
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<TransactionTypeCategoryResponse>))]
        public IHttpActionResult GetAllTransactionTypeCategory()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<TransactionTypeCategoryResponse> response = new List<TransactionTypeCategoryResponse>();
                List<TransactionTypeCategory> result = new List<TransactionTypeCategory>();

                result = transactionRepository.GetAllTransactionTypeCategory();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new TransactionTypeCategoryResponse { TransactionTypeId = item.TransactionTypeId, IsActive = item.IsActive, FriendlyName = item.FriendlyName, TransactionTypeCategoryName = item.TransactionTypeCategoryName, Amount = item.Amount, TransactionTypeCategoryId = item.TransactionTypeCategoryId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new TransactionTypeCategoryResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transactions By Ward
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<MakeTransactionResponse>))]
        public IHttpActionResult GetTransactionsByWard(TransactionsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<MakeTransactionResponse> response = new List<MakeTransactionResponse>();

                List<MasterTransactionRecord> result = transactionRepository.GetTransactionsByWard(request.WardId);

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new MakeTransactionResponse { CustomerTypeId = item.CustomerTypeId, AccessChannelId = item.AccessChannelId, Amount = item.Amount, DestinationUserName = item.DestinationUserName, Fee = item.Fee, IsTestTransaction = item.IsTestTransaction, MasterTransactionRecordId = item.MasterTransactionRecordId, PayeeBalanceAfterTransaction = item.PayeeBalanceAfterTransaction, PayeeBalanceBeforeTransaction = item.PayeeBalanceBeforeTransaction, PayeeId = item.PayeeId, PayeePaymentInstrumentId = item.PayeePaymentInstrumentId, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, PayerId = item.PayerId, PayerPaymentInstrumentId = item.PayerPaymentInstrumentId, ReversedTransactionOriginalTypeId = item.ReversedTransactionOriginalTypeId, ShortDescription = item.ShortDescription, SourceUserName = item.SourceUserName, Tax = item.Tax, ThirdPartyTransactionId = item.ThirdPartyTransactionId, TransactionDate = item.TransactionDate, TransactionErrorCodeId = item.TransactionErrorCodeId, TransactionReference = item.TransactionReference, TransactionStatusId = item.TransactionStatusId, TransactionTypeId = item.TransactionTypeId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Get Transactions By Date
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<MakeTransactionResponse>))]
        public IHttpActionResult GetTransactionsByDate(TransactionsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                List<MakeTransactionResponse> response = new List<MakeTransactionResponse>();

                List<MasterTransactionRecord> result = transactionRepository.GetTransactionsByDate(request.StartDate, request.EndDate);

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        response.Add(new MakeTransactionResponse { CustomerTypeId = item.CustomerTypeId, AccessChannelId = item.AccessChannelId, Amount = item.Amount, DestinationUserName = item.DestinationUserName, Fee = item.Fee, IsTestTransaction = item.IsTestTransaction, MasterTransactionRecordId = item.MasterTransactionRecordId, PayeeBalanceAfterTransaction = item.PayeeBalanceAfterTransaction, PayeeBalanceBeforeTransaction = item.PayeeBalanceBeforeTransaction, PayeeId = item.PayeeId, PayeePaymentInstrumentId = item.PayeePaymentInstrumentId, PayerBalanceAfterTransaction = item.PayerBalanceAfterTransaction, PayerBalanceBeforeTransaction = item.PayerBalanceBeforeTransaction, PayerId = item.PayerId, PayerPaymentInstrumentId = item.PayerPaymentInstrumentId, ReversedTransactionOriginalTypeId = item.ReversedTransactionOriginalTypeId, ShortDescription = item.ShortDescription, SourceUserName = item.SourceUserName, Tax = item.Tax, ThirdPartyTransactionId = item.ThirdPartyTransactionId, TransactionDate = item.TransactionDate, TransactionErrorCodeId = item.TransactionErrorCodeId, TransactionReference = item.TransactionReference, TransactionStatusId = item.TransactionStatusId, TransactionTypeId = item.TransactionTypeId, IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new PaymentInstrumentResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }


    }
}

