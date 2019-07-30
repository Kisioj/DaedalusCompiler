﻿using System;
using System.Diagnostics;
using System.Linq;
using DaedalusCompiler.Compilation;
using DaedalusCompiler.Compilation.SemanticAnalysis;

namespace DaedalusCompiler.Dat
{
    public enum DatSymbolType
    {
        Void = 0,
        Float = 1,
        Int = 2,
        String = 3,
        Class = 4,
        Func = 5,
        Prototype = 6,
        Instance = 7,
        Undefined = 8,
    }

    [Flags]
    public enum DatSymbolFlag
    {
        Const = 1,
        Return = 2,
        Classvar = 4,
        External = 8,
        Merged = 16,
    }
    

    [DebuggerDisplay("{BuiltinType} {ReturnType} {Name} '{Flags}'")]
    public class DatSymbol
    {
        public const int NULL_INDEX = -1;

        public NodeValue arrayLengthValue;
        public NodeValue[] contentValue;
        
        
        /// <summary>
        /// Symbol's index, id. It shows how many symbols were loaded before this one.
        /// </summary>
        public int Index { get; set; }
        
        /// <summary>
        /// Symbol name like C_MISSION.RUNNING, C_ITEM, MAX_WISPSKILL
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Length for array variables or constants. Set to 1 for non array variables or constants
        /// </summary>
        public uint ArrayLength { get; set; }

        /// <summary>
        /// Only for functions
        /// </summary>
        public uint ParametersCount { get; set; }
        
        /// <summary>
        /// Symbol type ex. DatSymbolType.Class or DatSymbolType.Func
        /// </summary>
        public DatSymbolType BuiltinType { get; set; }

        /// <summary>
        /// Symbol type in string format ex. 'class' or 'func'. Later evaluated with result saved into SymbolType
        /// </summary>
        public string ComplexTypeName { get; set; }
        
        /// <summary>
        /// Symbol flags ex. 'const' or 'return' or 'const, external, return'
        /// </summary>
        public DatSymbolFlag Flags { get; set; }

        /// <summary>
        /// Return type which is set only for 'func' symbols with 'return' flag set
        /// </summary>
        public DatSymbolType ReturnType { get; set; }

        /// <summary>
        /// Addres of parent 'class' symbol set only for 'classvar' symbol.
        /// </summary>
        public int ClassVarOffset { get; set; }

        /// <summary>
        /// Fields count which is set only for 'class' symbol
        /// </summary>
        public int ClassSize { get; set; }

        /// <summary>
        /// Specifies where symbol is located in source scripts
        /// </summary>
        public NodeLocation Location { get; set; }

        /// <summary>
        /// Content of const variable or array
        /// </summary>
        public object[] Content { get; set; }

        /// <summary>
        /// Address of first token. Only set for 'function', 'instance' and 'prototype' symbols.
        /// </summary>
        public int FirstTokenAddress  { get; set; }
        
        /// <summary>
        /// Only set for 'class' symbol.
        /// </summary>
        public int ClassOffset  { get; set; }
        
        /// <summary>
        /// Reference to parent symbol for nested symbols like class variables
        /// </summary>
        //public int ParentIndex { get; set; }
        public DatSymbol Parent { get; set; }
        
        
        //public ErrorLineContext ErrorLineContext;
        
        public DatSymbol()
        {
            Index = -1;
            Name = "";
            ArrayLength = 0;

            ParametersCount = 0;
            BuiltinType = DatSymbolType.Undefined;
            ComplexTypeName = "";
            
            Flags = 0;
            ReturnType = DatSymbolType.Void;

            ClassVarOffset = 0;
            ClassSize = 0;

            Location = null;
            
            Content = null;
            FirstTokenAddress = 0;
            ClassOffset = 0;
            
            Parent = null;
            
            //ErrorLineContext = null;
        }

        public bool IsStringLiteralSymbol()
        {
            return Name.StartsWith($"{(char) 255}") && BuiltinType == DatSymbolType.String && Flags == DatSymbolFlag.Const;
        }
        
        /// <summary>
        /// Saves DatSymbol to binary stream using DAT format 
        /// </summary>
        public void Save(DatBinaryWriter writer)
        {
            // Save name
            writer.Write(Convert.ToUInt32(Name != null));
            if (Name != null)
            {
                writer.Write(Name);
            }

            // Save ReturnType / ClassSize / ClassVarOffset
            if (BuiltinType == DatSymbolType.Func && Flags.HasFlag(DatSymbolFlag.Return))
            {
                writer.Write((int)ReturnType);
            }
            else if (Flags.HasFlag(DatSymbolFlag.Classvar))
            {
                writer.Write(ClassVarOffset);
            }
            else if (BuiltinType == DatSymbolType.Class)
            {
                writer.Write(ClassSize);
            }
            else
            {
                writer.Write(0);
            }

            // Save ArrayLength & Type & Flags
            var bitField = 0u;
            if (BuiltinType == DatSymbolType.Func && !Flags.HasFlag(DatSymbolFlag.Classvar))
            {
                bitField |= ParametersCount;
            }
            else
            {
                bitField |= ArrayLength;
            }
            bitField |= ((uint)BuiltinType << 12);
            bitField |= ((uint)Flags << 16);
            bitField |= 0x400000;
            writer.Write(bitField);

            writer.Write(Location.FileIndex);
            writer.Write(Location.Line);
            writer.Write(Location.LinesCount);
            writer.Write(Location.Index);
            writer.Write(Location.CharsCount);

            if (!Flags.Equals(0) && !Flags.HasFlag(DatSymbolFlag.Classvar))
            {
                switch (BuiltinType)
                {
                    case DatSymbolType.Class:
                        writer.Write(ClassOffset);
                        break;
                
                    case DatSymbolType.Func:
                    case DatSymbolType.Instance:
                    case DatSymbolType.Prototype:
                        writer.Write(FirstTokenAddress);
                        break;
                
                    default:
                        foreach(var obj in Content ?? Enumerable.Empty<object>())
                        {
                            switch (BuiltinType)
                            {
                                case DatSymbolType.String:
                                    writer.Write((string)obj);
                                    break;
                                case DatSymbolType.Float:
                                    writer.Write(Convert.ToSingle(obj));
                                    break;
                                default:
                                    writer.Write((int)obj);
                                    break;
                            }
                        }
                        break;
                }
            }

            // Save parent
            writer.Write(Parent?.Index ?? -1);
        }

        /// <summary>
        /// Loads DatSymbol from binary DAT formatted stream
        /// </summary>
        public static DatSymbol Load(DatBinaryReader reader)
        {
            var symbol = new DatSymbol();

            // Read Name
            var hasName = Convert.ToBoolean(reader.ReadUInt32());
            if (hasName)
            {
                symbol.Name = reader.ReadString();
            }

            // Read ReturnType / ClassSize / ClassVarOffset / ArrayLength / Type / Flags
            var valueField = reader.ReadInt32();
            var bitField = reader.ReadUInt32();

            symbol.ArrayLength = bitField & 0xFFF;
            symbol.BuiltinType = (DatSymbolType)((bitField & 0xF000) >> 12);
            symbol.Flags = (DatSymbolFlag)((bitField & 0x3F0000) >> 16);

            if (symbol.BuiltinType == DatSymbolType.Func && symbol.Flags.HasFlag(DatSymbolFlag.Return))
            {
                symbol.ReturnType = (DatSymbolType)valueField;
            }

            if (symbol.BuiltinType == DatSymbolType.Class)
            {
                symbol.ClassSize = valueField;
            }

            if (symbol.Flags.HasFlag(DatSymbolFlag.Classvar))
            {
                symbol.ClassVarOffset = valueField;
            }

            symbol.Location = new NodeLocation
            {
                FileIndex = reader.ReadInt32(),
                Line = reader.ReadInt32(),
                LinesCount = reader.ReadInt32(),
                Index = reader.ReadInt32(),
                CharsCount = reader.ReadInt32(),
            };

            switch (symbol.BuiltinType)
            {
                case DatSymbolType.Class:
                    symbol.ClassOffset = reader.ReadInt32();
                    break;
                
                case DatSymbolType.Func:
                case DatSymbolType.Instance:
                case DatSymbolType.Prototype:
                    symbol.FirstTokenAddress = reader.ReadInt32();
                    break;
                
                default:
                    symbol.Content = GetContentIfExists(reader, symbol);
                    break;
            }

            //symbol.ParentIndex = reader.ReadInt32();

            return symbol;
        }

        private static object[] GetContentIfExists(DatBinaryReader reader, DatSymbol symbol)
        {
            // TODO : Verify and refactor this method.

            object[] result = null;

            if (symbol.Flags.HasFlag(DatSymbolFlag.Classvar) == false)
            {
                if (symbol.BuiltinType == DatSymbolType.Func || symbol.BuiltinType == DatSymbolType.Class || symbol.BuiltinType == DatSymbolType.Prototype)
                {
                    result = new object[1];
                }
                else
                {
                    result = new object[symbol.ArrayLength];
                }

                if ((result.Length == 0) && (symbol.BuiltinType == DatSymbolType.Instance))
                {
                    result = new object[1];
                }

                for (int i = 0; i < result.Length; i++)
                {
                    switch (symbol.BuiltinType)
                    {
                        case DatSymbolType.String:
                            result[i] = reader.ReadString();
                            break;
                        case DatSymbolType.Float:
                            result[i] = reader.ReadSingle();
                            break;
                        default:
                            result[i] = reader.ReadInt32();
                            break;
                    }
                }
            }

            return result;
        }
    }

    public class DatSymbolReference
    {
        public static readonly DatSymbol UndeclaredSymbol = new DatSymbol();

        /*
        object[10].attribute[10] - NOT POSSIBLE NOW
        object.attribute[10]     - OK
         */
        public DatSymbol Object;
        public DatSymbol Attribute;
        public int Index;

        public DatSymbolReference()
        {
            Object = UndeclaredSymbol;
            Attribute = null;
            Index = -1;
        }

        public bool HasAttribute()
        {
            return Attribute != null;
        }
        public DatSymbol GetSymbol()
        {
            if (HasAttribute())
            {
                return Attribute;
            }

            return Object;
        }

        public DatSymbolType GetSymbolType()
        {
            DatSymbol symbol = GetSymbol();
            if (symbol == null)
            {
                return DatSymbolType.Undefined;
            }
            return GetSymbol().BuiltinType;
        }
    }
}
