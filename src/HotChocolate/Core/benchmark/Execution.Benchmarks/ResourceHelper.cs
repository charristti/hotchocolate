﻿using System.IO;
using System.Reflection;
using System.Text;

namespace HotChocolate.Execution.Benchmarks
{
    public class ResourceHelper
    {
        private const string _resourcePath =
            "HotChocolate.Execution.Benchmarks.Resources";
        private Assembly _assembly;

        public ResourceHelper()
        {
            _assembly = GetType().Assembly;
        }

        public string GetResourceString(string fileName)
        {
            var stream = GetResourceStream(fileName);
            if (stream != null)
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            throw new FileNotFoundException(
                "Could not find the specified resource file",
                fileName);
        }

        private Stream GetResourceStream(string fileName)
        {
            return _assembly.GetManifestResourceStream(
                $"{_resourcePath}.{fileName}");
        }
    }
}
