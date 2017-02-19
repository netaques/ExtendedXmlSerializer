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
using System.Reflection;
using ExtendedXmlSerialization.Core.Sources;
using ExtendedXmlSerialization.Core.Specifications;

namespace ExtendedXmlSerialization.ContentModel.Xml
{
	class TypeOption : OptionBase<IXmlReader, TypeInfo>, ITypeOption
	{
		readonly static ContentModel.Identities Identities = ContentModel.Identities.Default;

		public static TypeOption Default { get; } = new TypeOption();
		TypeOption() : this(AlwaysSpecification<IXmlReader>.Default, Types.Default) {}

		readonly ITypes _types;
		readonly ContentModel.IIdentities _identities;

		public TypeOption(ISpecification<IXmlReader> specification, ITypes types) : this(specification, types, Identities) {}

		public TypeOption(ISpecification<IXmlReader> specification, ITypes types, ContentModel.IIdentities identities)
			: base(specification)
		{
			_types = types;
			_identities = identities;
		}

		public override TypeInfo Get(IXmlReader parameter)
		{
			var identity = _identities.Get(parameter.Name, parameter.Identifier);
			var result = _types.Get(identity);
			if (result == null)
			{
				var name = IdentityFormatter.Default.Get(identity);
				throw new InvalidOperationException(
					$"An attempt was made to load a type with the fully qualified name of '{name}', but no type could be located with that name.");
			}
			return result;
		}
	}
}