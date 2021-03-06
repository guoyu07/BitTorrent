﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Torrent.Client.Bencoding
{
    /// <summary>
    /// Bencoding的编码格式
    /// </summary>
    internal enum BencodedNodeType
    {
        String,
        Integer,
        List,
        Dictionary
    }

    /// <summary>
    /// Parses bencoded data.
    /// </summary>
    public static class BencodingParser
    {
        private static BinaryReader reader;
        private static Stream stream;

        /// <summary>
        /// 解析编码后的string转换成Bencoding对象
        /// </summary>
        /// <remarks>If byte data is contained the the string, refrain from using this method and use Decode(byte[]) instead.</remarks>
        /// <param name="bencoded">The bencoded string to parse.</param>
        /// <returns></returns>
        public static IBencodedElement Decode(string bencoded)
        {
            Contract.Requires(bencoded != null);
            return Decode(bencoded.Select(c => (byte) c).ToArray());
        }

        /// <summary>
        /// 把字节流转换成Bencoding对象
        /// </summary>
        /// <param name="bencoded">The bencoded bytestring to parse.</param>
        /// <returns>The <c>IBencodedElement</c> representing the top node of the returned tree.</returns>
        /// <exception cref="BencodingParserException"></exception>
        public static IBencodedElement Decode(byte[] bencoded)
        {
            Contract.Requires(bencoded != null);

            try
            {
                using (stream = new MemoryStream(bencoded))
                using (reader = new BinaryReader(stream))
                {
                    return ParseElement();
                }
            }
            catch (Exception e)
            {
                throw new BencodingParserException("Unable to parse stream.", e);
            }
        }

        /// <summary>
        /// 转换一个数据
        /// </summary>
        /// <returns></returns>
        private static IBencodedElement ParseElement()
        {
            switch (CurrentNodeType())
            {
                case BencodedNodeType.Integer:
                    return ParseInteger();
                case BencodedNodeType.String:
                    return ParseString();
                case BencodedNodeType.List:
                    return ParseList();
                case BencodedNodeType.Dictionary:
                    return ParseDictionary();
                default:
                    throw new BencodingParserException("Unrecognized node type.");
            }
        }

        /// <summary>
        /// 解析字典d:e
        /// </summary>
        /// <returns></returns>
        private static BencodedDictionary ParseDictionary()
        {
            char endChar = 'e';
            char beginChar = 'd';
            var list = new BencodedDictionary();
            if (reader.PeekChar() != beginChar) throw new BencodingParserException("Expected dictionary.");

            reader.Read();
            while ((char) reader.PeekChar() != endChar)
            {
                string key = ParseElement() as BencodedString;
                if (key == null) throw new BencodingParserException("Key is expected to be a string.");
                list.Add(key, ParseElement());
            }
            reader.Read();
            return list;
        }


        /// <summary>
        /// 解析list l:e
        /// </summary>
        /// <returns></returns>
        private static BencodedList ParseList()
        {
            char endChar = 'e';
            char beginChar = 'l';
            var list = new BencodedList();
            if (reader.PeekChar() != beginChar) throw new BencodingParserException("Expected list.");

            reader.Read();
            while ((char) reader.PeekChar() != endChar)
            {
                list.Add(ParseElement());
            }
            reader.Read();
            return list;
        }

        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <returns></returns>
        private static BencodedString ParseString()
        {
            char lenEndChar = ':';
            if (!char.IsDigit((char) reader.PeekChar()))
                throw new BencodingParserException("Expected to read string length.");
            long length = ReadIntegerValue(lenEndChar);
            if (length < 0) string.Format("String can not have a negative length of {0}.", length);
            int len;
            var byteResult = new byte[length];
            if ((len = reader.Read(byteResult, 0, (int) length)) != length)
                throw new BencodingParserException(string.Format("Did not read the expected amount of {0} bytes, {1} instead.", length, len));
        
            return new BencodedString(new string(byteResult.Select(b => (char)b).ToArray()));
          
        }

        /// <summary>
        /// 解析整数
        /// </summary>
        /// <returns></returns>
        private static BencodedInteger ParseInteger()
        {
            char endChar = 'e';
            char beginChar = 'i';
            if (reader.PeekChar() != beginChar) throw new BencodingParserException("Expected integer.");
            reader.Read();
            long result = ReadIntegerValue(endChar);
            return result;
        }

        /// <summary>
        /// 读取整数
        /// </summary>
        /// <param name="endChar"></param>
        /// <returns></returns>
        private static long ReadIntegerValue(char endChar)
        {
            char c;
            long result = 0;
            int negative = 1;
            if ((char) reader.PeekChar() == '-')
            {
                reader.Read();
                negative = -1;
            }
            while ((c = (char) reader.Read()) != endChar)
            {
                if (!char.IsDigit(c))
                    throw new BencodingParserException(string.Format("Expected a digit, got '{0}'.", c));
                result *= 10;
                result += ((long) char.GetNumericValue(c));
            }
            return result*negative;
        }

        /// <summary>
        /// 根据当前数据位置的字符串判断当前数据类型
        /// </summary>
        /// <returns></returns>
        private static BencodedNodeType CurrentNodeType()
        {
            char c;
            switch (c = (char) reader.PeekChar())
            {
                case 'l':
                    return BencodedNodeType.List;
                case 'd':
                    return BencodedNodeType.Dictionary;
                case 'i':
                    return BencodedNodeType.Integer;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return BencodedNodeType.String;
                default:
                    throw new BencodingParserException(string.Format("Node type not recognized: '{0}'.", c));
            }
        }
    }
}