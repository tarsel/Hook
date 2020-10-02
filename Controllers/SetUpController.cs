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
    public class SetUpController : ApiController
    {
        private SetUpRepository setUpRepository = new SetUpRepository();

        /// <summary>
        /// Create Customer Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreateCustomerType(CustomerTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.CreateCustomerType(request.CustomerTypeName, request.CustomerTypeDescription, request.CreatedBy);

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
        /// Create User Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreateUserType(UserTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.CreateUserType(request.UserTypeName, request.UserTypeDescription, request.CreatedBy);

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
        /// Create Id Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreateIdType(IdTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.CreateIDType(request.IdTypeName, request.IdTypeDescription, request.CreatedBy);

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
        /// Create Sub County
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreateSubCounty(SubCountyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.CreateSubCounty(request.SubCountyName, request.SubCountyDescription, request.CreatedBy);

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
        /// Create Town
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CreateTown(TownRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.CreateTown(request.TownName, request.TownDescription, request.SubCountyId, request.CreatedBy);

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
        /// Get All Customer Types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<CustomerTypeResponse>))]
        public IHttpActionResult GetAllCustomerTypes()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<CustomerTypeResponse> response = new List<CustomerTypeResponse>();

                List<CustomerType> cust = setUpRepository.GetAllCustomerTypes();

                foreach (var item in cust)
                {
                    response.Add(new CustomerTypeResponse { CreatedBy = item.CreatedBy, CustomerTypeDescription = item.CustomerTypeDescription, CustomerTypeId = item.CustomerTypeId, CustomerTypeName = item.CustomerTypeName, DateCreated = item.DateCreated, DateUpdated = item.DateUpdated, IsSuccessful = true, UpdatedBy = item.UpdatedBy, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new CustomerTypeResponse { StatusCode = (int)HttpStatusCode.InternalServerError, IsSuccessful = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get All User Types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<UserTypeResponse>))]
        public IHttpActionResult GetAllUserTypes()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<UserTypeResponse> response = new List<UserTypeResponse>();

                List<UserType> cust = setUpRepository.GetAllUserTypes();

                foreach (var item in cust)
                {
                    response.Add(new UserTypeResponse { CreatedBy = item.CreatedBy, DateCreated = item.DateCreated, DateUpdated = item.DateUpdated, IsSuccessful = true, UpdatedBy = item.UpdatedBy, UserTypeDescription = item.UserTypeDescription, UserTypeName = item.UserTypeName, UserTypeId = item.UserTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {

                return Ok(new UserTypeResponse
                {
                    IsSuccessful = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// GetAll Id Types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<IdTypeResponse>))]
        public IHttpActionResult GetAllIdTypes()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<IdTypeResponse> response = new List<IdTypeResponse>();

                List<IDType> cust = setUpRepository.GetAllIDTypes();

                foreach (var item in cust)
                {
                    response.Add(new IdTypeResponse { CreatedBy = item.CreatedBy, DateCreated = item.DateCreated, DateUpdated = item.DateUpdated, IdTypeDescription = item.IdTypeDescription, IdTypeId = item.IdTypeId, IdTypeName = item.IdTypeName, IsSuccessful = true, UpdatedBy = item.UpdatedBy, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new IdTypeResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    IsSuccessful = false
                });
            }
        }

        /// <summary>
        /// Get All SubCounties
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<SubCountyResponse>))]
        public IHttpActionResult GetAllSubCounties()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<SubCountyResponse> response = new List<SubCountyResponse>();

                List<SubCounty> cust = setUpRepository.GetAllSubCounties();

                foreach (var item in cust)
                {
                    response.Add(new SubCountyResponse { IsSuccessful = true, CreatedBy = item.CreatedBy, DateCreated = item.DateCreated, DateUpdated = item.DateUpdated, SubCountyDescription = item.SubCountyDescription, SubCountyId = item.SubCountyId, SubCountyName = item.SubCountyName, UpdatedBy = item.UpdatedBy, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new SubCountyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    IsSuccessful = false
                });
            }
        }

        /// <summary>
        /// Get All Towns
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<TownResponse>))]
        public IHttpActionResult GetAllTowns()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<TownResponse> response = new List<TownResponse>();

                List<Town> cust = setUpRepository.GetAllTowns();

                foreach (var item in cust)
                {
                    response.Add(new TownResponse { TownId = item.TownId, UpdatedBy = item.UpdatedBy, SubCountyId = item.SubCountyId, DateUpdated = item.DateUpdated, CreatedBy = item.CreatedBy, DateCreated = item.DateCreated, IsSuccessful = true, TownDescription = item.TownDescription, TownName = item.TownName, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new TownResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    IsSuccessful = false
                });
            }
        }

        /// <summary>
        /// Update Customer Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult UpdateCustomerType(CustomerTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.UpdateCustomerType(request.CustomerTypeId, request.CustomerTypeName, request.CustomerTypeDescription, request.UpdatedBy);

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
        /// Update User Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult UpdateUserType(UserTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.UpdateUserType(request.UserTypeId, request.UserTypeName, request.UserTypeDescription, request.UpdatedBy);

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
        /// Update Id Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult UpdateIdType(IdTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.UpdateIDType(request.IdTypeId, request.IdTypeName, request.IdTypeDescription, request.UpdatedBy);

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
        /// Update Sub County
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult UpdateSubCounty(SubCountyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.UpdateSubCounty(request.SubCountyId, request.SubCountyName, request.SubCountyDescription, request.UpdatedBy);

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
        /// Update Town
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult UpdateTown(TownRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int result = setUpRepository.UpdateTown(request.TownId, request.TownName, request.TownDescription, request.SubCountyId, request.UpdatedBy);

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
        /// Get Towns By Sub County Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<TownResponse>))]
        public IHttpActionResult GetTownsBySubCountyId(GenericRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<TownResponse> response = new List<TownResponse>();

                List<Town> cust = setUpRepository.GetTownsBySubCountyId(request.SubCountyId);

                foreach (var item in cust)
                {
                    response.Add(new TownResponse { TownId = item.TownId, UpdatedBy = item.UpdatedBy, SubCountyId = item.SubCountyId, DateUpdated = item.DateUpdated, CreatedBy = item.CreatedBy, DateCreated = item.DateCreated, IsSuccessful = true, TownDescription = item.TownDescription, TownName = item.TownName, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new TownResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    IsSuccessful = false
                });
            }
        }

        /// <summary>
        /// Get Town By Town Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(TownResponse))]
        public IHttpActionResult GetTownByTownId(GenericRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Town cust = setUpRepository.GetTownByTownId(request.TownId);

                TownResponse response = new TownResponse { TownId = cust.TownId, UpdatedBy = cust.UpdatedBy, SubCountyId = cust.SubCountyId, DateUpdated = cust.DateUpdated, CreatedBy = cust.CreatedBy, DateCreated = cust.DateCreated, IsSuccessful = true, TownDescription = cust.TownDescription, TownName = cust.TownName, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new TownResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    IsSuccessful = false
                });
            }
        }

        /// <summary>
        /// Get Sub County By Sub County Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(SubCountyResponse))]
        public IHttpActionResult GetSubCountyBySubCountyId(GenericRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                SubCounty cust = setUpRepository.GetSubCountyBySubCountyId(request.SubCountyId);

                SubCountyResponse response = new SubCountyResponse { IsSuccessful = true, CreatedBy = cust.CreatedBy, DateCreated = cust.DateCreated, DateUpdated = cust.DateUpdated, SubCountyDescription = cust.SubCountyDescription, SubCountyId = cust.SubCountyId, SubCountyName = cust.SubCountyName, UpdatedBy = cust.UpdatedBy, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK };

                return Ok(response);

            }
            catch (Exception ex)
            {
                return Ok(new SubCountyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    IsSuccessful = false
                });
            }
        }

    }
}
