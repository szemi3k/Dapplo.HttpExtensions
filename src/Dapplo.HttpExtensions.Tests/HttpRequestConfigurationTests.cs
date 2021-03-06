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

using System.Drawing.Imaging;
using System.Net.Cache;
using Dapplo.HttpExtensions.ContentConverter;
using Dapplo.HttpExtensions.Extensions;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.HttpExtensions.Tests
{
    public class HttpRequestConfigurationTests
    {
        public HttpRequestConfigurationTests(ITestOutputHelper testOutputHelper)
        {
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            HttpExtensionsGlobals.HttpSettings.RequestCacheLevel = RequestCacheLevel.NoCacheNoStore;
        }

        /// <summary>
        ///     Test posting, using Bitmap
        /// </summary>
        [Fact]
        public void Test_GetSet()
        {
            var httpBehaviour = HttpBehaviour.Current;
            var testConfig = new BitmapConfiguration {Format = ImageFormat.Gif};
            Assert.Equal(ImageFormat.Gif, testConfig.Format);
            httpBehaviour.SetConfig(testConfig);
            Assert.Equal(ImageFormat.Gif, testConfig.Format);
            var retrievedConfig = httpBehaviour.GetConfig<BitmapConfiguration>();
            Assert.Equal(ImageFormat.Gif, retrievedConfig.Format);
        }
    }
}