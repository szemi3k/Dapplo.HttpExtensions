﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
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
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapplo.HttpExtensions.JsonSimple;
using Dapplo.HttpExtensions.Tests.TestEntities;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.HttpExtensions.Tests
{
    /// <summary>
    ///     Test posting parts
    /// </summary>
    public class HttpPartsPostTest
    {
        private static readonly LogSource Log = new LogSource();
        private static readonly Uri RequestBinUri = new Uri("http://httpbin.org");

        public HttpPartsPostTest(ITestOutputHelper testOutputHelper)
        {
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            SimpleJsonSerializer.RegisterGlobally();
        }

        /// <summary>
        ///     Test posting, using Bitmap
        /// </summary>
        [Fact]
        public async Task TestPost_Bitmap()
        {
            var testUri = RequestBinUri.AppendSegments("post");
            var uploadBehaviour = HttpBehaviour.Current.ShallowClone();

            bool hasProgress = false;

            uploadBehaviour.UseProgressStream = true;
            uploadBehaviour.UploadProgress += progress =>
            {
                Log.Info().WriteLine("Progress {0}", (int) (progress * 100));
                hasProgress = true;
            };
            uploadBehaviour.MakeCurrent();
            var testObject = new MyMultiPartRequest<Bitmap>
            {
                BitmapContentName = "MyBitmapContent",
                BitmapFileName = "MyBitmapFilename",
                OurBitmap = new Bitmap(10, 10),
                JsonInformation = new GitHubError {DocumentationUrl = "http://test.de", Message = "Hello"}
            };
            testObject.Headers.Add("Name", "Dapplo");
            var result = await testUri.PostAsync<dynamic>(testObject);
            Assert.NotNull(result);
            Assert.True(hasProgress);
        }

        /// <summary>
        ///     Test posting, this time use a BitmapSource
        /// </summary>
        [Fact]
        public async Task TestPost_BitmapSource()
        {
            var testUri = RequestBinUri.AppendSegments("post");
            var uploadBehaviour = HttpBehaviour.Current.ShallowClone();

            bool hasProgress = false;

            uploadBehaviour.UseProgressStream = true;
            uploadBehaviour.UploadProgress += progress =>
            {
                Log.Info().WriteLine("Progress {0}", (int) (progress * 100));
                hasProgress = true;
            };
            uploadBehaviour.MakeCurrent();
            var testObject = new MyMultiPartRequest<BitmapSource>
            {
                BitmapContentName = "MyBitmapContent",
                BitmapFileName = "MyBitmapFilename",
                OurBitmap = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgr24, null, new byte[] {0, 0, 0}, 3),
                JsonInformation = new GitHubError {DocumentationUrl = "http://test.de", Message = "Hello"}
            };
            testObject.Headers.Add("Name", "Dapplo");
            var result = await testUri.PostAsync<dynamic>(testObject);
            Assert.NotNull(result);
            Assert.True(hasProgress);
        }
    }
}