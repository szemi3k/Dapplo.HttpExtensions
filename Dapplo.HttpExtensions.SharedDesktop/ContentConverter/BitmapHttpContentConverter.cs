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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.HttpExtensions.Support;
using Dapplo.LogFacade;

namespace Dapplo.HttpExtensions.ContentConverter
{
	/// <summary>
	/// This can convert HttpContent from/to a GDI Bitmap
	/// </summary>
	public class BitmapHttpContentConverter : IHttpContentConverter
	{
		private static readonly LogSource Log = new LogSource();
		private static readonly IList<string> SupportedContentTypes = new List<string>();
		public static readonly BitmapHttpContentConverter Instance = new BitmapHttpContentConverter();

		static BitmapHttpContentConverter()
		{
			SupportedContentTypes.Add(MediaTypes.Bmp.EnumValueOf());
			SupportedContentTypes.Add(MediaTypes.Gif.EnumValueOf());
			SupportedContentTypes.Add(MediaTypes.Jpeg.EnumValueOf());
			SupportedContentTypes.Add(MediaTypes.Png.EnumValueOf());
			SupportedContentTypes.Add(MediaTypes.Tiff.EnumValueOf());
		}

		private int _quality;

		public int Order => 0;

		public ImageFormat Format
		{
			get;
			set;
		} = ImageFormat.Png;

		/// <summary>
		/// Check the parameters for the encoder, like setting Jpg quality
		/// </summary>
		public IList<EncoderParameter> EncoderParameters { get; } = new List<EncoderParameter>();

		/// <summary>
		/// Set the quality EncoderParameter, for the Jpg format 0-100
		/// </summary>
		public int Quality
		{
			get
			{
				return _quality;
			}
			set
			{
				_quality = value;
				Log.Verbose().WriteLine("Setting Quality to {0}", value);
				var qualityParameter = EncoderParameters.FirstOrDefault(x => x.Encoder.Guid == Encoder.Quality.Guid);
				if (qualityParameter != null)
				{
					EncoderParameters.Remove(qualityParameter);
				}
				EncoderParameters.Add(new EncoderParameter(Encoder.Quality, value));
			}
		}

		public BitmapHttpContentConverter()
		{
			// Default quality
			Quality = 80;
		}

		/// <summary>
		/// This checks if the HttpContent can be converted to a Bitmap and is assignable to the specified Type 
		/// </summary>
		/// <param name="typeToConvertTo">This should be something we can assign Bitmap to</param>
		/// <param name="httpContent">HttpContent to process</param>
		/// <returns>true if it can convert</returns>
		public bool CanConvertFromHttpContent(Type typeToConvertTo, HttpContent httpContent)
		{
			if (typeToConvertTo == typeof(object) || !typeToConvertTo.IsAssignableFrom(typeof (Bitmap)))
			{
				return false;
			}
			var httpBehaviour = HttpBehaviour.Current;
			return !httpBehaviour.ValidateResponseContentType || SupportedContentTypes.Contains(httpContent.GetContentType());
		}

		public async Task<object> ConvertFromHttpContentAsync(Type resultType, HttpContent httpContent, CancellationToken token = default(CancellationToken))
		{
			if (!CanConvertFromHttpContent(resultType, httpContent))
			{
				var exMessage = "CanConvertFromHttpContent resulted in false, ConvertFromHttpContentAsync is not supposed to be called.";
				Log.Error().WriteLine(exMessage);
				throw new NotSupportedException(exMessage);
			}
			var memoryStream = (MemoryStream)await StreamHttpContentConverter.Instance.ConvertFromHttpContentAsync(typeof(MemoryStream),httpContent, token).ConfigureAwait(false);
			Log.Debug().WriteLine("Creating a Bitmap from the MemoryStream.");
			return new Bitmap(memoryStream);
		}

		public bool CanConvertToHttpContent(Type typeToConvert, object content)
		{
			return typeof(Bitmap).IsAssignableFrom(typeToConvert) && content != null;
		}

		public HttpContent ConvertToHttpContent(Type typeToConvert, object content)
		{
			if (!CanConvertToHttpContent(typeToConvert, content)) return null;

			var bitmap = content as Bitmap;
			if (bitmap == null) return null;

			var memoryStream = new MemoryStream();
			var encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == Format.Guid);
			if (encoder != null)
			{
				var parameters = new EncoderParameters(EncoderParameters.Count);
				int index = 0;
				EncoderParameters.ForEach(parameter => parameters.Param[index++] = parameter);
				bitmap.Save(memoryStream, encoder, parameters);
			}
			else
			{
				var exMessage = $"Can't find an encoder for {Format}";
				Log.Error().WriteLine(exMessage);
				throw new NotSupportedException(exMessage);
			}
			memoryStream.Seek(0, SeekOrigin.Begin);
			HttpContent httpContent;
			var httpBehaviour = HttpBehaviour.Current;
			if (httpBehaviour.UseProgressStreamContent)
			{
				httpContent = new ProgressStreamContent(memoryStream, httpBehaviour.UploadProgress);
			}
			else
			{
				httpContent = new StreamContent(memoryStream);
			}
			httpContent.Headers.Add("Content-Type", "image/" + Format.ToString().ToLowerInvariant());
			return httpContent;
		}

		public void AddAcceptHeadersForType(Type resultType, HttpRequestMessage httpRequestMessage)
		{
			if (resultType == null)
			{
				throw new ArgumentNullException(nameof(resultType));
			}
			if (httpRequestMessage == null)
			{
				throw new ArgumentNullException(nameof(httpRequestMessage));
			}
			if (resultType == typeof(object) || !resultType.IsAssignableFrom(typeof (Bitmap)))
			{
				return;
			}
			httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Png.EnumValueOf()));
			httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Jpeg.EnumValueOf(), Quality/100d));
			httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Tiff.EnumValueOf()));
			httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Bmp.EnumValueOf()));
			httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Gif.EnumValueOf()));
			httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Icon.EnumValueOf()));
			Log.Debug().WriteLine("Modified the header(s) of the HttpRequestMessage: Accept: {0}", httpRequestMessage.Headers.Accept);
		}
	}
}