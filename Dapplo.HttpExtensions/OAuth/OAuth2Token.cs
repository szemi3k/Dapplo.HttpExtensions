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

namespace Dapplo.HttpExtensions.OAuth
{
	/// <summary>
	/// A default implementation for the IOAuth2Token, nothing fancy
	/// For more information, see the IOAuth2Token interface
	/// </summary>
	internal class OAuth2Token : IOAuth2Token
	{
		public string AccessToken { get; set; }

		public DateTimeOffset AccessTokenExpires { get; set; }

		public string RefreshToken { get; set; }
	}
}
