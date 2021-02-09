// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.Internal
{
    /// <summary>
    /// Marks the constructor to be used when activating type using <see cref="ActivatorUtilities"/>.
    /// </summary>
    public class ActivatorUtilitiesConstructorAttribute : Attribute
    {
    }
}