﻿using System;
using System.Web.Routing;
using System.ServiceModel.Activation;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService
{
    using SwaggerWcf;

    public class Global : System.Web.HttpApplication
    {
        private static ResourceManagementClient client;

        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new ServiceRoute("v1", new WebServiceHostFactory(), typeof(v1.ResourceManagementWebServicev1)));
            RouteTable.Routes.Add(new ServiceRoute("v2", new WebServiceHostFactory(), typeof(v2.ResourceManagementWebServicev2)));
            RouteTable.Routes.Add(new ServiceRoute("api-docs", new WebServiceHostFactory(), typeof(SwaggerWcfEndpoint)));
            var x = Global.Client;
        }

        internal static ResourceManagementClient Client
        {
            get
            {
                if (Global.client == null)
                {
                    Global.client = new ResourceManagementClient();
                    Global.client.RefreshSchema();
                }

                return Global.client;
            }
        }
    }
}