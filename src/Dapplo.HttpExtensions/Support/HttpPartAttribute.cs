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

#endregion

namespace Dapplo.HttpExtensions.Support
{
    /// <summary>
    ///     This attribute marks a property in a HttpRequestAttributed or HttpResponseAttribute class as being a part for
    ///     processing
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HttpPartAttribute : Attribute
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="part">HttpParts</param>
        public HttpPartAttribute(HttpParts part)
        {
            Part = part;
        }

        /// <summary>
        ///     Order of the content when using multi-part content
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///     Use this to specify what the property is representing
        /// </summary>
        public HttpParts Part { get; set; }
    }
}