﻿using DeepClone.Model;
using Natasha;
using Natasha.Operator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DeepClone.Template
{
    public class FastCloneDictTemplate : ICloneTemplate
    {

        internal readonly static int HashCode;
        static FastCloneDictTemplate() => HashCode = typeof(FastCloneDictTemplate).GetHashCode();




        public override int GetHashCode() => HashCode;




        public bool MatchType(Type type)
        {

            return type.IsImplementFrom<IDictionary>();

        }




        public Delegate TypeRouter(NBuildInfo info)
        {

            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(@"if(old!=default){ return ");


            //初始化目标，区分实体类和接口
            if (!info.CurrentType.IsInterface)
            {

                scriptBuilder.AppendLine($"new {info.CurrentTypeName}");

            }
           scriptBuilder.AppendLine("(old.Select(item=>KeyValuePair.Create(");



            //克隆Key
            var keyType = info.CurrentType.GetGenericArguments()[0];
            if (keyType.IsSimpleType())
            {

                scriptBuilder.Append($"item.Key,");

            }
            else if (keyType == typeof(object))
            {

                scriptBuilder.AppendLine($"FastObjectCloneOperator.Clone(item.Key),");

            }
            else
            {

                scriptBuilder.AppendLine($"FastCloneOperator.Clone(item.Key),");

            }


            //克隆Value
            var valueType = info.CurrentType.GetGenericArguments()[1];
            if (valueType.IsSimpleType())
            {

                scriptBuilder.Append($"item.Value");

            }
            else if (keyType == typeof(object))
            {

                scriptBuilder.AppendLine($"FastObjectCloneOperator.Clone(item.Value),");

            }
            else
            {

                scriptBuilder.AppendLine($"FastCloneOperator.Clone(item.Value)");

            }


            //补全括号，返回默认值。
            scriptBuilder.AppendLine(")));}return default;");
            var actionBuilder = FastMethodOperator.New;

            var action = actionBuilder
                            .Using("DeepClone")
                            .Using("System.Linq")
                            .Using(typeof(IDictionary))
                            .Using(typeof(KeyValuePair<,>))
                            .Param(info.CurrentType, "old")
                            .MethodBody(scriptBuilder.ToString())
                            .Return(info.CurrentType)
                            .Complie();
            return action;

        }

    }

}
