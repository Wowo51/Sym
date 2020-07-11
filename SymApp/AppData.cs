using System;
using System.Collections.Generic;
using System.Text;

namespace SymApp
{
    public class AppData
    {
        public List<ScFile> mainFiles = new List<ScFile>();
        public List<ScFile> algebraTransforms = new List<ScFile>();
        public List<ScFile> calculusTransforms = new List<ScFile>();
        public List<ScFile> vectorTransforms = new List<ScFile>();
        public List<ScFile> matrixTransforms = new List<ScFile>();
        public List<ScFile> tensorTransforms = new List<ScFile>();
        public List<ScFile> logicTransforms = new List<ScFile>();
    }
}
