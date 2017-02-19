// MIT License
// 
// Copyright (c) 2016 Wojciech Nag�rski
//                    Michael DeMond
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtendedXmlSerialization.ContentModel.Xml.Namespacing;
using ExtendedXmlSerialization.Core;
using ExtendedXmlSerialization.Core.Sources;
using ExtendedXmlSerialization.Core.Specifications;
using ExtendedXmlSerialization.TypeModel;

namespace ExtendedXmlSerialization.ContentModel.Xml
{
	class IdentityPartitionedTypes : ITypes
	{
		public IdentityPartitionedTypes(ISpecification<TypeInfo> specification) : this(specification, TypeFormatter.Default) {}

		public IdentityPartitionedTypes(ISpecification<TypeInfo> specification, ITypeFormatter formatter)
			: this(WellKnownNamespaces.Default
			                          .ToDictionary(x => x.Value.Identifier, new TypeNamePartition(specification, formatter).Get)
			                          .Get) {}

		readonly Partition _partition;

		public IdentityPartitionedTypes(Partition partition)
		{
			_partition = partition;
		}

		public TypeInfo Get(IIdentity parameter) => _partition.Invoke(parameter.Identifier)?.Invoke(parameter.Name);

		class TypeNamePartition : IParameterizedSource<KeyValuePair<Assembly, Namespace>, Func<string, TypeInfo>>
		{
			readonly IApplicationTypes _types;
			readonly Func<TypeInfo, bool> _specification;
			readonly Func<TypeInfo, string> _formatter;

			public TypeNamePartition(ISpecification<TypeInfo> specification, ITypeFormatter formatter)
				: this(ApplicationTypes.Default, specification.IsSatisfiedBy, formatter.Get) {}

			public TypeNamePartition(IApplicationTypes types, Func<TypeInfo, bool> specification,
			                         Func<TypeInfo, string> formatter)
			{
				_types = types;
				_specification = specification;
				_formatter = formatter;
			}

			public Func<string, TypeInfo> Get(KeyValuePair<Assembly, Namespace> parameter)
				=> _types.Get(parameter.Key)
				         .Where(_specification)
				         .ToLookup(_formatter)
				         .ToDictionary(y => y.Key, y => y.First())
				         .Get;
		}
	}
}