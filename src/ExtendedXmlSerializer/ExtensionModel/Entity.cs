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

using ExtendedXmlSerialization.ContentModel.Converters;
using ExtendedXmlSerialization.ContentModel.Members;
using ExtendedXmlSerialization.ContentModel.Properties;
using ExtendedXmlSerialization.ContentModel.Xml;

namespace ExtendedXmlSerialization.ExtensionModel
{
	class Entity : IEntity
	{
		readonly static EntityProperty EntityProperty = EntityProperty.Default;

		readonly IConverter _converter;
		readonly IMember _member;

		public Entity(IConverter converter, IMember member)
		{
			_converter = converter;
			_member = member;
		}

		public string Get(object parameter) => _converter.Format(_member.Adapter.Get(parameter));

		public object Get(IXmlReader parameter)
		{
			var contains = parameter.Contains(_member.Adapter);
			if (contains)
			{
				var result = _member.Get(parameter);
				parameter.Reset();
				return result;
			}
			return null;
		}

		public object Reference(IXmlReader parameter)
			=> parameter.Contains(EntityProperty) ? _converter.Parse(EntityProperty.Get(parameter)) : null;
	}
}