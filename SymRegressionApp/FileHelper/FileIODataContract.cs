//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Net;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SymRegressionApp.FileHelper
{
    public class FileIODataContract<T> : FileIOBase<T>
    {
        public List<Type> knownTypes = new List<Type>();
        public override T OpenWithKnownFilename(Type appType, string FileName)
        {
            StreamReader stream = new StreamReader(FileName);
            DataContractSerializer serializer = new DataContractSerializer(typeof(T), knownTypes);
            object obj = serializer.ReadObject(stream.BaseStream);
            T data = (T)obj;
            stream.Close();
            return data;
        }

        public override void SaveWithKnownFilename(T InApp, string FileName)
        {
            StreamWriter stream = new StreamWriter(FileName);
            DataContractSerializer serializer = new DataContractSerializer(typeof(T), knownTypes);
            serializer.WriteObject(stream.BaseStream, InApp);
            stream.Flush();
            stream.Close();
        }


    }
}
