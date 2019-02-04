using System;
using System.Collections.Generic;

namespace Umbraco.Core.Composing.MSDI.Named
{
    /*
    MIT License

    Copyright (c) 2019 James Jackson-South

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    */

    /// <summary>
    /// A named dependency. Used for targeted injection.
    /// </summary>
    public readonly struct NamedDependency : IEquatable<NamedDependency>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedDependency"/> struct.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The registered service name.</param>
        /// <param name="parameterName">The parameter name to match when the service is injected as a dependency.</param>
        public NamedDependency(Type serviceType, string serviceName, string parameterName)
        {
            this.ServiceType = serviceType;
            this.ServiceName = serviceName;
            this.ParameterName = parameterName;
        }

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the registered service name.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the parameter name to match when the service is injected as a dependency.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Returns a boolean indicating whether the given two named dependencies are equal.
        /// </summary>
        /// <param name="value1">The first dependency to compare.</param>
        /// <param name="value2">The second dependency to compare.</param>
        /// <returns>True if the given named dependencies are equal; False otherwise.</returns>
        public static bool operator ==(in NamedDependency value1, in NamedDependency value2) => value1.Equals(value2);

        /// <summary>
        /// Returns a boolean indicating whether the given two named dependencies are not equal.
        /// </summary>
        /// <param name="value1">The first dependency to compare.</param>
        /// <param name="value2">The second dependency to compare.</param>
        /// <returns>True if the given named dependencies are not equal; False otherwise.</returns>
        public static bool operator !=(in NamedDependency value1, in NamedDependency value2) => !(value1 == value2);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is NamedDependency namedDependency && this.Equals(namedDependency);

        /// <inheritdoc/>
        public bool Equals(NamedDependency other) => EqualityComparer<Type>.Default.Equals(this.ServiceType, other.ServiceType) && this.ServiceName == other.ServiceName && this.ParameterName == other.ParameterName;

        /// <inheritdoc/>
        public override int GetHashCode() => String.Join("", this.ServiceType, this.ServiceName, this.ParameterName).GetHashCode();
    }
}
