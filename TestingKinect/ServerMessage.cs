﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingKinect
{
    public class ServerMessage<T>
    {
        public Protocol Type { get; set; }
        public T Data { get; set; }

    }
}
