using ExtendedXmlSerializer.ContentModel.Members;
using ExtendedXmlSerializer.ExtensionModel.Content.Members;
using ExtendedXmlSerializer.ExtensionModel.Xml.Classic;
using System;

namespace ExtendedXmlSerializer.Configuration
{
	/// <summary>
	/// A set of built-in, identified behaviors that configure how a serializer emits content when it serializes an object.
	/// </summary>
	public static class EmitBehaviors
	{
		/// <summary>
		/// Ensures that content is always emitted, regardless of its value.
		/// </summary>
		public static IEmitBehavior Always { get; } =
			new EmitBehavior(new AllowedSpecificationAlteration(AlwaysEmitMemberSpecification.Default));

		/// <summary>
		/// Follows the classic serializer behavior for emitting content.  For classic serialization, the serializer always emits the value when it is a System.<see cref="Enum"/>.  Otherwise, it emits if the value is assigned (non-null).
		/// </summary>
		public static IEmitBehavior Classic { get; } =
			new EmitBehavior(new AddAlteration(ClassicAllowedMemberValues.Default));

		/// <summary>
		/// This configures the container to emit when the value is assigned.  That is, not null.
		/// </summary>
		public static IEmitBehavior WhenAssigned { get; } =
			new EmitBehavior(new AllowedSpecificationAlteration(AllowAssignedValues.Default));

		/// <summary>
		/// This is a variant of the <see cref="WhenAssigned"/> behavior.  With this behavior, the serializer emits when the
		/// value is different from the defined value in the class.  For instance, if you have a property
		/// <code>public bool MyProperty {get; set} = true</code> and <code>MyProperty</code> is <code>false</code> upon
		/// serialization, then the content is emitted.
		/// </summary>
		public static IEmitBehavior WhenModified { get; } =
			new EmitBehavior(new AddAlteration(AllowedAssignedInstanceValues.Default));

		#region Obsolete

		[Obsolete("This is considered deprecated and will be removed in a future release.  Use EmitBehaviors.WhenModified instead.")]
		public static IEmitBehavior Assigned { get; } =
			new EmitBehavior(new AddAlteration(AllowedAssignedInstanceValues.Default));

		[Obsolete("This is considered deprecated and will be removed in a future release.  Use EmitBehaviors.WhenAssigned instead.")]
		public static IEmitBehavior NotDefault { get; } =
			new EmitBehavior(new AllowedSpecificationAlteration(AllowAssignedValues.Default));

		#endregion
	}
}