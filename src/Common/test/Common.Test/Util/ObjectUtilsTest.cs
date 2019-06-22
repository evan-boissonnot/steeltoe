// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Steeltoe.Common.Util.Test
{
    public class ObjectUtilsTest
    {
        [Fact]
        public void NullSafeEqualsWithArrays()
        {
            Assert.True(ObjectUtils.NullSafeEquals(new string[] { "a", "b", "c" }, new string[] { "a", "b", "c" }));
            Assert.True(ObjectUtils.NullSafeEquals(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }));
        }

        [Fact]
        public void NullSafeHashCodeWithBooleanArray()
        {
            int expected = (31 * 7) + true.GetHashCode();
            expected = (31 * expected) + false.GetHashCode();

            bool[] array = { true, false };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithBooleanArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((bool[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithByteArray()
        {
            int expected = (31 * 7) + 8;
            expected = (31 * expected) + 10;

            byte[] array = { 8, 10 };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithByteArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((byte[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithCharArray()
        {
            int expected = (31 * 7) + 'a'.GetHashCode();
            expected = (31 * expected) + 'E'.GetHashCode();

            char[] array = { 'a', 'E' };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithCharArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((char[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithDoubleArray()
        {
            int expected = (31 * 7) + ((double)8449.65d).GetHashCode();
            expected = (31 * expected) + ((double)9944.923d).GetHashCode();

            double[] array = { 8449.65, 9944.923 };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithDoubleArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((double[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithFloatArray()
        {
            int expected = (31 * 7) + ((float)9.6f).GetHashCode();
            expected = (31 * expected) + ((float)7.4f).GetHashCode();

            float[] array = { 9.6f, 7.4f };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithFloatArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((float[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithIntArray()
        {
            int expected = (31 * 7) + 884;
            expected = (31 * expected) + 340;

            int[] array = { 884, 340 };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithIntArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((int[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithLongArray()
        {
            long lng = 7993L;
            int expected = (31 * 7) + (int)(lng ^ ((lng >> 32) & 0x0000FFFF));
            lng = 84320L;
            expected = (31 * expected) + (int)(lng ^ ((lng >> 32) & 0x0000FFFF));

            long[] array = { 7993L, 84320L };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithLongArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((long[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithobject()
        {
            string str = "Luke";
            Assert.Equal(str.GetHashCode(), ObjectUtils.NullSafeHashCode(str));
        }

        [Fact]
        public void NullSafeHashCodeWithobjectArray()
        {
            int expected = (31 * 7) + "Leia".GetHashCode();
            expected = (31 * expected) + "Han".GetHashCode();

            object[] array = { "Leia", "Han" };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((object[])null));
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingBooleanArray()
        {
            object array = new bool[] { true, false };
            int expected = ObjectUtils.NullSafeHashCode((bool[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingByteArray()
        {
            object array = new byte[] { 6, 39 };
            int expected = ObjectUtils.NullSafeHashCode((byte[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingCharArray()
        {
            object array = new char[] { 'l', 'M' };
            int expected = ObjectUtils.NullSafeHashCode((char[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingDoubleArray()
        {
            object array = new double[] { 68930.993, 9022.009 };
            int expected = ObjectUtils.NullSafeHashCode((double[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingFloatArray()
        {
            object array = new float[] { 9.9f, 9.54f };
            int expected = ObjectUtils.NullSafeHashCode((float[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingIntArray()
        {
            object array = new int[] { 89, 32 };
            int expected = ObjectUtils.NullSafeHashCode((int[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingLongArray()
        {
            object array = new long[] { 4389, 320 };
            int expected = ObjectUtils.NullSafeHashCode((long[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingobjectArray()
        {
            object array = new object[] { "Luke", "Anakin" };
            int expected = ObjectUtils.NullSafeHashCode((object[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectBeingShortArray()
        {
            object array = new short[] { 5, 3 };
            int expected = ObjectUtils.NullSafeHashCode((short[])array);
            AssertEqualHashCodes(expected, array);
        }

        [Fact]
        public void NullSafeHashCodeWithobjectEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((object)null));
        }

        [Fact]
        public void NullSafeHashCodeWithShortArray()
        {
            int expected = (31 * 7) + ((short)70).GetHashCode();
            expected = (31 * expected) + ((short)8).GetHashCode();

            short[] array = { 70, 8 };
            int actual = ObjectUtils.NullSafeHashCode(array);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullSafeHashCodeWithShortArrayEqualToNull()
        {
            Assert.Equal(0, ObjectUtils.NullSafeHashCode((short[])null));
        }

        private void AssertEqualHashCodes(int expected, object array)
        {
            int actual = ObjectUtils.NullSafeHashCode(array);
            Assert.Equal(expected, actual);
            Assert.True(array.GetHashCode() != actual);
        }
    }
}
