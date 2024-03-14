using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using XDevkit;

namespace XboxCore.Xbox.Connection {
    public static class JRPC {
        private static readonly uint Byte = 4;
        private static readonly uint ByteArray = 7;
        private static uint connectionId;
        private static readonly uint Float = 3;
        private static readonly uint FloatArray = 6;
        private static readonly uint Int = 1;
        private static readonly uint IntArray = 5;
        public static readonly uint JRPCVersion = 2;
        private static readonly uint String = 2;
        private static Dictionary<Type, int> StructPrimitiveSizeMap;
        private static readonly uint Uint64 = 8;
        private static readonly uint Uint64Array = 9;
        private static HashSet<Type> ValidReturnTypes;
        private static Dictionary<Type, int> ValueTypeSizeMap;
        private static readonly uint Void = 0;
        private static bool Connected = false;

        static JRPC() {
            Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
            dictionary.Add(typeof(bool), 4);
            dictionary.Add(typeof(byte), 1);
            dictionary.Add(typeof(short), 2);
            dictionary.Add(typeof(int), 4);
            dictionary.Add(typeof(long), 8);
            dictionary.Add(typeof(ushort), 2);
            dictionary.Add(typeof(uint), 4);
            dictionary.Add(typeof(ulong), 8);
            dictionary.Add(typeof(float), 4);
            dictionary.Add(typeof(double), 8);
            ValueTypeSizeMap = dictionary;
            StructPrimitiveSizeMap = new Dictionary<Type, int>();
            ValidReturnTypes = new HashSet<Type> { 
                typeof(void), typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(ushort), typeof(uint), typeof(ulong), typeof(float), typeof(double), typeof(string), typeof(bool[]), typeof(byte[]), typeof(short[]), typeof(int[]), 
                typeof(long[]), typeof(ushort[]), typeof(uint[]), typeof(ulong[]), typeof(float[]), typeof(double[]), typeof(string[])
             };
        }

        private static T[] ArrayReturn<T>(this IXboxConsole console, uint Address, uint Size) {
            if (Size == 0) {
                return new T[1];
            }

            Type type = typeof(T);
            object obj2 = new object();
            if (type == typeof(short)) {
                obj2 = console.ReadInt16(Address, Size);
            }

            if (type == typeof(ushort)) {
                obj2 = console.ReadUInt16(Address, Size);
            }

            if (type == typeof(int)) {
                obj2 = console.ReadInt32(Address, Size);
            }

            if (type == typeof(uint)) {
                obj2 = console.ReadInt32(Address, Size);
            }

            if (type == typeof(long)) {
                obj2 = console.ReadInt64(Address, Size);
            }

            if (type == typeof(ulong)) {
                obj2 = console.ReadUInt64(Address, Size);
            }

            if (type == typeof(float)) {
                obj2 = console.ReadFloat(Address, Size);
            }

            if (type == typeof(byte)) {
                obj2 = console.GetMemory(Address, Size);
            }
            return (T[]) obj2;
        }

        public static T Call<T>(this IXboxConsole console, uint Address, params object[] Arguments) where T: struct {
            return (T)CallArgs(console, true, TypeToType<T>(false), typeof(T), null, 0, Address, 0, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments) where T: struct {
            return (T) CallArgs(console, Type == ThreadType.System, TypeToType<T>(false), typeof(T), null, 0, Address, 0, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, string module, int ordinal, params object[] Arguments) where T: struct {
             return (T) CallArgs(console, true, TypeToType<T>(false), typeof(T), module, ordinal, 0, 0, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments) where T: struct {
            return (T) CallArgs(console, Type == ThreadType.System, TypeToType<T>(false), typeof(T), module, ordinal, 0, 0, Arguments);
        }

        private static object CallArgs(IXboxConsole console, bool SystemThread, uint Type, Type t, string module, int ordinal, uint Address, uint ArraySize, params object[] Arguments) {
            if (Connected) {
                if (!IsValidReturnType(t)) {
                    throw new Exception(string.Concat(new object[] { "Invalid type ", t.Name, Environment.NewLine, "JRPC only supports: bool, byte, short, int, long, ushort, uint, ulong, float, double" }));
                }

                console.ConnectTimeout = console.ConversationTimeout = 0x3d0900;
                string command = string.Concat(new object[] { "consolefeatures ver=", JRPCVersion, " type=", Type, SystemThread ? " system" : "", (module != null) ? string.Concat(new object[] { " module=\"", module, "\" ord=", ordinal }) : "", " as=", ArraySize, " params=\"A\\", Address.ToString("X"), @"\A\", Arguments.Length, @"\" });
                if (Arguments.Length > 0x25) {
                    throw new Exception("Can not use more than 37 paramaters in a call");
                }

                foreach (object obj2 in Arguments) {
                    object obj3;
                    string str14;
                    bool flag = false;
                    if (obj2 is uint) {
                        obj3 = command;
                        command = string.Concat(new object[] { obj3, Int, @"\", UIntToInt((uint)obj2), @"\" });
                        flag = true;
                    }

                    if (((obj2 is int) || (obj2 is bool)) || (obj2 is byte)) {
                        if (obj2 is bool) {
                            object obj4 = command;
                            command = string.Concat(new object[] { obj4, Int, "/", Convert.ToInt32((bool)obj2), @"\" });
                        } else {
                            object obj5 = command;
                            command = string.Concat(new object[] { obj5, Int, @"\", (obj2 is byte) ? Convert.ToByte(obj2).ToString() : Convert.ToInt32(obj2).ToString(), @"\" });
                        }
                        flag = true;
                    } else if ((obj2 is int[]) || (obj2 is uint[])) {
                        byte[] buffer = IntArrayToByte((int[])obj2);
                        object obj6 = command;
                        command = string.Concat(new object[] { obj6, ByteArray.ToString(), "/", buffer.Length, @"\" });
                        for (int i = 0; i < buffer.Length; i++) {
                            command = command + buffer[i].ToString("X2");
                        }
                        command = command + @"\";
                        flag = true;
                    } else if (obj2 is string) {
                        string str3 = (string)obj2;
                        object obj7 = command;
                        command = string.Concat(new object[] { obj7, ByteArray.ToString(), "/", str3.Length, @"\", ((string)obj2).ToHexString(), @"\" });
                        flag = true;
                    } else if (obj2 is double) {
                        double num2 = (double)obj2;
                        str14 = command;
                        command = str14 + Float.ToString() + @"\" + num2.ToString() + @"\";
                        flag = true;
                    } else if (obj2 is float) {
                        float num3 = (float)obj2;
                        str14 = command;
                        command = str14 + Float.ToString() + @"\" + num3.ToString() + @"\";
                        flag = true;
                    } else if (obj2 is float[]) {
                        float[] numArray = (float[])obj2;
                        str14 = command;
                        string[] strArray = new string[] { str14, ByteArray.ToString(), "/", (numArray.Length * 4).ToString(), @"\" };
                        command = string.Concat(strArray);
                        for (int j = 0; j < numArray.Length; j++) {
                            byte[] bytes = BitConverter.GetBytes(numArray[j]);
                            Array.Reverse(bytes);
                            for (int k = 0; k < 4; k++) {
                                command = command + bytes[k].ToString("X2");
                            }
                        }
                        command = command + @"\";
                        flag = true;
                    } else if (obj2 is byte[]) {
                        byte[] buffer3 = (byte[])obj2;
                        obj3 = command;
                        command = string.Concat(new object[] { obj3, ByteArray.ToString(), "/", buffer3.Length, @"\" });
                        for (int m = 0; m < buffer3.Length; m++) {
                            command = command + buffer3[m].ToString("X2");
                        }
                        command = command + @"\";
                        flag = true;
                    }

                    if (!flag) {
                        str14 = command;
                        command = str14 + Uint64.ToString() + @"\" + ConvertToUInt64(obj2).ToString() + @"\";
                    }
                }
                command = command + "\"";
                string str2 = SendCommand(console, command);
                string str4 = "buf_addr=";
                while (str2.Contains(str4)) {
                    Thread.Sleep(250);
                    str2 = SendCommand(console, "consolefeatures " + str4 + "0x" + uint.Parse(str2.Substring(str2.find(str4) + str4.Length), NumberStyles.HexNumber).ToString("X"));
                }
                console.ConversationTimeout = 0x7d0;
                console.ConnectTimeout = 0x1388;
                switch (Type) {
                    case 1:
                        {
                            uint num8 = uint.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                            if (t != typeof(uint)) {
                                if (t == typeof(int)) {
                                    return UIntToInt(num8);
                                }

                                if (t == typeof(short)) {
                                    return short.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                                }

                                if (t != typeof(ushort)) {
                                    break;
                                }
                                return ushort.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                            }
                            return num8;
                        }
                    case 2:
                        {
                            string str5 = str2.Substring(str2.find(" ") + 1);
                            if (t != typeof(string)) {
                                if (t != typeof(char[])) {
                                    break;
                                }
                                return str5.ToCharArray();
                            }
                            return str5;
                        }
                    case 3:
                        if (t != typeof(double)) {
                            if (t != typeof(float)) {
                                break;
                            }
                            return float.Parse(str2.Substring(str2.find(" ") + 1));
                        }
                        return double.Parse(str2.Substring(str2.find(" ") + 1));

                    case 4:
                        {
                            byte num9 = byte.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                            if (t != typeof(byte)) {
                                if (t != typeof(char)) {
                                    break;
                                }
                                return (char)num9;
                            }
                            return num9;
                        }
                    case 8:
                        if (t != typeof(long))
                        {
                            if (t == typeof(ulong)) {
                                return ulong.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                            }
                            break;
                        }
                        return long.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);

                    case 5:
                        {
                            string str6 = str2.Substring(str2.find(" ") + 1);
                            int index = 0;
                            string s = "";
                            uint[] numArray2 = new uint[8];
                            foreach (char ch in str6) {
                                if ((ch != ',') && (ch != ';')) {
                                    s = s + ch.ToString();
                                } else {
                                    numArray2[index] = uint.Parse(s, NumberStyles.HexNumber);
                                    index++;
                                    s = "";
                                }

                                if (ch == ';') {
                                    return numArray2;
                                }
                            }
                            return numArray2;
                        }
                    case 6:
                        {
                            string str8 = str2.Substring(str2.find(" ") + 1);
                            int num11 = 0;
                            string str9 = "";
                            float[] numArray3 = new float[ArraySize];
                            foreach (char ch2 in str8) {
                                if ((ch2 != ',') && (ch2 != ';')) {
                                    str9 = str9 + ch2.ToString();
                                } else {
                                    numArray3[num11] = float.Parse(str9);
                                    num11++;
                                    str9 = "";
                                }

                                if (ch2 == ';') {
                                    return numArray3;
                                }
                            }
                            return numArray3;
                        }
                    case 7:
                        {
                            string str10 = str2.Substring(str2.find(" ") + 1);
                            int num12 = 0;
                            string str11 = "";
                            byte[] buffer4 = new byte[ArraySize];
                            foreach (char ch3 in str10) {
                                if ((ch3 != ',') && (ch3 != ';')) {
                                    str11 = str11 + ch3.ToString();
                                } else {
                                    buffer4[num12] = byte.Parse(str11);
                                    num12++;
                                    str11 = "";
                                }

                                if (ch3 == ';') {
                                    return buffer4;
                                }
                            }
                            return buffer4;
                        }
                }

                if (Type == Uint64Array) {
                    string str12 = str2.Substring(str2.find(" ") + 1);
                    int num13 = 0;
                    string str13 = "";
                    ulong[] numArray4 = new ulong[ArraySize];
                    foreach (char ch4 in str12) {
                        if ((ch4 != ',') && (ch4 != ';')) {
                            str13 = str13 + ch4.ToString();
                        } else {
                            numArray4[num13] = ulong.Parse(str13);
                            num13++;
                            str13 = "";
                        }

                        if (ch4 == ';') {
                            break;
                        }
                    }

                    if (t == typeof(ulong)) {
                        return numArray4;
                    }

                    if (t == typeof(long)) {
                        long[] numArray5 = new long[ArraySize];
                        for (int n = 0; n < ArraySize; n++) {
                            numArray5[n] = BitConverter.ToInt64(BitConverter.GetBytes(numArray4[n]), 0);
                        }
                        return numArray5;
                    }
                }

                if (Type == Void) {
                    return 0;
                }
                return ulong.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
            }
            return null;
        }

        public static T[] CallArray<T>(this IXboxConsole console, uint Address, uint ArraySize, params object[] Arguments) where T: struct {
            if (ArraySize == 0) {
                return new T[1];
            }
            return (T[]) CallArgs(console, true, TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, ThreadType Type, uint Address, uint ArraySize, params object[] Arguments) where T: struct {
            if (ArraySize == 0) {
                return new T[1];
            }
            return (T[]) CallArgs(console, Type == ThreadType.System, TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, string module, int ordinal, uint ArraySize, params object[] Arguments) where T: struct {
            if (ArraySize == 0) {
                return new T[1];
            }
            return (T[]) CallArgs(console, true, TypeToType<T>(true), typeof(T), module, ordinal, 0, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, ThreadType Type, string module, int ordinal, uint ArraySize, params object[] Arguments) where T: struct {
            if (ArraySize == 0) {
                return new T[1];
            }
            return (T[]) CallArgs(console, Type == ThreadType.System, TypeToType<T>(true), typeof(T), module, ordinal, 0, ArraySize, Arguments);
        }

        public static string CallString(this IXboxConsole console, uint Address, params object[] Arguments) {
            return (string) CallArgs(console, true, String, typeof(string), null, 0, Address, 0, Arguments);
        }

        public static string CallString(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments) {
            return (string) CallArgs(console, Type == ThreadType.System, String, typeof(string), null, 0, Address, 0, Arguments);
        }

        public static string CallString(this IXboxConsole console, string module, int ordinal, params object[] Arguments) {
            return (string) CallArgs(console, true, String, typeof(string), module, ordinal, 0, 0, Arguments);
        }

        public static string CallString(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments) {
            return (string) CallArgs(console, Type == ThreadType.System, String, typeof(string), module, ordinal, 0, 0, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, uint Address, params object[] Arguments) {
            CallArgs(console, true, Void, typeof(void), null, 0, Address, 0, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments) {
            CallArgs(console, Type == ThreadType.System, Void, typeof(void), null, 0, Address, 0, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, string module, int ordinal, params object[] Arguments) {
            CallArgs(console, true, Void, typeof(void), module, ordinal, 0, 0, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments) {
            CallArgs(console, Type == ThreadType.System, Void, typeof(void), module, ordinal, 0, 0, Arguments);
        }

        public static bool Connect(this IXboxConsole console, out IXboxConsole Console, string XboxNameOrIP = "default") {
            if (XboxNameOrIP == "default") {
                XboxNameOrIP = new XboxManager().DefaultConsole.ToString();
            }
            IXboxConsole console2 = new XboxManager().OpenConsole(XboxNameOrIP);
            int num = 0;
            bool flag = false;
            while (!flag) {
                try {
                    connectionId = console2.OpenConnection(null);
                    flag = true;
                    continue;
                } catch (COMException exception) {
                    if (exception.ErrorCode == UIntToInt(0x82da0100)) {
                        if (num >= 3) {
                            Console = console2;
                            Connected = false;
                            return false;
                        }
                        num++;
                        Thread.Sleep(100);
                        continue;
                    }
                    Console = console2;
                    Connected = false;
                    return false;
                }
            }
            Console = console2;
            Connected = true;
            return true;
        }

        public static string ConsoleType(this IXboxConsole console) {
            string command = "consolefeatures ver=" + JRPCVersion + " type=17 params=\"A\\0\\A\\0\\\"";
            string str2 = SendCommand(console, command);
            return str2.Substring(str2.find(" ") + 1);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value) {
            constantMemorySetting(console, Address, Value, false, 0, false, 0);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint TitleID) {
            constantMemorySetting(console, Address, Value, false, 0, true, TitleID);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint IfValue, uint TitleID) {
           constantMemorySetting(console, Address, Value, true, IfValue, true, TitleID);
        }

        public static void constantMemorySetting(IXboxConsole console, uint Address, uint Value, bool useIfValue, uint IfValue, bool usetitleID, uint TitleID) {
            string command = string.Concat(new object[] { 
                "consolefeatures ver=", JRPCVersion, " type=18 params=\"A\\", Address.ToString("X"), @"\A\5\", Int, @"\", UIntToInt(Value), @"\", Int, @"\", useIfValue ? 1 : 0, @"\", Int, @"\", IfValue, 
                @"\", Int, @"\", usetitleID ? 1 : 0, @"\", Int, @"\", UIntToInt(TitleID), "\\\""
             });
            if (Connected)
                SendCommand(console, command);
        }

        internal static ulong ConvertToUInt64(object o) {
            if (o is bool) {
                if ((bool) o) {
                    return 1L;
                }
                return 0L;
            }

            if (o is byte) {
                return (ulong) ((byte) o);
            }

            if (o is short) {
                return (ulong) ((short) o);
            }

            if (o is int) {
                return (ulong) ((int) o);
            }

            if (o is long) {
                return (ulong) ((long) o);
            }

            if (o is ushort) {
                return (ulong) ((ushort) o);
            }

            if (o is uint) {
                return (ulong) ((uint) o);
            }

            if (o is ulong) {
                return (ulong) o;
            }

            if (o is float) {
                return (ulong) BitConverter.DoubleToInt64Bits((double) ((float) o));
            }

            if (o is double) {
                return (ulong) BitConverter.DoubleToInt64Bits((double) o);
            }
            return 0L;
        }

        public static int find(this string String, string _Ptr) {
            if ((_Ptr.Length != 0) && (String.Length != 0)) {
                for (int i = 0; i < String.Length; i++) {
                    if (String[i] == _Ptr[0]) {
                        bool flag = true;
                        for (int j = 0; j < _Ptr.Length; j++) {
                            if (String[i + j] != _Ptr[j]) {
                                flag = false;
                            }
                        }

                        if (flag) {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public static string GetCPUKey(this IXboxConsole console) {
            if (Connected) {
                string command = "consolefeatures ver=" + JRPCVersion + " type=10 params=\"A\\0\\A\\0\\\"";
                string str2 = SendCommand(console, command);
                return str2.Substring(str2.find(" ") + 1);
            }
            return "";
        }

        public static uint GetKernalVersion(this IXboxConsole console) {
            if (Connected) {
                string command = "consolefeatures ver=" + JRPCVersion + " type=13 params=\"A\\0\\A\\0\\\"";
                string str2 = SendCommand(console, command);
                return uint.Parse(str2.Substring(str2.find(" ") + 1));
            }
            return 0;
        }

        public static byte[] GetMemory(this IXboxConsole console, uint Address, uint Length) {
            uint bytesRead = 0;
            byte[] data = new byte[Length];
            if (Connected) {
                console.DebugTarget.GetMemory(Address, Length, data, out bytesRead);
                console.DebugTarget.InvalidateMemoryCache(true, Address, Length);
            }
            return data;
        }

        public static uint GetTemperature(this IXboxConsole console, TemperatureType TemperatureType) {
            if (Connected) {
                string command = string.Concat(new object[] { "consolefeatures ver=", JRPCVersion, " type=15 params=\"A\\0\\A\\1\\", Int, @"\", (int)TemperatureType, "\\\"" });
                string str2 = SendCommand(console, command);
                return uint.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
            }
            return 0;
        }

        private static byte[] IntArrayToByte(int[] iArray) {
            byte[] buffer = new byte[iArray.Length * 4];
            int index = 0;
            for (int i = 0; index < iArray.Length; i += 4) {
                for (int j = 0; j < 4; j++) {
                    buffer[i + j] = BitConverter.GetBytes(iArray[index])[j];
                }
                index++;
            }
            return buffer;
        }

        internal static bool IsValidReturnType(Type t) {
            return ValidReturnTypes.Contains(t);
        }

        internal static bool IsValidStructType(Type t) {
            return (!t.IsPrimitive && t.IsValueType);
        }

        public static void Push(this byte[] InArray, out byte[] OutArray, byte Value) {
            OutArray = new byte[InArray.Length + 1];
            InArray.CopyTo(OutArray, 0);
            OutArray[InArray.Length] = Value;
        }

        public static bool ReadBool(this IXboxConsole console, uint Address) {
            if (Connected)
                return (console.GetMemory(Address, 1)[0] != 0);
            else
                return false;
        }

        public static byte ReadByte(this IXboxConsole console, uint Address) {
            if (Connected)
                return console.GetMemory(Address, 1)[0];
            else
                return 0;
        }

        public static float ReadFloat(this IXboxConsole console, uint Address) {
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, 4);
                ReverseBytes(buffer, 4);
                return BitConverter.ToSingle(buffer, 0);
            }
            return 0F;
        }

        public static float[] ReadFloat(this IXboxConsole console, uint Address, uint ArraySize) {
            float[] numArray = new float[ArraySize];
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, ArraySize * 4);
                ReverseBytes(buffer, 4);
                for (int i = 0; i < ArraySize; i++) {
                    numArray[i] = BitConverter.ToSingle(buffer, i * 4);
                }
            }
            return numArray;
        }

        public static short ReadInt16(this IXboxConsole console, uint Address) {
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, 2);
                ReverseBytes(buffer, 2);
                return BitConverter.ToInt16(buffer, 0);
            }
            return 0;
        }

        public static short[] ReadInt16(this IXboxConsole console, uint Address, uint ArraySize) {
            short[] numArray = new short[ArraySize];
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, ArraySize * 2);
                ReverseBytes(buffer, 2);
                for (int i = 0; i < ArraySize; i++) {
                    numArray[i] = BitConverter.ToInt16(buffer, i * 2);
                }
            }
            return numArray;
        }

        public static int ReadInt32(this IXboxConsole console, uint Address) {
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, 4);
                ReverseBytes(buffer, 4);
                return BitConverter.ToInt32(buffer, 0);
            }
            return 0;
        }

        public static int[] ReadInt32(this IXboxConsole console, uint Address, uint ArraySize) {
            int[] numArray = new int[ArraySize];
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, ArraySize * 4);
                ReverseBytes(buffer, 4);
                for (int i = 0; i < ArraySize; i++) {
                    numArray[i] = BitConverter.ToInt32(buffer, i * 4);
                }
            }
            return numArray;
        }

        public static long ReadInt64(this IXboxConsole console, uint Address) {
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, 8);
                ReverseBytes(buffer, 8);
                return BitConverter.ToInt64(buffer, 0);
            }
            return 0;
        }

        public static long[] ReadInt64(this IXboxConsole console, uint Address, uint ArraySize) {
            long[] numArray = new long[ArraySize];
            byte[] buffer = console.GetMemory(Address, ArraySize * 8);
            ReverseBytes(buffer, 8);
            for (int i = 0; i < ArraySize; i++) {
                numArray[i] = BitConverter.ToUInt32(buffer, i * 8);
            }
            return numArray;
        }

        public static sbyte ReadSByte(this IXboxConsole console, uint Address) {
            if (Connected)
                return (sbyte)console.GetMemory(Address, 1)[0];
            else
                return 0;
        }

        public static string ReadString(this IXboxConsole console, uint Address, uint size) {
            if (Connected)
                return Encoding.UTF8.GetString(console.GetMemory(Address, size));
            else
                return "";
        }

        public static ushort ReadUInt16(this IXboxConsole console, uint Address) {
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, 2);
                ReverseBytes(buffer, 2);
                return BitConverter.ToUInt16(buffer, 0);
            }
            return 0;
        }

        public static ushort[] ReadUInt16(this IXboxConsole console, uint Address, uint ArraySize) {
            ushort[] numArray = new ushort[ArraySize];
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, ArraySize * 2);
                ReverseBytes(buffer, 2);
                for (int i = 0; i < ArraySize; i++) {
                    numArray[i] = BitConverter.ToUInt16(buffer, i * 2);
                }
            }
            return numArray;
        }

        public static uint ReadUInt32(this IXboxConsole console, uint Address) {
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, 4);
                ReverseBytes(buffer, 4);
                return BitConverter.ToUInt32(buffer, 0);
            }
            return 0;
        }

        public static uint[] ReadUInt32(this IXboxConsole console, uint Address, uint ArraySize) {
            uint[] numArray = new uint[ArraySize];
            if (Connected) {
                byte[] buffer = console.GetMemory(Address, ArraySize * 4);
                ReverseBytes(buffer, 4);
                for (int i = 0; i < ArraySize; i++) {
                    numArray[i] = BitConverter.ToUInt32(buffer, i * 4);
                }
            }
            return numArray;
        }

        public static ulong ReadUInt64(this IXboxConsole console, uint Address) {
            byte[] buffer = console.GetMemory(Address, 8);
            ReverseBytes(buffer, 8);
            if (Connected)
                return BitConverter.ToUInt64(buffer, 0);
            else
                return 0;
        }

        public static ulong[] ReadUInt64(this IXboxConsole console, uint Address, uint ArraySize) {
            ulong[] numArray = new ulong[ArraySize];
            byte[] buffer = console.GetMemory(Address, ArraySize * 8);
            ReverseBytes(buffer, 8);
            for (int i = 0; i < ArraySize; i++) {
                numArray[i] = BitConverter.ToUInt32(buffer, i * 8);
            }
            return numArray;
        }

        public static uint ResolveFunction(this IXboxConsole console, string ModuleName, uint Ordinal) {
            string command = string.Concat(new object[] { "consolefeatures ver=", JRPCVersion, " type=9 params=\"A\\0\\A\\2\\", String, "/", ModuleName.Length, @"\", ModuleName.ToHexString(), @"\", Int, @"\", Ordinal, "\\\"" });
            string str2 = SendCommand(console, command);
            if (Connected)
                return uint.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
            else
                return 0;
        }

        private static void ReverseBytes(byte[] buffer, int groupSize) {
            if ((buffer.Length % groupSize) != 0) {
                throw new ArgumentException("Group size must be a multiple of the buffer length", "groupSize");
            }

            for (int i = 0; i < buffer.Length; i += groupSize) {
                int index = i;
                for (int j = (i + groupSize) - 1; index < j; j--) {
                    byte num4 = buffer[index];
                    buffer[index] = buffer[j];
                    buffer[j] = num4;
                    index++;
                }
            }
        }

        private static string SendCommand(IXboxConsole console, string Command) {
            if (Connected) {
                string str;
                uint connectionId = JRPC.connectionId;
                try {
                    console.SendTextCommand(JRPC.connectionId, Command, out str);
                    if (str.Contains("error=")) {
                        throw new Exception(str.Substring(11));
                    }

                    if (str.Contains("DEBUG")) {
                        throw new Exception("JRPC is not installed on the current console");
                    }
                } catch (COMException exception) {
                    if (exception.ErrorCode == UIntToInt(0x82da0007)) {
                        throw new Exception("JRPC is not installed on the current console");
                    }
                    throw exception;
                }
                return str;
            }
            return "";
        }

        public static void SetLeds(this IXboxConsole console, LEDState Top_Left, LEDState Top_Right, LEDState Bottom_Left, LEDState Bottom_Right) {
            string command = string.Concat(new object[] { 
                "consolefeatures ver=", JRPCVersion, " type=14 params=\"A\\0\\A\\4\\", Int, @"\", (uint) Top_Left, @"\", Int, @"\", (uint) Top_Right, @"\", Int, @"\", (uint) Bottom_Left, @"\", Int, 
                @"\", (uint) Bottom_Right, "\\\""
             });
            SendCommand(console, command);
        }

        public static void SetMemory(this IXboxConsole console, uint Address, byte[] Data) {
            uint num;
            if (Connected)
                console.DebugTarget.SetMemory(Address, (uint) Data.Length, Data, out num);
        }

        public static void ShutDownConsole(this IXboxConsole console) {
            try {
                string command = "consolefeatures ver=" + JRPCVersion + " type=11 params=\"A\\0\\A\\0\\\"";
                SendCommand(console, command);
            } catch { }
        }

        public static byte[] ToByteArray(this string String) {
            byte[] buffer = new byte[String.Length + 1];
            for (int i = 0; i < String.Length; i++) {
                buffer[i] = (byte) String[i];
            }
            return buffer;
        }

        public static string ToHexString(this string String) {
            string str = "";
            string str2 = String;
            for (int i = 0; i < str2.Length; i++) {
                str = str + ((byte) str2[i]).ToString("X2");
            }
            return str;
        }

        public static byte[] ToWCHAR(this string String) {
            return WCHAR(String);
        }

        private static uint TypeToType<T>(bool Array) where T: struct {
            Type type = typeof(T);
            if (((type == typeof(int)) || (type == typeof(uint))) || ((type == typeof(short)) || (type == typeof(ushort)))) {
                if (Array) {
                    return IntArray;
                }
                return Int;
            }

            if ((type == typeof(string)) || (type == typeof(char[]))) {
                return String;
            }

            if ((type == typeof(float)) || (type == typeof(double))) {
                if (Array) {
                    return FloatArray;
                }
                return Float;
            }

            if ((type == typeof(byte)) || (type == typeof(char))) {
                if (Array) {
                    return ByteArray;
                }
                return Byte;
            }

            if (((type == typeof(ulong)) || (type == typeof(long))) && Array) {
                return Uint64Array;
            }
            return Uint64;
        }

        private static int UIntToInt(uint Value) {
            return BitConverter.ToInt32(BitConverter.GetBytes(Value), 0);
        }

        public static byte[] WCHAR(string String) {
            byte[] buffer = new byte[(String.Length * 2) + 2];
            int index = 1;
            string str = String;
            for (int i = 0; i < str.Length; i++) {
                buffer[index] = (byte) str[i];
                index += 2;
            }
            return buffer;
        }

        public static void WriteBool(this IXboxConsole console, uint Address, bool Value) {
            if (Connected)
                console.SetMemory(Address, new byte[] { Value ? ((byte) 1) : ((byte) 0) });
        }

        public static void WriteBool(this IXboxConsole console, uint Address, bool[] Value) {
            byte[] inArray = new byte[0];
            for (int i = 0; i < Value.Length; i++) {
                inArray.Push(out inArray, Value[i] ? ((byte) 1) : ((byte) 0));
            }
            console.SetMemory(Address, inArray);
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte Value) {
            if (Connected)
                console.SetMemory(Address, new byte[] { Value });
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte[] Value) {
            if (Connected)
                console.SetMemory(Address, Value);
        }

        public static void WriteFloat(this IXboxConsole console, uint Address, float Value) {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse(bytes);
            if (Connected)
                console.SetMemory(Address, bytes);
        }

        public static void WriteFloat(this IXboxConsole console, uint Address, float[] Value) {
            byte[] array = new byte[Value.Length * 4];
            for (int i = 0; i < Value.Length; i++) {
                BitConverter.GetBytes(Value[i]).CopyTo(array, (int) (i * 4));
            }
            ReverseBytes(array, 4);
            if (Connected)
                console.SetMemory(Address, array);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short Value) {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 2);
            if (Connected)
                console.SetMemory(Address, bytes);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short[] Value) {
            byte[] array = new byte[Value.Length * 2];
            for (int i = 0; i < Value.Length; i++) {
                BitConverter.GetBytes(Value[i]).CopyTo(array, (int) (i * 2));
            }
            ReverseBytes(array, 2);
            if (Connected)
                console.SetMemory(Address, array);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int Value) {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 4);
            if (Connected)
                console.SetMemory(Address, bytes);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int[] Value) {
            byte[] array = new byte[Value.Length * 4];
            for (int i = 0; i < Value.Length; i++) {
                BitConverter.GetBytes(Value[i]).CopyTo(array, (int) (i * 4));
            }
            ReverseBytes(array, 4);
            if (Connected)
                console.SetMemory(Address, array);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long Value) {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 8);
            if (Connected)
                console.SetMemory(Address, bytes);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long[] Value) {
            byte[] array = new byte[Value.Length * 8];
            for (int i = 0; i < Value.Length; i++) {
                BitConverter.GetBytes(Value[i]).CopyTo(array, (int) (i * 8));
            }
            ReverseBytes(array, 8);
            if (Connected)
                console.SetMemory(Address, array);
        }

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte Value) {
            if (Connected)
                console.SetMemory(Address, new byte[] { BitConverter.GetBytes((short) Value)[0] });
        }

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte[] Value) {
            byte[] inArray = new byte[0];
            foreach (byte num in Value) {
                inArray.Push(out inArray, num);
            }
            if (Connected)
                console.SetMemory(Address, inArray);
        }

        public static void WriteString(this IXboxConsole console, uint Address, string String) {
            byte[] inArray = new byte[0];
            string str = String;
            for (int i = 0; i < str.Length; i++) {
                byte num = (byte) str[i];
                inArray.Push(out inArray, num);
            }
            inArray.Push(out inArray, 0);
            if (Connected)
                console.SetMemory(Address, inArray);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort Value) {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 2);
            if (Connected)
                console.SetMemory(Address, bytes);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort[] Value) {
            byte[] array = new byte[Value.Length * 2];
            for (int i = 0; i < Value.Length; i++) {
                BitConverter.GetBytes(Value[i]).CopyTo(array, (int) (i * 2));
            }
            ReverseBytes(array, 2);
            if (Connected)
                console.SetMemory(Address, array);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint Value) {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 4);
            if (Connected)
                console.SetMemory(Address, bytes);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint[] Value) {
            byte[] array = new byte[Value.Length * 4];
            for (int i = 0; i < Value.Length; i++) {
                BitConverter.GetBytes(Value[i]).CopyTo(array, (int) (i * 4));
            }
            ReverseBytes(array, 4);
            if (Connected)
                console.SetMemory(Address, array);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong Value) {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 8);
            if (Connected)
                console.SetMemory(Address, bytes);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong[] Value) {
            byte[] array = new byte[Value.Length * 8];
            for (int i = 0; i < Value.Length; i++) {
                BitConverter.GetBytes(Value[i]).CopyTo(array, (int) (i * 8));
            }
            ReverseBytes(array, 8);
            if (Connected)
                console.SetMemory(Address, array);
        }

        public static uint XamGetCurrentTitleId(this IXboxConsole console) {
            string command = "consolefeatures ver=" + JRPCVersion + " type=16 params=\"A\\0\\A\\0\\\"";
            string str2 = SendCommand(console, command);
            return uint.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
        }

        public static string XboxIP(this IXboxConsole console) {
            byte[] bytes = BitConverter.GetBytes(console.IPAddress);
            Array.Reverse(bytes);
            return new IPAddress(bytes).ToString();
        }

        public static void XNotify(this IXboxConsole console, XNotiyLogo type, string Text) {
            if (Connected)
                console.XNotify(Text, Convert.ToUInt32(type));
        }

        public static void XNotify(this IXboxConsole console, string Text, uint Type) {
            string command = string.Concat(new object[] { "consolefeatures ver=", JRPCVersion, " type=12 params=\"A\\0\\A\\2\\", String, "/", Text.Length, @"\", Text.ToHexString(), @"\", Int, @"\", Type, "\\\"" });
            SendCommand(console, command);
        }

        public enum LEDState {
            GREEN = 0x80,
            OFF = 0,
            ORANGE = 0x88,
            RED = 8
        }

        public enum TemperatureType {
            CPU,
            GPU,
            EDRAM,
            MotherBoard
        }

        public enum ThreadType {
            System,
            Title
        }

        public enum XNotiyLogo {
            ACHIEVEMENT_UNLOCKED = 0x1b,
            ACHIEVEMENTS_UNLOCKED = 0x27,
            AVATAR_AWARD_UNLOCKED = 60,
            BLANK = 0x2a,
            CANT_CONNECT_XBL_PARTY = 0x38,
            CANT_DOWNLOAD_X = 0x20,
            CANT_SIGN_IN_MESSENGER = 0x2b,
            DEVICE_FULL = 0x24,
            DISCONNECTED_FROM_XBOX_LIVE = 11,
            DISCONNECTED_XBOX_LIVE_11_MINUTES_REMAINING = 0x2e,
            DISCONNECTED_XBOX_LIVE_PARTY = 0x36,
            DOWNLOAD = 12,
            DOWNLOAD_STOPPED_FOR_X = 0x21,
            DOWNLOADED = 0x37,
            FAMILY_TIMER_X_TIME_REMAINING = 0x2d,
            FLASH_LOGO = 0x17,
            FLASHING_CHAT_ICON = 0x26,
            FLASHING_CHAT_SYMBOL = 0x41,
            FLASHING_DOUBLE_SIDED_HAMMER = 0x10,
            FLASHING_FROWNING_FACE = 15,
            FLASHING_HAPPY_FACE = 14,
            FLASHING_MUSIC_SYMBOL = 13,
            FLASHING_XBOX_CONSOLE = 0x22,
            FLASHING_XBOX_LOGO = 4,
            FOUR_2 = 0x19,
            FOUR_3 = 0x1a,
            FOUR_5 = 0x30,
            FOUR_7 = 0x25,
            FOUR_9 = 0x1c,
            FRIEND_REQUEST_LOGO = 2,
            GAME_INVITE_SENT = 0x16,
            GAME_INVITE_SENT_TO_XBOX_LIVE_PARTY = 0x33,
            GAMER_PICTURE_UNLOCKED = 0x3b,
            GAMERTAG_HAS_JOINED_CHAT = 20,
            GAMERTAG_HAS_JOINED_XBL_PARTY = 0x39,
            GAMERTAG_HAS_LEFT_CHAT = 0x15,
            GAMERTAG_HAS_LEFT_XBL_PARTY = 0x3a,
            GAMERTAG_SENT_YOU_A_MESSAGE = 5,
            GAMERTAG_SIGNED_IN_OFFLINE = 9,
            GAMERTAG_SIGNED_INTO_XBOX_LIVE = 8,
            GAMERTAG_SIGNEDIN = 7,
            GAMERTAG_SINGED_OUT = 6,
            GAMERTAG_WANTS_TO_CHAT = 10,
            GAMERTAG_WANTS_TO_CHAT_2 = 0x11,
            GAMERTAG_WANTS_TO_TALK_IN_VIDEO_KINECT = 0x1d,
            GAMERTAG_WANTS_YOU_TO_JOIN_AN_XBOX_LIVE_PARTY = 0x31,
            JOINED_XBL_PARTY = 0x3d,
            KICKED_FROM_XBOX_LIVE_PARTY = 0x34,
            KINECT_HEALTH_EFFECTS = 0x2f,
            MESSENGER_DISCONNECTED = 0x29,
            MISSED_MESSENGER_CONVERSATION = 0x2c,
            NEW_MESSAGE = 3,
            NEW_MESSAGE_LOGO = 1,
            NULLED = 0x35,
            PAGE_SENT_TO = 0x18,
            PARTY_INVITE_SENT = 50,
            PLAYER_MUTED = 0x3f,
            PLAYER_UNMUTED = 0x40,
            PLEASE_RECONNECT_CONTROLLERM = 0x13,
            PLEASE_REINSERT_MEMORY_UNIT = 0x12,
            PLEASE_REINSERT_USB_STORAGE_DEVICE = 0x3e,
            READY_TO_PLAY = 0x1f,
            UPDATING = 0x4c,
            VIDEO_CHAT_INVITE_SENT = 30,
            X_HAS_SENT_YOU_A_NUDGE = 40,
            X_SENT_YOU_A_GAME_MESSAGE = 0x23,
            XBOX_LOGO = 0
        }
    }
}
