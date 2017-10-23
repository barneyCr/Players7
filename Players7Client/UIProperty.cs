using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Players7Client
{
    public delegate void ValueChangedHandler<T>();
    public class UIProperty<T>
    {
        private T m;
        public event ValueChangedHandler<T> ValueChanged;
        public UIProperty(T member)
        {
            this.m = member;
        }

        public T Value
        {
            get
            {
                return this.m;
            }
            set
            {
                this.m = value;
                if (ValueChanged != null)
                    this.ValueChanged();
            }
        }

        public static implicit operator T(UIProperty<T> value)
        {
            return value.m;
        }
    }
}