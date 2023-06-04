﻿using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Objects
{
    internal interface IRenderObject
    {
        public void OnLoad(Shader shader);
        public void OnRenderFrame(Shader shader);
        public void OnUnload();
    }
}