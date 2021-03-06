﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2017 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.HttpExtensions
// 
//  Dapplo.HttpExtensions is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.HttpExtensions is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region Usings

using System;
using System.Net.Http;

#endregion

namespace Dapplo.HttpExtensions.Factory
{
    /// <summary>
    ///     Creating a HttpClient is not very straightforward, that is why the logic is capsulated in the HttpClientFactory.
    /// </summary>
    public static class HttpClientFactory
    {
        /// <summary>
        ///     Create a HttpClient which is modified by the settings specified in the IHttpSettings of the HttpBehaviour.
        ///     If nothing is passed, the GlobalSettings are used
        /// </summary>
        /// <param name="uriForConfiguration">
        ///     If a Uri is supplied, this is used to configure the HttpClient. Currently the
        ///     Uri.UserInfo is used to set the basic authorization.
        /// </param>
        /// <returns>HttpClient</returns>
        public static HttpClient Create(Uri uriForConfiguration = null)
        {
            var httpBehaviour = HttpBehaviour.Current;
            var httpSettings = httpBehaviour.HttpSettings ?? HttpExtensionsGlobals.HttpSettings;

            var httpClient = new HttpClient(HttpMessageHandlerFactory.Create())
            {
                Timeout = httpSettings.RequestTimeout,
                MaxResponseContentBufferSize = httpSettings.MaxResponseContentBufferSize
            };
            if (!string.IsNullOrEmpty(httpSettings.DefaultUserAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(httpSettings.DefaultUserAgent);
            }
            // If the uri has username/password, use this to set Basic Authorization
            if (uriForConfiguration != null)
            {
                httpClient.SetBasicAuthorization(uriForConfiguration);
            }

            // Copy the expect continue value
            httpClient.DefaultRequestHeaders.ExpectContinue = httpSettings.Expect100Continue;

            // Allow the passed OnCreateHttpClient action to modify the HttpClient
            httpBehaviour.OnHttpClientCreated?.Invoke(httpClient);
            return httpClient;
        }
    }
}