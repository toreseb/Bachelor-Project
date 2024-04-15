﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project.Electrode_Types
{
    
    public class Sensor(int x, int y, int sizeX, int sizeY, string name = "") : Apparature(x, y, sizeX, sizeY, name)
    {
        readonly int SensorID;

        public Sensor() : this(0, 0, 1, 1)
        {
            CoilInto = false;
        }
    }

    


}
