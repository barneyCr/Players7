using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Players7Client
{
    public delegate void ValueChangedHandler<T>(string modification);
    public class UIProperty<T>
    {
        protected T m;
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
                InvokeEvent("set_Value()");
            }
        }

        protected void InvokeEvent(string modif)
        {
            if (ValueChanged != null)
                this.ValueChanged(modif);
        }

        public static implicit operator T(UIProperty<T> value)
        {
            return value.m;
        }
    }

    public class UIPropertyCollection<T> : UIProperty<List<T>>, IEnumerable<T>
    {
        public UIPropertyCollection(List<T> member) : base(member)
        {
        }

        public void Add(T item)
        {
            base.m.Add(item);
            base.InvokeEvent("add()");
        }

        public void Remove(T item)
        {
            if (base.m.Remove(item))
            {
                base.InvokeEvent("remove()");
            }
        }

        public void Clear() {
            base.m.Clear();
            base.InvokeEvent("clear()");
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m.GetEnumerator();
        }
    }
}