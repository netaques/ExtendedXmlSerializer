﻿using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer.ContentModel.Members;
using ExtendedXmlSerializer.Core;
using ExtendedXmlSerializer.Core.Collections;
using ExtendedXmlSerializer.Core.Sources;
using ExtendedXmlSerializer.ExtensionModel;
using ExtendedXmlSerializer.ExtensionModel.Content;
using ExtendedXmlSerializer.ExtensionModel.Content.Members;
using ExtendedXmlSerializer.ExtensionModel.Content.Registration;
using ExtendedXmlSerializer.ExtensionModel.References;
using ExtendedXmlSerializer.ExtensionModel.Services;
using ExtendedXmlSerializer.ExtensionModel.Types;
using ExtendedXmlSerializer.ExtensionModel.Xml;
using ExtendedXmlSerializer.Tests.Support;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable All

namespace ExtendedXmlSerializer.Tests.Configuration
{
	public sealed class ContainerTests
	{
		[Fact]
		public void Verify()
		{
			new ConfigurationContainer().ToSupport().Cycle(6776);
		}

		[Fact]
		public void VerifyTypes()
		{
			var container = new ConfigurationContainer();
			container.Types()
			         .Should()
			         .BeEmpty();

			container.Type<Subject>()
			         .Should()
			         .BeSameAs(container.Type<Subject>());

			container.Types()
			         .Should()
			         .ContainSingle();
		}

		[Fact]
		public void VerifyMembers()
		{
			var container = new ConfigurationContainer();
			var type = container.Type<Subject>();

			type.Members()
			    .Should()
			    .BeEmpty();

			var member = type.Member(x => x.Message);
			member.Should()
			      .BeSameAs(type.Member(x => x.Message));

			type.Members()
			    .Should()
			    .ContainSingle();
		}

		[Fact]
		public void VerifyClass()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new Subject {Message = "Hello World!"});
		}

		[Fact]
		public void VerifyArray()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new[]{1, 2, 3});
		}

		[Fact]
		public void VerifyCollection()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new List<string>{ "Hello", "World"});
		}

		[Fact]
		public void VerifyDictionary()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new Dictionary<string, int>{ {"Hello", 1}, {"World!", 2}});
		}


		[Fact]
		public void VerifyNullable()
		{
			var support = new ConfigurationContainer().ToSupport();
			support.Cycle(new int?(6776));
			support.Cycle((int?)null);
		}

		[Fact]
		public void VerifyGroups()
		{
			var container = new GroupCollection<ISerializerExtension>(Groups.Default);

			var all = container.ToArray();
			all.Should()
			   .HaveCount(18);

			all.First()
			   .Should()
			   .BeOfType<ConfigurationServicesExtension>();
			all.Last()
			   .Should()
			   .BeSameAs(CachingExtension.Default);

			var types = container.Get(Categories.TypeSystem);
			types.Should()
			     .HaveCount(6);
			types.First()
			     .Should()
			     .BeOfType<TypeModelExtension>();
			types.Last()
			     .Should()
			     .BeOfType<MemberModelExtension>();

			new RemoveExtensionCommand(container).Execute(types.FirstOfType<MemberModelExtension>());

			types.Should()
			     .HaveCount(5);

			Action add = () => types.Add(TypeModelExtension.Default);
			add.ShouldThrow<InvalidOperationException>();
		}

		[Fact]
		public void VerifyBasicAdd()
		{
			var sut = new ExtensionCollection();
			var container = new ConfigurationContainer(sut);
			var extension = container.Extend<AddExtension>();
			extension.Should()
			         .NotBeNull();
			sut.Get(Categories.Content)
			   .Should()
			   .Contain(extension);
		}

		[Fact]
		public void VerifyAddFromMetadata()
		{
			var sut       = new ExtensionCollection();
			var container = new ConfigurationContainer(sut);

			sut.FirstOfType<AddMetadataExtension>().Should().BeNull();

			var extension = container.Extend<AddMetadataExtension>();
			extension.Should()
			         .NotBeNull();

			sut.FirstOfType<AddMetadataExtension>().Should().NotBeNull();

			sut.Get(Categories.Content)
			   .Should()
			   .NotContain(extension);

			sut.Get(Categories.Format)
			   .Should()
			   .Contain(extension);

			sut.Get(Categories.Format)
			   .Last()
			   .Should()
			   .BeSameAs(extension);
		}

		[Fact]
		public void VerifyInsertFromMetadata()
		{
			var sut       = new ExtensionCollection();
			var container = new ConfigurationContainer(sut);

			sut.FirstOfType<InsertMetadataExtension>().Should().BeNull();

			var extension = container.Extend<InsertMetadataExtension>();
			extension.Should()
			         .NotBeNull();

			sut.FirstOfType<InsertMetadataExtension>().Should().NotBeNull();

			sut.Get(Categories.Content)
			   .Should()
			   .NotContain(extension);

			var collection = sut.Get(Categories.Elements);
			collection
			   .Should()
			   .Contain(extension);

			collection
			   .First()
			   .Should()
			   .BeSameAs(extension);
		}

		[Fact]
		public void VerifyAware()
		{
			var sut       = new ExtensionCollection();
			var container = new ConfigurationContainer(sut);

			sut.FirstOfType<Aware>().Should().BeNull();

			var extension = container.Extend<Aware>();
			extension.Should()
			         .NotBeNull();

			sut.FirstOfType<Aware>().Should().NotBeNull();

			sut.Get(Categories.Content)
			   .Should()
			   .NotContain(extension);

			var collection = sut.Get(Categories.ObjectModel);
			collection
				.Should()
				.Contain(extension);

			collection
				.Last()
				.Should()
				.BeSameAs(extension);
		}

		[Fact]
		public void VerifyGroupCollectionAware()
		{
			var sut       = new ExtensionCollection();
			var container = new ConfigurationContainer(sut);

			sut.FirstOfType<GroupCollectionAware>().Should().BeNull();

			var extension = container.Extend<GroupCollectionAware>();
			extension.Should()
			         .NotBeNull();

			sut.FirstOfType<GroupCollectionAware>().Should().NotBeNull();

			sut.Get(Categories.Content)
			   .Should()
			   .NotContain(extension);

			var collection = sut.Get(Categories.Caching);
			collection
				.Should()
				.Contain(extension);

			collection
				.First()
				.Should()
				.BeSameAs(extension);
		}

		[Fact]
		public void VerifySort()
		{
			var sut       = new ExtensionCollection(Groups.Default);
			var container = new ConfigurationContainer(sut);

			sut.FirstOfType<GroupCollectionAware>().Should().BeNull();

			sut.FirstOfType<First>().Should().BeNull();
			sut.FirstOfType<Second>().Should().BeNull();
			sut.FirstOfType<Third>().Should().BeNull();
			sut.FirstOfType<Fourth>().Should().BeNull();
			sut.FirstOfType<Fifth>().Should().BeNull();

			var group = sut.Get(Categories.Finish);
			group.Should().BeEmpty();

			group.AddingAll(Fourth.Default, Second.Default, First.Default, Fifth.Default, Third.Default);

			group.Should().ContainInOrder(Fourth.Default, Second.Default, First.Default, Fifth.Default, Third.Default);

			sut.FirstOfType<First>().Should().NotBeNull();
			sut.FirstOfType<Second>().Should().NotBeNull();
			sut.FirstOfType<Third>().Should().NotBeNull();
			sut.FirstOfType<Fourth>().Should().NotBeNull();
			sut.FirstOfType<Fifth>().Should().NotBeNull();

			EnumerableEx.TakeLast(sut, 5).Should().ContainInOrder(First.Default, Second.Default, Third.Default, Fourth.Default, Fifth.Default);
		}

		sealed class First : ISerializerExtension
		{
			public static First Default { get; } = new First();
			First() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
		}

		[Sort(2)]
		sealed class Second : ISerializerExtension
		{
			public static Second Default { get; } = new Second();
			Second() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
		}

		sealed class Third : ISerializerExtension, ISortAware
		{
			public static Third Default { get; } = new Third();
			Third() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
			public int Get() => 3;
		}

		[Sort(4)]
		sealed class Fourth : ISerializerExtension
		{
			public static Fourth Default { get; } = new Fourth();
			Fourth() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();

		}

		sealed class Fifth : ISerializerExtension, ISortAware
		{
			public static Fifth Default { get; } = new Fifth();
			Fifth() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
			public int Get() => 5;
		}

		sealed class AddExtension : ISerializerExtension
		{
			public static AddExtension Default { get; } = new AddExtension();
			AddExtension() {}

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
		}

		sealed class Aware : ISerializerExtension, IGroupNameAware
		{
			public static Aware Default { get; } = new Aware();
			Aware() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
			public GroupName Get() => Categories.ObjectModel;
		}


		sealed class GroupCollectionAware : ISerializerExtension, IGroupCollectionAware<ISerializerExtension>
		{
			public static GroupCollectionAware Default { get; } = new GroupCollectionAware();
			GroupCollectionAware() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();

			public void Execute(IGroupCollection<ISerializerExtension> parameter)
			{
				parameter.Get(Categories.Caching)
				         .Insert(0, this);
			}
		}

		[GroupElement(nameof(Categories.Format))]
		sealed class AddMetadataExtension : ISerializerExtension
		{
			public static AddMetadataExtension Default { get; } = new AddMetadataExtension();
			AddMetadataExtension() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
		}

		[GroupElement(nameof(Categories.Elements)), InsertGroupElement]
		sealed class InsertMetadataExtension : ISerializerExtension
		{
			public static InsertMetadataExtension Default { get; } = new InsertMetadataExtension();
			InsertMetadataExtension() { }

			public IServiceRepository Get(IServiceRepository parameter) => throw new NotImplementedException();

			public void Execute(IServices parameter) => throw new NotImplementedException();
		}

		sealed class Subject
		{
			public string Message { get; set; }
		}

		sealed class Groups : ItemsBase<IGroup<ISerializerExtension>>
		{
			public static Groups Default { get; } = new Groups();
			Groups() : this(DefaultMetadataSpecification.Default, DefaultMemberOrder.Default) { }

			readonly IMetadataSpecification _metadata;
			readonly IParameterizedSource<MemberInfo, int> _defaultMemberOrder;

			public Groups(IMetadataSpecification metadata,
								 IParameterizedSource<MemberInfo, int> defaultMemberOrder)
			{
				_metadata = metadata;
				_defaultMemberOrder = defaultMemberOrder;
			}

			public override IEnumerator<IGroup<ISerializerExtension>> GetEnumerator()
			{
				var all = new KeyedByTypeCollection<ISerializerExtension>();
				yield return new Group(Categories.Start, all,
									   new ConfigurationServicesExtension()
									  );

				yield return new Group(Categories.TypeSystem, all,
									   TypeModelExtension.Default,
									   SingletonActivationExtension.Default,
									   new MemberNamesExtension(),
									   new MemberOrderingExtension(_defaultMemberOrder),
									   ImmutableArrayExtension.Default,
									   MemberModelExtension.Default
									  );
				yield return new Group(Categories.ObjectModel, all,
									   new DefaultReferencesExtension());
				yield return new Group(Categories.Framework, all,
									   SerializationExtension.Default);
				yield return new Group(Categories.Elements, all);
				yield return new Group(Categories.Content, all,
									   ContentModelExtension.Default,
									   Contents.Default,
									   new AllowedMembersExtension(_metadata),
									   new AllowedMemberValuesExtension(),
									   new ConvertersExtension(),
									   new RegisteredSerializersExtension()
									  );
				yield return new Group(Categories.Format, all,
									   new XmlSerializationExtension(),
									   new MemberFormatExtension()
									  );
				yield return new Group(Categories.Caching, all, CachingExtension.Default);
				yield return new Group(Categories.Finish, all);
			}
		}
	}
}