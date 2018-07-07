// <copyright file="ExtractMemberTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using System;
using System.Linq.Expressions;
using Xunit;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.UnitTests.ExpressionBuilderTests
{
    public class ExtractMemberTests : TestBase
    {
        private const string REFLECTED_VALUE_OF_INNER_OBJECT = "x.InnerMember.Age";
        private const string REFLECTED_VALUE_OF_MAIN_OBJECT = "x.Name";

        [Fact]
        public void ExtractMemberShouldThrowInvalidDynamicQueryExceptionWhenArgumentsNullOrEmpty()
        {
            Assert.Throws<DynamicQueryException>(() =>
            {
                ExpressionBuilder.ExtractMember(null, string.Empty);
            });

            Assert.Throws<DynamicQueryException>(() =>
            {
                ExpressionBuilder.ExtractMember(null, "notempty");
            });

            Assert.Throws<DynamicQueryException>(() =>
            {
                ExpressionBuilder.ExtractMember(Expression.Parameter(typeof(MemberTestClass), "x"), null);
            });
        }

        [Fact]
        public void ExtractMemberShouldHandleInnerObjects()
        {
            Assert.Equal(REFLECTED_VALUE_OF_INNER_OBJECT, ExpressionBuilder.ExtractMember(XParam, "InnerMember.Age").ToString());
        }

        [Fact]
        public void ExtractMemberShouldHandleObjects()
        {
            Assert.Equal(REFLECTED_VALUE_OF_MAIN_OBJECT, ExpressionBuilder.ExtractMember(XParam, "Name").ToString());
        }

        [Fact]
        public void ExtractMemberShouldThrowArgumentExceptionWhenInvalidParameter()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ExpressionBuilder.ExtractMember(XParam, "Name.Age");
            });
        }

        [Fact]
        public void ExtractMemberShouldHandleNullableTypes()
        {
            const string reflectedValue = "x.NullableMember.Value";
            Assert.Equal(reflectedValue, ExpressionBuilder.ExtractMember(XParam, "NullableMember").ToString());
        }
    }
}
