using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Utils
{
    public class ObjectPoolList<T>
    {
        private readonly List<T> _objects;
        private readonly Func<T> _objectGenerator;

        public ObjectPoolList(Func<T> objectGenerator, int startPool = 0)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _objects = new List<T>();

            for (int i = 0; i < startPool; i++)
                _objects.Add(_objectGenerator());
        }

        public T Get() {
            if (_objects.Count > 0)
            {
                var result = _objects[0];
                _objects.RemoveAt(0);
                return result;
            }

            return _objectGenerator();
        }

        public void Return(T item) => _objects.Add(item);
    }
}
