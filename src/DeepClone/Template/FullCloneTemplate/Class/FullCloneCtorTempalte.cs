﻿using Natasha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DeepClone.Template
{
    public class FullCloneCtorTempalte
    {


        private HashSet<Dictionary<string, Type>> _ctors;
        private Dictionary<Dictionary<string, Type>, Dictionary<string, string>> _parametersMapping;
        private Dictionary<Dictionary<string, Type>, int> _values;


        public FullCloneCtorTempalte(Type type)
        {

            _ctors = new HashSet<Dictionary<string, Type>>();
            _values = new Dictionary<Dictionary<string, Type>, int>();
            _parametersMapping = new Dictionary<Dictionary<string, Type>, Dictionary<string, string>>();
            var temp = type.GetConstructors();


            for (int i = 0; i < temp.Length; i++)
            {

                var dict = new Dictionary<string, Type>();
                _ctors.Add(dict);
                _parametersMapping[dict] = new Dictionary<string, string>();
                _values[dict] = 0;
                var parameters = temp[i].GetParameters();


                foreach (var item in parameters)
                {

                    dict[item.Name.ToUpper()] = item.ParameterType;
                    _parametersMapping[dict][item.Name.ToUpper()] = item.Name;
                }

            }

        }




        public string GetCtor(IEnumerable<NBuildInfo> infos)
        {

            Dictionary<string, Type> pairs = default;
            int preResult = 0;
            foreach (var item in _ctors)
            {

                int value = 0;
                foreach (var info in infos)
                {

                    if (item.ContainsKey(info.DeclaringAvailableName))
                    {

                        if (item[info.DeclaringAvailableName] == info.MemberType)
                        {
                            value += 1;
                        }

                    }

                }


                if (preResult < value)
                {
                    preResult = value;
                    pairs = item;
                }


            }

            if (pairs != default)
            {

                var cache = _parametersMapping[pairs];
                StringBuilder scriptBuilder = new StringBuilder();
                foreach (var item in infos)
                {

                    var name = item.DeclaringAvailableName;
                    if (cache.ContainsKey(name))
                    {

                        if (pairs[name] == item.MemberType)
                        {
                            scriptBuilder.Append($"{cache[name]}:old.{item.MemberName},");
                            pairs.Remove(name);
                        }
                        
                    }
                   

                }


                foreach (var item in pairs)
                {
                    scriptBuilder.Append($"{cache[item.Key]}:default,");
                }


                if (scriptBuilder.Length>0)
                {
                    scriptBuilder.Length -= 1;
                }
                return scriptBuilder.ToString();

            }

            return default;

        }

    }

}
