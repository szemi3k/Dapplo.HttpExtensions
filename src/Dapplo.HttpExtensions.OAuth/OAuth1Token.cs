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

namespace Dapplo.HttpExtensions.OAuth
{
    /// <summary>
    ///     A default implementation for the IOAuthToken, nothing fancy
    ///     For more information, see the IOAuthToken interface
    /// </summary>
    public class OAuth1Token : IOAuth1Token
    {
        /// <inheritdoc />
        public string OAuthTokenSecret { get; set; }

        /// <inheritdoc />
        public string OAuthToken { get; set; }

        /// <inheritdoc />
        public string OAuthTokenVerifier { get; set; }
    }
}