﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diagnostics.DataProviders;
using Diagnostics.ModelsAndUtils.Attributes;
using Diagnostics.ModelsAndUtils.Models;
using Diagnostics.RuntimeHost.Models;
using Diagnostics.RuntimeHost.Services;
using Diagnostics.RuntimeHost.Utilities;

namespace Diagnostics.RuntimeHost.Controllers
{
    public abstract class SiteControllerBase : DiagnosticControllerBase<App>
    {
        protected ISiteService _siteService;

        public SiteControllerBase(IServiceProvider services)
            : base(services)
        {
            this._siteService = (ISiteService)services.GetService(typeof(ISiteService));
        }

        protected async Task<App> GetAppResource(string subscriptionId, string resourceGroup, string appName, DiagnosticSiteData postBody, DateTime startTime, DateTime endTime)
        {
            var defaultHostName = postBody.DefaultHostName;
            var hostnames = postBody.HostNames != null ? postBody.HostNames.Select(p => p.Name) : new List<string>();
            var webSpace = postBody.WebSpace;
            var scmSiteHostname = postBody.ScmSiteHostname;
            var stamp = await GetHostingEnvironment(postBody.Stamp.Subscription, postBody.Stamp.ResourceGroup, postBody.Stamp != null ? postBody.Stamp.Name : string.Empty, postBody.Stamp, startTime, endTime);
            var appType = GetApplicationType(postBody.Kind);
            var platformType = (!string.IsNullOrWhiteSpace(postBody.Kind) && postBody.Kind.ToLower().Contains("linux")) ? PlatformType.Linux : PlatformType.Windows;
            var stackType = await this._siteService.GetApplicationStack(subscriptionId, resourceGroup, appName, (DataProviderContext)HttpContext.Items[HostConstants.DataProviderContextKey]);

            App app = new App(subscriptionId, resourceGroup, appName)
            {
                DefaultHostName = defaultHostName,
                Hostnames = hostnames,
                WebSpace = webSpace,
                ScmSiteHostname = scmSiteHostname,
                Stamp = stamp,
                AppType = appType,
                PlatformType = platformType,
                StackType = stackType
            };

            switch (app.Stamp.HostingEnvironmentType)
            {
                case HostingEnvironmentType.V1:
                    app.StampType = StampType.ASEV1;
                    break;
                case HostingEnvironmentType.V2:
                    app.StampType = StampType.ASEV2;
                    break;
                default:
                    app.StampType = StampType.Public;
                    break;
            }

            return app;
        }

        private AppType GetApplicationType(string kind)
        {
            if (string.IsNullOrWhiteSpace(kind)) return AppType.WebApp;

            string kindProperty = kind.ToLower();

            if (kindProperty.Contains("api")) return AppType.ApiApp;
            else if (kindProperty.Contains("function")) return AppType.FunctionApp;
            else if (kindProperty.Contains("mobile")) return AppType.MobileApp;
            else if (kindProperty.Contains("gateway")) return AppType.GatewayApp;
            else return AppType.WebApp;
        }
    }
}
