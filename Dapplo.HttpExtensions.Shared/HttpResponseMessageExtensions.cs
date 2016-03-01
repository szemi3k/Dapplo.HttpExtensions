﻿/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.HttpExtensions.

	Dapplo.HttpExtensions is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.HttpExtensions is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.LogFacade;
using Dapplo.HttpExtensions.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.HttpExtensions
{
	/// <summary>
	/// Extensions for the HttpResponseMessage class
	/// </summary>
	public static class HttpResponseMessageExtensions
	{
		private static readonly LogSource Log = new LogSource();

		/// <summary>
		/// Find the PropertyInfo for the matching property
		/// </summary>
		/// <param name="properties">Ienumerable with PropertyInfo</param>
		/// <param name="part">HttpParts specifying which property to find</param>
		/// <param name="propertyInfo">PropertyInfo out parameter</param>
		/// <returns>bool if found</returns>
		private static bool TryFindTarget(this IEnumerable<PropertyInfo> properties, HttpParts part, out PropertyInfo propertyInfo)
		{
			propertyInfo = properties.FirstOrDefault(t => t.GetCustomAttribute<HttpAttribute>().Part == part);
			return propertyInfo != null;
		}

		/// <summary>
		/// Extension method reading the HttpResponseMessage to a Type object
		/// Currently we support Json objects which are annotated with the DataContract/DataMember attributes
		/// We might support other object, e.g MemoryStream, Bitmap etc soon
		/// </summary>
		/// <typeparam name="TResult">The Type to read into</typeparam>
		/// <param name="httpResponseMessage">HttpResponseMessage</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>the deserialized object of type T or default(T)</returns>
		public static async Task<TResult> GetAsAsync<TResult>(this HttpResponseMessage httpResponseMessage, CancellationToken token = default(CancellationToken)) where TResult : class
		{
			Log.Verbose().WriteLine("Response status code: {0}", httpResponseMessage.StatusCode);
			var resultType = typeof(TResult);
			// Quick exit if the caller just wants the HttpResponseMessage
			if (resultType == typeof(HttpResponseMessage))
			{
				return httpResponseMessage as TResult;
			}
			// See if we have a container
			var httpAttribute = resultType.GetTypeInfo().GetCustomAttribute<HttpAttribute>();
			if (httpAttribute != null && httpAttribute.Part == HttpParts.Response)
			{
				Log.Info().WriteLine("Filling type {0}", resultType.Name);
				// special type
				var instance = Activator.CreateInstance<TResult>();
				var properties = resultType.GetProperties().Where(x => x.GetCustomAttribute<HttpAttribute>() != null).ToList();

				PropertyInfo targetPropertyInfo;
				// Headers
				if (properties.TryFindTarget(HttpParts.ResponseHeaders, out targetPropertyInfo))
				{
					targetPropertyInfo.SetValue(instance, httpResponseMessage.Headers);
				}
				// StatusCode
				if (properties.TryFindTarget(HttpParts.ResponseStatuscode, out targetPropertyInfo))
				{
					targetPropertyInfo.SetValue(instance, httpResponseMessage.StatusCode);
				}

				var responsePart = httpResponseMessage.IsSuccessStatusCode
					? HttpParts.ResponseContent
					: HttpParts.ResponseErrorContent;
				bool contentSet = false;
				// Try to find the target for the error response
				if (properties.TryFindTarget(responsePart, out targetPropertyInfo))
				{
					contentSet = true;
					// get the response
					var httpContent = httpResponseMessage.Content;

					// Convert the HttpContent to the value type 
					var convertedContent = await httpContent.GetAsAsync(targetPropertyInfo.PropertyType, token).ConfigureAwait(false);

					// Now set the value
					targetPropertyInfo.SetValue(instance, convertedContent);

					// Cleanup, but only if the value is not passed onto the container 
					if (!typeof (HttpContent).IsAssignableFrom(targetPropertyInfo.PropertyType))
					{
						httpContent?.Dispose();
					}
				}
				if (!contentSet && !httpResponseMessage.IsSuccessStatusCode)
				{
					await httpResponseMessage.HandleErrorAsync(token).ConfigureAwait(false);
				}
				return instance;
			}
			if (httpResponseMessage.IsSuccessStatusCode)
			{
				var httpContent = httpResponseMessage.Content;
				var result = await httpContent.GetAsAsync<TResult>(token).ConfigureAwait(false);
				// Make sure the httpContent is only disposed when it's not the return type
				if (!typeof(HttpContent).IsAssignableFrom(typeof(TResult)))
				{
					httpContent?.Dispose();
				}

				return result;
			}
			await httpResponseMessage.HandleErrorAsync(token).ConfigureAwait(false);
			return default(TResult);
		}

		/// <summary>
		/// Simplified error handling, this makes sure the uri and response are logged
		/// </summary>
		/// <param name="httpResponseMessage">HttpResponseMessage</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>string with the error content if HttpBehaviour.ThrowErrorOnNonSuccess = false</returns>
		public static async Task<string> HandleErrorAsync(this HttpResponseMessage httpResponseMessage, CancellationToken token = default(CancellationToken))
		{
			if (httpResponseMessage == null)
			{
				throw new ArgumentNullException(nameof(httpResponseMessage));
			}
			Exception throwException = null;
			string errorContent = null;
			Uri requestUri = httpResponseMessage.RequestMessage?.RequestUri;
			try
			{
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					try
					{
						// try reading the content, so this is not lost
						errorContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						Log.Debug().WriteLine("Error while reading the error content: {0}", ex.Message);
					}
					// Write log if an error occured.
					Log.Error().WriteLine("Http response {0} ({1}) for {2}, details from website: {3}", (int)httpResponseMessage.StatusCode, httpResponseMessage.StatusCode, requestUri, errorContent);

					httpResponseMessage.EnsureSuccessStatusCode();
				}
				else
				{
					// Write log for success
					Log.Debug().WriteLine("Http response {0} ({1}) for {2}", (int)httpResponseMessage.StatusCode, httpResponseMessage.StatusCode, requestUri);
				}
			}
			catch (Exception ex)
			{
				throwException = ex;
				throwException.Data.Add("uri", requestUri);
				if (errorContent != null)
				{
					throwException.Data.Add("response", errorContent);
				}
			}

			var httpBehaviour = HttpBehaviour.Current;
			if (httpBehaviour.ThrowOnError && throwException != null)
			{
				throw throwException;
			}
			return errorContent;
		}
	}
}