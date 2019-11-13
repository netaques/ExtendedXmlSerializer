﻿using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer.ContentModel;
using ExtendedXmlSerializer.ContentModel.Content;
using ExtendedXmlSerializer.ContentModel.Conversion;
using ExtendedXmlSerializer.ContentModel.Members;
using ExtendedXmlSerializer.Core;
using ExtendedXmlSerializer.Core.Sources;
using ExtendedXmlSerializer.Core.Specifications;
using ExtendedXmlSerializer.ExtensionModel;
using ExtendedXmlSerializer.ExtensionModel.Content;
using ExtendedXmlSerializer.ExtensionModel.Content.Members;
using ExtendedXmlSerializer.ExtensionModel.Instances;
using ExtendedXmlSerializer.ReflectionModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExtendedXmlSerializer
{
	// ReSharper disable once MismatchedFileName
	public static partial class ExtensionMethods
	{
		/// <summary>
		/// Convenience method for extension authors.  This is used to establish a context to decorate the container's
		/// <see cref="IContents"/> component.
		/// </summary>
		/// <typeparam name="T">The implementation type, of type IContent.</typeparam>
		/// <param name="this">The repository to configure (used within an extension).</param>
		/// <returns>The configured repository.</returns>
		public static ContentsDecorationContext<T> DecorateContentsWith<T>(this IServiceRepository @this)
			where T : IContents
			=> new ContentsDecorationContext<T>(@this);

		/// <summary>
		/// Convenience method for extension authors.  This is used to establish a fluent context which can further be used to
		/// decorate the container's <see cref="IElement"/> component.
		/// </summary>
		/// <typeparam name="T">The implementation type, of type IElement.</typeparam>
		/// <param name="this">The repository to configure.</param>
		/// <returns>The configured repository.</returns>
		public static ElementDecorationContext<T> DecorateElementWith<T>(this IServiceRepository @this)
			where T : IElement
			=> new ElementDecorationContext<T>(@this);

		/* Extension Model: */

		/// <summary>
		/// Assigns a default serialization monitor for a configuration container.  A serialization monitor is a component
		/// that gets notified whenever there is a serialization such as OnSerializing, OnSerialized, as well as
		/// deserialization events such as OnDeserializing, OnDeserialized, etc.
		///
		/// The default serialization monitor is applied for every type that is serialized with the serializer that the
		/// configured container creates.  Use <see cref="WithMonitor{T}"/> on a type configuration to
		/// apply a monitor to a specific type.
		/// </summary>
		/// <param name="this">The configuration container to configure.</param>
		/// <param name="monitor">The monitor to assign as the default monitor.</param>
		/// <returns>The configured container.</returns>
		/// <seealso href="https://github.com/ExtendedXmlSerializer/ExtendedXmlSerializer/issues/264"/>
		public static IConfigurationContainer WithDefaultMonitor(this IConfigurationContainer @this,
		                                                         ISerializationMonitor monitor)
			=> @this.Extend(new SerializationMonitorExtension(monitor));

		/// <summary>
		/// Applies a serialization monitor to a specific type.  A serialization monitor is a component that gets notified
		/// whenever there is a serialization such as OnSerializing, OnSerialized, as well as deserialization events such as
		/// OnDeserializing, OnDeserialized, etc.
		///
		/// Note that calling this method will establish a default monitor if one has not already been assigned.  If you also
		/// want to use a default monitor in addition to type-specific monitors, call the <see cref="WithDefaultMonitor" />
		/// first before calling this method on any types.
		/// </summary>
		/// <typeparam name="T">The type to monitor.</typeparam>
		/// <param name="this">The type configuration to configure.</param>
		/// <param name="monitor">The monitor to apply to the specified type.</param>
		/// <returns>The configured type configuration.</returns>
		/// <seealso href="https://github.com/ExtendedXmlSerializer/ExtendedXmlSerializer/issues/264" />
		public static ITypeConfiguration<T> WithMonitor<T>(this ITypeConfiguration<T> @this,
		                                                   ISerializationMonitor<T> monitor)
		{
			@this.Root.With<SerializationMonitorExtension>()
			     .Assign(Support<T>.Key, new SerializationMonitor<T>(monitor));
			return @this;
		}

		/// <summary>
		/// Allows content to be read as parameters for a constructor call to activate an object, rather than the more
		/// traditional route of activating an object and its content read as property assignments.  This is preferred --
		/// required, even -- if your model is comprised of immutable objects.
		///
		/// Note that there are several requirements for a class to be successfully processed:
		/// <list type="number">
		///		<item>only public fields / properties are considered</item>
		///		<item>any public fields (spit) must be readonly</item>
		///		<item>any public properties must have a get but not a set (on the public API, at least)</item>
		///		<item>there must be exactly one interesting constructor, with parameters that are a case-insensitive match for each field/property in some order (i.e. there must be an obvious 1:1 mapping between members and constructor parameter names)</item>
		/// </list>
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <returns>The configured container.</returns>
		/// <seealso href="https://github.com/ExtendedXmlSerializer/ExtendedXmlSerializer/wiki/04.-Features#immutable-classes-and-content"/>
		public static IConfigurationContainer EnableParameterizedContent(this IConfigurationContainer @this)
			=> @this.Extend(ParameterizedMembersExtension.Default);

		/// <summary>
		/// This is a less strict version of <see cref="EnableParameterizedContent"/>.  Using this version, parameterized
		/// content works the same as <see cref="EnableParameterizedContent"/> but in addition, all properties defined in the
		/// deserialized document are also considered and assigned to the target instance if the property is writable.
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer EnableParameterizedContentWithPropertyAssignments(
			this IConfigurationContainer @this)
			=> @this.Extend(AllParameterizedMembersExtension.Default);

		/// <summary>
		/// Intended for extension authors, and enables a reader context on the deserialization process.  Extension authors
		/// can use <seealso cref="ContentsHistory"/> to retrieve this history of objects being parsed and activated to the
		/// current point of the graph.  This is valuable when parsing object graphs with many internal properties which in
		/// turn have their own set of complex properties.
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer EnableReaderContext(this IConfigurationContainer @this)
			=> @this.Extend(ReaderContextExtension.Default);

		/// <summary>
		/// This is intended to circumvent default behavior which throws an exception for primitive data types when there is
		/// no content provided for their elements.  For example, say you have a boolean element defined as such:
		/// <code>&lt;Boolean /&gt;</code>  Or perhaps the long-form version: <code>&lt;Boolean&gt;&lt;/Boolean&gt;</code>
		///
		/// Either one of these will throw a <seealso cref="FormatException"/>.  Configuring the container with
		/// <seealso cref="EnableImplicitlyDefinedDefaultValues"/> will allow the use of empty values within document
		/// elements.
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer EnableImplicitlyDefinedDefaultValues(this IConfigurationContainer @this)
			=> @this.Alter(ImplicitlyDefinedDefaultValueAlteration.Default);

		/* Emit: */

		/// <summary>
		/// Used to control and determine when content is emitted during serialization.  This is a general-purpose
		/// configuration that works across every type encountered by the serializer. Use the <seealso cref="EmitBehaviors" />
		/// class to utilize one of the built-in (and identified 😁) behaviors, or implement your own
		/// <see cref="IEmitBehavior"/>.
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <param name="behavior">The behavior to apply to the container.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer Emit(this IConfigurationContainer @this, IEmitBehavior behavior)
			=> behavior.Get(@this);

		/// <summary>
		/// Configures a member configuration to only emit when its value meets certain criteria.
		/// </summary>
		/// <typeparam name="T">The containing type of the member.</typeparam>
		/// <typeparam name="TMember">The member type.</typeparam>
		/// <param name="this">The member to configure.</param>
		/// <param name="specification">The specification to use to determine whether or not to emit the member, based on value.</param>
		/// <returns>The configured member.</returns>
		public static IMemberConfiguration<T, TMember> EmitWhen<T, TMember>(this IMemberConfiguration<T, TMember> @this,
		                                                                    Func<TMember, bool> specification)
		{
			@this.Root.Find<AllowedMemberValuesExtension>()
			     .Specifications[@this.GetMember()] =
				new AllowedValueSpecification(new DelegatedSpecification<TMember>(specification).AdaptForNull());
			return @this;
		}

		/// <summary>
		/// Configures a member configuration to only emit when a condition of its containing instance is met.  This is useful
		/// for when a data value from another member in another part of the containing instance is needed to determine
		/// whether or not to emit the (currently) configured member.
		/// </summary>
		/// <typeparam name="T">The containing type of the member.</typeparam>
		/// <typeparam name="TMember">The member type.</typeparam>
		/// <param name="this">The member to configure.</param>
		/// <param name="specification"></param>
		/// <returns>The configured member.</returns>
		public static IMemberConfiguration<T, TMember> EmitWhenInstance<T, TMember>(
			this IMemberConfiguration<T, TMember> @this,
			Func<T, bool> specification)
		{
			@this.Root.Find<AllowedMemberValuesExtension>()
			     .Instances[@this.GetMember()] = new DelegatedSpecification<T>(specification).AdaptForNull();
			return @this;
		}

		/// <summary>
		/// Configures a type configuration so that instances of its type only emit when the provided condition is met.
		/// </summary>
		/// <typeparam name="T">The instance type.</typeparam>
		/// <param name="this">The type configuration to configure.</param>
		/// <param name="specification">The specification to determine the condition on when to emit.</param>
		/// <returns>The configured type configuration.</returns>
		public static ITypeConfiguration<T> EmitWhen<T>(this ITypeConfiguration<T> @this, Func<T, bool> specification)
		{
			@this.Root.With<AllowedInstancesExtension>()
			     .Assign(@this.Get(),
			             new AllowedValueSpecification(new DelegatedSpecification<T>(specification).AdaptForNull()));
			return @this;
		}

		/* Membership: */

		/// <summary>
		/// Convenience method to iterate through all explicitly configured types and include all explicitly configured
		/// members.  Only these members will be considered to emit content during serialization as well as reading it
		/// during deserialization.
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer IncludeConfiguredMembers(this IConfigurationContainer @this)
		{
			foreach (var type in @this)
			{
				type.IncludeConfiguredTypeMembers();
			}

			return @this;
		}

		/// <summary>
		/// Convenience method to iterate through all explicitly configured members of a type and mark them as included.  Only
		/// these members will be considered to emit content during serialization as well as reading it during
		/// deserialization.
		/// </summary>
		/// <typeparam name="T">The type under configuration.</typeparam>
		/// <param name="this">The type to configure.</param>
		/// <returns>The configured type.</returns>
		public static ITypeConfiguration<T> IncludeConfiguredMembers<T>(this ITypeConfiguration<T> @this)
			=> IncludeConfiguredTypeMembers(@this)
				.Return(@this);

		static object IncludeConfiguredTypeMembers(this IEnumerable<IMemberConfiguration> @this)
		{
			foreach (var member in @this)
			{
				member.Include();
			}

			return default;
		}

		/// <summary>
		/// Ignores a member so that it is not emitted during serialization, and is not read in during deserialization, even
		/// if the content is specified in the document.  Note that this establishes a "blacklist" policy so that members that
		/// are not ignored get processed.
		/// </summary>
		/// <typeparam name="T">The instance type.</typeparam>
		/// <typeparam name="TMember">The member type.</typeparam>
		/// <param name="this">The member to configure.</param>
		/// <returns>The configured member.</returns>
		public static IMemberConfiguration<T, TMember> Ignore<T, TMember>(this IMemberConfiguration<T, TMember> @this)
			=> @this.Ignore(@this.GetMember())
			        .Return(@this);

		/// <summary>
		/// Ignores a member so that it is not emitted during serialization, and is not read in during deserialization, even
		/// if the content is specified in the document.  Note that this establishes a "blacklist" policy so that members that
		/// are not ignored get processed.
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <param name="member">The member to ignore.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer Ignore(this IConfigurationContainer @this, MemberInfo member)
		{
			@this.Root.With<AllowedMembersExtension>()
			     .Blacklist.Add(member);
			return @this;
		}

		/// <summary>
		/// Includes a member so that it is emitted during serialization and read during deserialization.  Note that including
		/// a member establishes a "whitelist" policy so that only members that are explicitly included are considered for processing.
		/// </summary>
		/// <typeparam name="T">The type that contains the member.</typeparam>
		/// <typeparam name="TMember">The type of the member's value.</typeparam>
		/// <param name="this">The member to configure.</param>
		/// <returns>The configured member.</returns>
		public static IMemberConfiguration<T, TMember> Include<T, TMember>(this IMemberConfiguration<T, TMember> @this)
			=> @this.To<IMemberConfiguration>()
			        .Include()
			        .Return(@this);

		/// <summary>
		/// Includes a member so that it is emitted during serialization and read during deserialization.  Note that including
		/// a member establishes a "whitelist" policy so that only members that are explicitly included are considered for
		/// processing.
		/// </summary>
		/// <param name="this">The member to configure.</param>
		/// <returns>The configured member.</returns>
		public static IMemberConfiguration Include(this IMemberConfiguration @this)
		{
			@this.Root.With<AllowedMembersExtension>()
			     .Whitelist.Add(@this.GetMember());
			return @this;
		}

		/* Content registration: */

		/// <summary>
		/// Used to alter a serializer whenever one is created for a specific type.  This allows the scenario of decorating
		/// a serializer to override or monitor serialization and/or deserialization.
		/// </summary>
		/// <typeparam name="T">The type that the serializer processes.</typeparam>
		/// <param name="this">The type configuration to configure.</param>
		/// <param name="compose">The delegate used to alterate the created serializer.</param>
		/// <returns>The configured type configuration.</returns>
		/// <seealso href="https://github.com/ExtendedXmlSerializer/ExtendedXmlSerializer/issues/264#issuecomment-531491807"/>
		public static ITypeConfiguration<T> RegisterContentComposition<T>(this ITypeConfiguration<T> @this,
		                                                                  Func<ISerializer<T>, ISerializer<T>> compose)
			=> @this.RegisterContentComposition(new SerializerComposer<T>(compose).Get);

		/// <summary>
		/// Used to alter a serializer whenever one is created for a specific type.  This allows the scenario of decorating
		/// a serializer to override or monitor serialization and/or deserialization.  This override accepts a generalized
		/// serializer delegate.
		/// </summary>
		/// <typeparam name="T">The type that the serializer processes.</typeparam>
		/// <param name="this">The type configuration to configure.</param>
		/// <param name="compose">The delegate used to alterate the created serializer.</param>
		/// <returns>The configured type configuration.</returns>
		/// <seealso href="https://github.com/ExtendedXmlSerializer/ExtendedXmlSerializer/issues/264#issuecomment-531491807"/>
		public static ITypeConfiguration<T> RegisterContentComposition<T>(this ITypeConfiguration<T> @this,
		                                                                  Func<ISerializer, ISerializer> compose)
			=> @this.RegisterContentComposition(new SerializerComposer(compose));

		/// <summary>
		/// Used to alter a serializer whenever one is created for a specific type.  This allows the scenario of decorating a
		/// serializer to override or monitor serialization and/or deserialization.  This override accepts an
		/// <see cref="ISerializerComposer"/> that performs the alteration on the created serializer.
		/// </summary>
		/// <typeparam name="T">The type that the serializer processes.</typeparam>
		/// <param name="this">The type configuration to configure.</param>
		/// <param name="composer">The composer that is used to alter the serializer upon creation.</param>
		/// <returns>The configured type configuration.</returns>
		/// <seealso href="https://github.com/ExtendedXmlSerializer/ExtendedXmlSerializer/issues/264#issuecomment-531491807"/>
		public static ITypeConfiguration<T> RegisterContentComposition<T>(this ITypeConfiguration<T> @this,
		                                                                  ISerializerComposer composer)
		{
			@this.Root.With<RegisteredCompositionExtension>()
			     .Assign(Support<T>.Key, composer);
			return @this;
		}

		/// <summary>
		/// Provides a way to alter converters when they are accessed by the serializer.  This provides a mechanism to
		/// decorate converters.  Alterations only occur once per converter per serializer.
		/// </summary>
		/// <param name="this">The container to configure.</param>
		/// <param name="alteration">The alteration to perform on each converter when it is accessed by the serializer.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer Alter(this IConfigurationContainer @this,
		                                            IAlteration<IConverter> alteration)
		{
			@this.Root.With<ConverterAlterationsExtension>()
			     .Alterations.Add(alteration);
			return @this;
		}

		/// <summary>
		/// Registers a converter for the provided type.  This defines how to deconstruct a type into a string for
		/// serialization, and to construct a string during deserialization.
		/// </summary>
		/// <typeparam name="T">The type to convert.</typeparam>
		/// <param name="this">The configuration container to configure.</param>
		/// <param name="format">The formatter to use during serialization.</param>
		/// <param name="parse">The parser to use during deserialization.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer Register<T>(this IConfigurationContainer @this,
		                                                  Func<T, string> format,
		                                                  Func<string, T> parse)
			=> @this.Register<T>(new Converter<T>(parse, format));

		/// <summary>
		/// Registers a converter for the provided type.  This defines how to deconstruct a type into a string for
		/// serialization, and to construct a string during deserialization.
		/// </summary>
		/// <typeparam name="T">The type to convert.</typeparam>
		/// <param name="this">The configuration container to configure.</param>
		/// <param name="converter">The converter to register.</param>
		/// <returns>The configured container.</returns>
		public static IConfigurationContainer Register<T>(this IConfigurationContainer @this, IConverter<T> converter)
		{
			var item = converter as Converter<T> ?? Converters<T>.Default.Get(converter);
			return @this.Root.Find<ConvertersExtension>()
			            .Converters
			            .AddOrReplace(item)
			            .Return(@this);
		}

		/// <summary>
		/// Removes the registration (if any) from the container's converter registration.
		/// </summary>
		/// <typeparam name="T">The type that the converter processes.</typeparam>
		/// <param name="this">The configuration container to configure.</param>
		/// <param name="converter">The converter to remove from registration.</param>
		/// <returns>The configured container.</returns>
		public static bool Unregister<T>(this IConfigurationContainer @this, IConverter<T> converter)
			=> @this.Root.Find<ConvertersExtension>()
			        .Converters.Removing(converter);

		sealed class Converters<T> : ReferenceCache<IConverter<T>, IConverter<T>>
		{
			public static Converters<T> Default { get; } = new Converters<T>();

			Converters() : base(key => new Converter<T>(key, key.Parse, key.Format)) {}
		}

		#region Obsolete

		[Obsolete(
			"This method is being deprecated and will be removed in a future release. Use Decorate.Element.When instead.")]
		public static IServiceRepository Decorate<T>(this IServiceRepository @this,
		                                             ISpecification<TypeInfo> specification)
			where T : IElement
			=> new ConditionalElementDecoration<T>(specification).Get(@this);

		[Obsolete(
			"This method is being deprecated and will be removed in a future release. Use Decorate.Contents.When instead.")]
		public static IServiceRepository DecorateContent<TSpecification, T>(this IServiceRepository @this)
			where TSpecification : ISpecification<TypeInfo>
			where T : IContents
			=> ConditionalContentDecoration<TSpecification, T>.Default.Get(@this);

		[Obsolete(
			"This method is being deprecated and will be removed in a future release. Use Decorate.Contents.When instead.")]
		public static IServiceRepository DecorateContent<T>(this IServiceRepository @this,
		                                                    ISpecification<TypeInfo> specification) where T : IContents
			=> new ConditionalContentDecoration<T>(specification).Get(@this);

		[Obsolete("This is considered a deprecated feature and will be removed in a future release.")]
		public static IConfigurationContainer OptimizeConverters(this IConfigurationContainer @this)
			=> OptimizeConverters(@this, new Optimizations());

		[Obsolete("This is considered a deprecated feature and will be removed in a future release.")]
		public static IConfigurationContainer OptimizeConverters(this IConfigurationContainer @this,
		                                                         IAlteration<IConverter> optimizations)
			=> @this.Alter(optimizations);

		[Obsolete(
			"This method will be removed in a future release.  Use IConfigurationContainer.IncludeConfiguredMembers instead.")]
		public static IConfigurationContainer OnlyConfiguredProperties(this IConfigurationContainer @this)
			=> @this.IncludeConfiguredMembers();

		[Obsolete(
			"This method will be removed in a future release.  Use ITypeConfiguration<T>.IncludeConfiguredMembers instead.")]
		public static ITypeConfiguration<T> OnlyConfiguredProperties<T>(this ITypeConfiguration<T> @this)
			=> @this.IncludeConfiguredTypeMembers()
			        .Return(@this);

		#endregion
	}
}