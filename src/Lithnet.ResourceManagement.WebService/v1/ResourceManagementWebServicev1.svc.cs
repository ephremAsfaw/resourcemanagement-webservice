﻿using Lithnet.ResourceManagement.Client;
using Microsoft.ResourceManagement.WebServices.WSResourceManagement;
using Newtonsoft.Json;
using SwaggerWcf.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Serialization;

namespace Lithnet.ResourceManagement.WebService.v1
{
    [SwaggerWcf("/v1")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [KnownType(typeof(ResourceObject))]
    [KnownType(typeof(string))]
    public class ResourceManagementWebServicev1 : IResourceManagementWebServicev1
    {
        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public Stream GetResources()
        {
            try
            {
                string attributes = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["attributes"];
                string objectType = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["objectType"];
                string filter = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["filter"];
                CultureInfo locale = GetLocaleFromParameters();

                if (filter == null)
                {
                    if (objectType == null)
                    {
                        filter = "/*";
                    }
                    else
                    {
                        filter = $"/{objectType}";
                    }
                }

                if (attributes != null)
                {
                    return ResourceManagementWebServicev1.GetResponse(Global.Client.GetResources(filter, attributes.Split(','), locale).ToList());
                }

                if (objectType != null)
                {
                    return ResourceManagementWebServicev1.GetResponse(Global.Client.GetResources(filter, ResourceManagementSchema.GetObjectType(objectType).Attributes.Select(t => t.SystemName), locale).ToList());
                }
                else
                {
                    return ResourceManagementWebServicev1.GetResponse(Global.Client.GetResources(filter, locale).ToList());
                }
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        private static CultureInfo GetLocaleFromParameters()
        {
            string locale = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["locale"];
            CultureInfo culture = null;
            if (locale != null)
            {
                culture = new CultureInfo(locale);
            }

            return culture;
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public Stream GetResourceByKey(string objectType, string key, string keyValue)
        {
            ResourceObject resource;
            try
            {
                ResourceManagementSchema.ValidateAttributeName(key);
                ResourceManagementSchema.ValidateObjectTypeName(objectType);
                CultureInfo locale = GetLocaleFromParameters();

                resource = Global.Client.GetResourceByKey(objectType, key, keyValue, locale);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceNotFoundException)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.NotFound);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }

            return ResourceManagementWebServicev1.GetResponse(resource);
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public Stream GetResourceByID(string id)
        {
            try
            {
                ResourceManagementWebServicev1.ValidateID(id);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResource(id, locale);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                return ResourceManagementWebServicev1.GetResponse(resource);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceNotFoundException)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.NotFound);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public KeyValuePair<string, string[]> GetResourceAttributeByID(string id, string attribute)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(attribute);
                ResourceManagementWebServicev1.ValidateID(id);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResource(id, new List<string>() { attribute }, locale);
                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                object value = resource.Attributes[attribute].Value;
                List<string> valuesToReturn = new List<string>();

                if (value is string)
                {
                    valuesToReturn.Add(value as string);
                }
                else if (value is byte[])
                {
                    valuesToReturn.Add(Convert.ToBase64String((byte[])value));
                }
                else
                {
                    IEnumerable values = value as IEnumerable;
                    if (values != null)
                    {
                        foreach (object enumvalue in values)
                        {
                            if (enumvalue is DateTime)
                            {
                                valuesToReturn.Add(((DateTime)enumvalue).ToResourceManagementServiceDateFormat());
                            }
                            else if (enumvalue is byte[])
                            {
                                valuesToReturn.Add(Convert.ToBase64String((byte[])enumvalue));
                            }
                            else
                            {
                                valuesToReturn.Add(enumvalue.ToString());
                            }
                        }
                    }
                    else
                    {
                        valuesToReturn.Add(value.ToString());
                    }
                }

                return new KeyValuePair<string, string[]>(attribute, valuesToReturn.ToArray());
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceNotFoundException)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.NotFound);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public KeyValuePair<string, string[]> GetResourceAttributeByKey(string objectType, string key, string keyValue, string attribute)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(attribute);
                ResourceManagementSchema.ValidateObjectTypeName(objectType);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResourceByKey(objectType, key, keyValue, new List<string>() { attribute }, locale);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                object value = resource.Attributes[attribute].Value;
                List<string> valuesToReturn = new List<string>();

                if (value is string)
                {
                    valuesToReturn.Add(value as string);
                }
                else if (value is byte[])
                {
                    valuesToReturn.Add(Convert.ToBase64String((byte[])value));
                }
                else
                {
                    IEnumerable values = value as IEnumerable;
                    if (values != null)
                    {
                        foreach (object enumvalue in values)
                        {
                            if (enumvalue is DateTime)
                            {
                                valuesToReturn.Add(((DateTime)enumvalue).ToResourceManagementServiceDateFormat());
                            }
                            else if (enumvalue is byte[])
                            {
                                valuesToReturn.Add(Convert.ToBase64String((byte[])enumvalue));
                            }
                            else
                            {
                                valuesToReturn.Add(enumvalue.ToString());
                            }
                        }
                    }
                    else
                    {
                        valuesToReturn.Add(value.ToString());
                    }
                }

                return new KeyValuePair<string, string[]>(attribute, valuesToReturn.ToArray());
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceNotFoundException)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.NotFound);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public void DeleteResourceByID(string id)
        {
            try
            {
                ResourceManagementWebServicev1.ValidateID(id);

                Global.Client.DeleteResource(id);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceNotFoundException)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.NotFound);
            }
            catch (AuthorizationRequiredException ex)
            {
                throw WebExceptionHelper.CreateWebException(ex);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public string CreateResource(ResourceUpdateRequest request)
        {
            try
            {
                AttributeValueUpdate objectTypeUpdate = request.Attributes.FirstOrDefault(t => t.Name == AttributeNames.ObjectType);

                if (objectTypeUpdate == null)
                {
                    throw new ArgumentException("An object type must be specified");
                }

                string objectType = objectTypeUpdate.Value?[0] as string;

                if (objectType == null)
                {
                    throw new ArgumentException("An object type must be specified");
                }

                ResourceObject resource = Global.Client.CreateResource(objectType);

                foreach (AttributeValueUpdate kvp in request.Attributes)
                {
                    if (kvp.Value.Length > 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value);
                    }
                    else if (kvp.Value.Length == 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value[0]);
                    }
                    else
                    {
                        resource.Attributes[kvp.Name].RemoveValues();
                    }
                }

                Global.Client.SaveResource(resource);
                return resource.ObjectID.ToString(false);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (AuthorizationRequiredException ex)
            {
                throw WebExceptionHelper.CreateWebException(ex);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public void UpdateResource(string id, ResourceUpdateRequest request)
        {
            try
            {
                ResourceManagementWebServicev1.ValidateID(id);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResource(id, locale);
                foreach (AttributeValueUpdate kvp in request.Attributes)
                {
                    if (kvp.Value.Length > 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value);
                    }
                    else if (kvp.Value.Length == 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value[0]);
                    }
                    else
                    {
                        resource.Attributes[kvp.Name].RemoveValues();
                    }
                }

                Global.Client.SaveResource(resource, locale);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (AuthorizationRequiredException ex)
            {
                throw WebExceptionHelper.CreateWebException(ex);
            }
            catch (ResourceNotFoundException)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.NotFound);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public Stream GetApprovalRequests()
        {
            return this.GetApprovalRequestsByStatus("Unknown");
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public Stream GetApprovalRequestsByStatus(string status)
        {
            try
            {
                Client.ApprovalStatus approvalStatus;

                if (Enum.TryParse(status, true, out approvalStatus))
                {
                    return ResourceManagementWebServicev1.GetResponse(Global.Client.GetApprovals(approvalStatus).ToList());
                }

                throw new ArgumentException("Invalid value for status parameter");
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public void SetPendingApproval(string id, string decision, ApprovalReason reason)
        {
            try
            {
                ResourceManagementWebServicev1.ValidateID(id);

                ResourceObject approval = Global.Client.GetResourceByKey(ObjectTypeNames.Approval, AttributeNames.ObjectID, id);

                if (string.Equals(decision, "approve", StringComparison.OrdinalIgnoreCase))
                {
                    Global.Client.Approve(approval, true, reason?.Reason);
                }
                else if (string.Equals(decision, "reject", StringComparison.OrdinalIgnoreCase))
                {
                    Global.Client.Approve(approval, false, reason?.Reason);
                }
                else 
                {
                    throw new ArgumentException($"The value '{decision}' is not supported. Allowed values are 'Approve' or 'Reject'");
                }
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (AuthorizationRequiredException ex)
            {
                throw WebExceptionHelper.CreateWebException(ex);
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public Stream GetRequestParameters(string id)
        {
            try
            {
                ResourceManagementWebServicev1.ValidateID(id);

                ResourceObject request = Global.Client.GetResourceByKey("Request", AttributeNames.ObjectID, id, new[] { "RequestParameter" });

                if (request == null)
                {
                    throw new WebFaultException(HttpStatusCode.NotFound);
                }

                if (!request.Attributes.ContainsAttribute("RequestParameter") || request.Attributes["RequestParameter"].IsNull)
                {
                    return new MemoryStream();
                }

                IList<string> parameters = request.Attributes["RequestParameter"].StringValues;
                List<RequestParameter> requestParameters = new List<RequestParameter>();

                foreach (string param in parameters)
                {
                    requestParameters.Add(XmlDeserializeFromString<RequestParameter>(param));
                }

                return ResourceManagementWebServicev1.GetResponse(requestParameters);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (ResourceManagementException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (ArgumentException ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        internal static T XmlDeserializeFromString<T>(string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        internal static object XmlDeserializeFromString(string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }

        private static void ValidateID(string id)
        {
            Guid result;

            if (!Guid.TryParse(id, out result))
            {
                throw new ArgumentException("The specified value was not a GUID type", nameof(id));
            }
        }

        private static Stream GetResponse(object r)
        {
            ResourceSerializationSettings settings = new ResourceSerializationSettings
            {
                ArrayHandling = ArraySerializationHandling.WhenRequired,
                IncludeNullValues = false,
                IncludePermissionHints = false,
                ResourceFormat = ResourceSerializationHandling.AttributeValuePairs,
                ValueFormat = AttributeValueSerializationHandling.ConvertToString
            };

            JsonSerializerSettings d = new JsonSerializerSettings
            {
                Context = new StreamingContext(StreamingContextStates.Other, settings),
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";

            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(r, d)));
        }
    }
}