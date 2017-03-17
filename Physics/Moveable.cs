using System;
using System.Windows.Media.Media3D;

namespace Physics
{

    public class Moveable
    {
        enum Axis { X, Y, Z };

        DateTime _t0;
        float _x0, _y0, _z0;
        float _v0x, _v0y, _v0z;
        float _ax, _ay, _az;
       
        Point3D _lastPoistion;

        float _minX, _minY, _minZ;
        float _maxX, _maxY, _maxZ;
        public Moveable(float x0, float y0, float z0,
                        float v0x, float v0y, float v0z,
                        float ax, float ay, float az)
        {
            _x0 = x0;
            _y0 = y0;
            _z0 = z0;
            _v0x = v0x;
            _v0y = v0y;
            _v0z = v0z;
            _ax = ax;
            _ay = ay;
            _az = az;
            _minX = float.MinValue;
            _minY = float.MinValue;
            _minZ = float.MinValue;
            _maxX = float.MaxValue;
            _maxY = float.MaxValue;
            _maxZ = float.MaxValue;
            _lastPoistion = new Point3D(_x0, _y0, _z0);
        }

        public void SetBoundaries(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            _minX = minX;
            _minY = minY;
            _minZ = minZ;
            _maxX = maxX;
            _maxY = maxY;
            _maxZ = maxZ;
        }

        public void Init()
        {
            _t0 = DateTime.Now;
        }

        public Point3D GetNextPosition()
        {
            var newPosition = new Point3D();
            double dt = (DateTime.Now - _t0).TotalSeconds/40;
            newPosition.X = calculate(dt, Axis.X);
            newPosition.Y = calculate(dt, Axis.Y);
            newPosition.Z = calculate(dt, Axis.Z);
            _lastPoistion = newPosition;
            return _lastPoistion;
        }

        public Point3D GetLastPosition()
        {
            return _lastPoistion;
        }

        float calculate(double dt, Axis axe)
        {
            double res = 0;
            switch (axe)
            {
                case Axis.X:
                    res = _x0 + _v0x * dt + 0.5 * _ax * dt * dt;
                    if (res > _maxX)
                    {
                        res = _maxX;
                    }
                    else if (res < _minX)
                    {
                        res = _minX;
                    }
                    break;
                case Axis.Y:
                    res = _y0 + _v0y * dt + 0.5 * _ay * dt * dt;
                    if (res > _maxY)
                    {
                        res = _maxY;
                    }
                    else if (res < _minY)
                    {
                        res = _minY;
                    }
                    break;
                case Axis.Z:
                    res = _z0 + _v0z * dt + 0.5 * _az * dt * dt;
                    if (res > _maxZ)
                    {
                        res = _maxZ;
                    }
                    else if (res < _minZ)
                    {
                        res = _minZ;
                    }
                    break;
            }

            return (float)res;
        }
    }
}
