using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Physics
{
    public class BoundingRect
    {
        float _x, _y, _z, _width, _height, _depth;
        public BoundingRect(float x, float y, float z, float width, float height, float depth)
        {
            _x = x;
            _y = y;
            _z = z;
            _width = width;
            _height = height;
            _depth = depth;
        }

        public BoundingRect Inflate(float xInfltionRatio, float yInflationRatio, float zInflationRatio)
        {
            var heightAddition = _height * yInflationRatio;
            var widthAddition = 2* _width* xInfltionRatio;
            var rect = new BoundingRect(
                _x - _width * xInfltionRatio,
                _y + heightAddition,
                //_z - _depth * zInflationRatio,
                _z,
                _width + widthAddition,
                _height + heightAddition,
                //_depth * (1 + 2 * zInflationRatio)
                _depth
                );
            return rect;
        }
        public float X { get { return _x; } }
        public float Y { get { return _y; } }
        public float Z { get { return _z; } }
        public float Height { get { return _height; } }
        public float Width { get { return _width; } }
        public float Depth { get { return _depth; } }

        public List<Point3D> GetCorners()
        {
            var points = new List<Point3D>();
            points.Add(new Point3D(_x, _y, _z));
            points.Add(new Point3D((_x + _width), _y, _z));
            points.Add(new Point3D((_x + _width), (_y - _height), _z));
            points.Add(new Point3D(_x, (_y - _height), _z));
            return points;
        }


        public override string ToString()
        {
            return string.Format("BoundingRect - X:{0}, Y:{1}, Z:{2}, Width:{3}, Height:{4}, Depth:{5}", _x, _y, _z, _width, _height, _depth);
        }
    }
}
