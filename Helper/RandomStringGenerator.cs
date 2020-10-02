using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Helper
{
    public class RandomStringGenerator
    {

        private const string UPPERCASE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LOWERCASE = "abcdefghijklmnopqrstuvwxyz";
        private const string NUMBERS = "0123456789";
        private const string SYMBOLS = "~`!@#$%^&*()-_=+<>?:/\\,.[]{}|'";
        private const string APISYMBOLS = "~`!@#$%^&*()-_=+<>?:,.[]{}|'";
        private Random r;

        public RandomStringGenerator()
        {
            this.r = new Random();
        }

        public RandomStringGenerator(int seed)
        {
            this.r = new Random(seed);
        }

        public virtual string NextString(int length)
        {
            return this.NextString(length, true, true, true, true);
        }

        public virtual string NextString(int length, bool lowerCase, bool upperCase, bool numbers, bool symbols)
        {
            char[] chArray = new char[length];
            string str = string.Empty;
            if (lowerCase)
                str += LOWERCASE;
            if (upperCase)
                str += UPPERCASE;
            if (numbers)
                str += NUMBERS;
            if (symbols)
                str += SYMBOLS;
            for (int index1 = 0; index1 < chArray.Length; ++index1)
            {
                int index2 = this.r.Next(0, str.Length);
                chArray[index1] = str[index2];
            }
            return new string(chArray);
        }

        public virtual string NextTokenString(int length, bool lowerCase, bool upperCase, bool numbers, bool symbols)
        {
            char[] chArray = new char[length];
            string str = string.Empty;
            if (lowerCase)
                str += LOWERCASE;
            if (upperCase)
                str += UPPERCASE;
            if (numbers)
                str += NUMBERS;
            if (symbols)
                str += SYMBOLS;
            for (int index1 = 0; index1 < chArray.Length; ++index1)
            {
                int index2 = this.r.Next(0, str.Length);
                chArray[index1] = str[index2];
            }
            return new string(chArray);
        }
    }

}
