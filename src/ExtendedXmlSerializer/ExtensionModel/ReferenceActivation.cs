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

using System.Reflection;
using ExtendedXmlSerialization.ContentModel;
using ExtendedXmlSerialization.ContentModel.Properties;
using ExtendedXmlSerialization.ContentModel.Xml;

namespace ExtendedXmlSerialization.ExtensionModel
{
	class ReferenceActivation : IActivation
	{
		readonly IActivation _activation;
		readonly IEntities _entities;
		readonly IReferenceMaps _maps;

		public ReferenceActivation(IActivation activation, IEntities entities)
			: this(activation, entities, ReferenceMaps.Default) {}

		public ReferenceActivation(IActivation activation, IEntities entities, IReferenceMaps maps)
		{
			_activation = activation;
			_entities = entities;
			_maps = maps;
		}

		public IReader Get(TypeInfo parameter) => new Activator(_activation.Get(parameter), _entities, _maps);

		class Activator : IReader
		{
			readonly IReader _activator;
			readonly IEntities _entities;
			readonly IReferenceMaps _maps;

			public Activator(IReader activator, IEntities entities, IReferenceMaps maps)
			{
				_activator = activator;
				_entities = entities;
				_maps = maps;
			}

			static ReferenceIdentity? Identity(IXmlReader reader) =>
				reader.Contains(IdentityProperty.Default)
					? (ReferenceIdentity?) new ReferenceIdentity(Defaults.Reference, IdentityProperty.Default.Get(reader))
					: null;

			ReferenceIdentity? Entity(IXmlReader reader, object instance)
			{
				var typeInfo = instance.GetType().GetTypeInfo();
				var entity = _entities.Get(typeInfo)?.Get(reader);
				var result = entity != null
					? (ReferenceIdentity?) new ReferenceIdentity(typeInfo, entity)
					: null;
				return result;
			}


			public object Get(IXmlReader parameter)
			{
				var declared = Identity(parameter);
				var result = _activator.Get(parameter);

				var identity = declared ?? Entity(parameter, result);

				if (identity != null)
				{
					_maps.Get(parameter).Add(identity.Value, result);
				}
				return result;
			}
		}
	}
}