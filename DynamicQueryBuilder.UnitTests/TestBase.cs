// <copyright file="TestBase.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using System.Linq.Expressions;

namespace DynamicQueryBuilder.UnitTests
{
    public abstract class TestBase
    {
        protected const string DYNAMIC_QUERY_STRING = "?o=Equals&p=Name&v=Value&s=Name,desc&offset=0&count=10";
        protected const string DYNAMIC_QUERY_STRING_PARAM = "dqb";
        protected string dynamicQueryWithParam = $"?{DYNAMIC_QUERY_STRING_PARAM}=o%3DEquals%26p%3Dcategory%26v%3DMovies";

        protected sealed class InnerMemberTestClass
        {
            public int Age { get; set; }
        }

        protected sealed class MemberTestClass
        {
            public InnerMemberTestClass InnerMember { get; set; }

            public string Name { get; set; }
        }

        protected ParameterExpression XParam
        {
            get
            {
                return Expression.Parameter(typeof(MemberTestClass), "x");
            }
        }

        public bool AreObjectPropertiesMatching(object object1, object object2)
        {
            if (object1 == null || object2 == null)
            {
                return false;
            }

            return JsonConvert.SerializeObject(object1) == JsonConvert.SerializeObject(object2);
        }
    }
}
