using Com.Github.Knose1.Common.Attributes.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.Github.Knose1.Common.Attributes.Abstract
{
	public abstract class DisabledHide : Disabled
	{
		public bool hide;

		public DisabledHide(bool hide) => this.hide = hide;
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public abstract class DisabledOn : DisabledHide
	{
		public string onField;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hide"></param>
		/// <param name="onField">The field where to compare</param>
		public DisabledOn(bool hide, string onField) : base(hide) => this.onField = onField;

		public abstract bool onCondition(FieldInfo field, object instance);
	}

	public abstract class DisabledOnNumber : DisabledOn
	{
		
		public NumberComparisionType comparisionType;

		public DisabledOnNumber(bool hide, string onField, NumberComparisionType comparisionType) : base(hide, onField) => this.comparisionType = comparisionType;
	}

	public abstract class DisabledOnEnum : DisabledOn
	{
		
		public ClassicComparisionType comparisionType;

		public DisabledOnEnum(bool hide, string onField, ClassicComparisionType comparisionType) : base(hide, onField) => this.comparisionType = comparisionType;

		public bool EnumComparition<T>(FieldInfo field, object instance, T value) where T : Enum
		{
			T fieldValue = (T)field.GetValue(instance);
			bool @return = false;
			switch (comparisionType)
			{
				case ClassicComparisionType.equal:
					@return = fieldValue.Equals(value);
					break;
				case ClassicComparisionType.notEqual:
					@return = !fieldValue.Equals(value);
					break;
			}

			return @return;
		}

		public bool EnumComparition<T>(FieldInfo field, object instance, T[] value) where T : Enum
		{
			for (int i = value.Length - 1; i >= 0; i--)
			{
				if (EnumComparition(field, instance, value[i])) return true;
			}

			return false;
		}
	}
}

namespace Com.Github.Knose1.Common.Attributes
{
	public enum ClassicComparisionType
	{
		equal,
		notEqual
	}

	public enum NumberComparisionType
	{
		equal,
		notEqual,
		superior,
		inferior,
		equalSuperior,
		equalInferior
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class Disabled : Attribute {}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class DisabledInRuntime : DisabledHide
	{
		public DisabledInRuntime(bool hide) : base(hide) {}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class DisabledInEditor : DisabledHide
	{
		public DisabledInEditor(bool hide) : base(hide) {}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class DisabledIfComponent : DisabledHide
	{
		public Type component;
		public bool toBeOrNotToBe;

		public DisabledIfComponent(bool hide, Type component, bool toBeOrNotToBe) : base(hide)
		{
			this.component = component;
			this.toBeOrNotToBe = toBeOrNotToBe;
		}

		public bool Test(GameObject gameObject)
		{
			return toBeOrNotToBe == gameObject.GetComponent(component);
		}
	}

	public class DisabledOnFloat : DisabledOnNumber
	{
		public float value;
		public DisabledOnFloat(bool hide, string onField, NumberComparisionType comparisionType, float value) : base(hide, onField, comparisionType) => this.value = value;

		public override bool onCondition(FieldInfo field, object instance)
		{
			float fieldValue = (float)field.GetValue(instance);

			switch (comparisionType)
			{
				case NumberComparisionType.equal:
					return value == fieldValue;

				case NumberComparisionType.notEqual:
					return value != fieldValue;

				case NumberComparisionType.superior:
					return value < fieldValue;
				
				case NumberComparisionType.inferior:
					return value > fieldValue;
				
				case NumberComparisionType.equalSuperior:
					return value <= fieldValue;
				
				case NumberComparisionType.equalInferior:
					return value >= fieldValue;

				default:
					return false;
			}
		}
	}

	public class DisabledOnInt : DisabledOnNumber
	{
		public int value;
		public DisabledOnInt(bool hide, string onField, NumberComparisionType comparisionType, int value) : base(hide, onField, comparisionType) => this.value = value;

		public override bool onCondition(FieldInfo field, object instance)
		{
			int fieldValue = (int)field.GetValue(instance);

			switch (comparisionType)
			{
				case NumberComparisionType.equal:
					return value == fieldValue;

				case NumberComparisionType.notEqual:
					return value != fieldValue;
				
				case NumberComparisionType.superior:
					return value < fieldValue;
				
				case NumberComparisionType.inferior:
					return value > fieldValue;
				
				case NumberComparisionType.equalSuperior:
					return value <= fieldValue;
				
				case NumberComparisionType.equalInferior:
					return value >= fieldValue;

				default:
					return false;
			}
		}
	}

	public class DisabledOnUInt : DisabledOnNumber
	{
		public uint value;
		public DisabledOnUInt(bool hide, string onField, NumberComparisionType comparisionType, uint value) : base(hide, onField, comparisionType) => this.value = value;

		public override bool onCondition(FieldInfo field, object instance)
		{
			uint fieldValue = (uint)field.GetValue(instance);

			switch (comparisionType)
			{
				case NumberComparisionType.equal:
					return value == fieldValue;

				case NumberComparisionType.notEqual:
					return value != fieldValue;

				case NumberComparisionType.superior:
					return value < fieldValue;
				
				case NumberComparisionType.inferior:
					return value > fieldValue;
				
				case NumberComparisionType.equalSuperior:
					return value <= fieldValue;
				
				case NumberComparisionType.equalInferior:
					return value >= fieldValue;

				default:
					return false;
			}
		}
	}

	public class DisabledOnBool : DisabledOn
	{
		public bool value;
		public DisabledOnBool(bool hide, string onField, bool value) : base(hide, onField) => this.value = value;

		public override bool onCondition(FieldInfo field, object instance) => value == (bool)field.GetValue(instance);
	}
}
