using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingKinect
{
    public enum Protocol
    {
        STATES = 1,
        KINECT_START = 10,
        KINECT_STOP = 11,
        KINECT_CHANGED_AVAILABILITY = 12,

        GET_RANDOM_OBJECT_INSTRUCTIONS = 100,
        GET_MOVING_OBJECT_INSTRUCTIONS = 101,
        GET_NEXT_INSTRUCTIONS = 199,
        SKELETON_DATA = 201
    }
}
