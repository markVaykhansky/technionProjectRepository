using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Physics
{
    public class Slice
    {
        float _y, _x, _z, _width, _height, _depth;
        public Slice(float x, float y, float z, float width, float height, float depth)
        {
            _x = x;
            _y = y;
            _z = z;
            _width = width;
            _height = height;
            _depth = depth;
        }

        public Point3D ConvertPoint(float x, float y, float z)
        {
            var p = new Point3D();
            p.X = _x + x * _width;
            //for top-left coordinate system
            p.Y = _y - y * _height;
            
            //for bottom-left coordinate system
            //p.Y = _y - y * _height;
            p.Z = _z + z * _depth;
            return p;
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
            points.Add(new Point3D(_x , _y ,_z));
            points.Add(new Point3D((_x+_width),_y,_z));
            points.Add(new Point3D((_x + _width) , (_y-_height) , _z));
            points.Add(new Point3D(_x, (_y - _height) , _z));
            return points;
        }

        public override string ToString()
        {
            return string.Format("Slice - X:{0}, Y:{1}, Z:{2}, Width:{3}, Height:{4}, Depth:{5}", _x, _y, _z, _width, _height, _depth);
        }
    }
}
