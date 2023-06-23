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
        public int VAO { get; }

        public int VBO { get; }

        public int EBO { get; }
        public void OnLoad(Shader shader);
        public void OnRenderFrame(Shader shader);
        public void OnUnload();
    }
}