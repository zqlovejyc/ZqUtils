#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ZqUtils.Extensions;
using RVA = System.UInt32;

namespace ZqUtils.Reflection
{
    /// <summary>
    /// PE镜像
    /// </summary>
    public class PEImage
    {
        #region 属性
        /// <summary>
        /// 可执行文件类型
        /// </summary>
        public PEFileKinds Kind { get; set; }

        /// <summary>
        /// 可执行文件代码特性
        /// </summary>
        public PortableExecutableKinds ExecutableKind { get; set; }

        /// <summary>
        /// 可执行文件的目标平台
        /// </summary>
        public ImageFileMachine Machine { get; set; }

        /// <summary>
        /// 标识特性
        /// </summary>
        public UInt16 Characteristics { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// 是否.Net程序
        /// </summary>
        public bool IsNet { get { return Version != null; } }
        // 不能只判断ILOnly，System.Data.SQLite.dll得到的ExecutableKind是24
        //public bool IsNet { get { return ExecutableKind.Has(PortableExecutableKinds.ILOnly) && Version != null; } }

        Section[] Sections;
        DataDirectory cli;
        DataDirectory metadata;
        #endregion

        #region 读取镜像
        /// <summary>读取镜像信息</summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static PEImage Read(string file)
        {
            if (string.IsNullOrEmpty(file)) return null;
            if (!File.Exists(file)) return null;
            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    return Read(fs);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>从数据流中读取PE文件头部</summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static PEImage Read(Stream stream)
        {
            if (stream.Length < 128) return null;
            var reader = new BinaryReader(stream);
            // - DOSHeader DOS头部 固定64字节

            // PE					2
            // Start				58
            // Lfanew				4
            // End					64

            // 幻数
            if (reader.ReadUInt16() != 0x5a4d) return null;
            stream.Seek(58, SeekOrigin.Current);
            // 最后4个字节是PEHeader位置
            stream.Position = reader.ReadUInt32();
            // 4字节PE文件头， PE\0\0
            if (reader.ReadUInt32() != 0x00004550) return null;
            // - PEFileHeader
            var image = new PEImage
            {
                // 执行环境平台
                // Machine				2
                Machine = (ImageFileMachine)reader.ReadUInt16()
            };
            // NumberOfSections		2
            var sections = reader.ReadUInt16();
            // TimeDateStamp		4
            // PointerToSymbolTable	4
            // NumberOfSymbols		4
            // OptionalHeaderSize	2
            stream.Seek(14, SeekOrigin.Current);
            // 一个标志的集合，其大部分位用于OBJ或LIB文件中
            // Characteristics		2
            image.Characteristics = reader.ReadUInt16();
            image.ReadOptionalHeaders(reader);
            image.ReadSections(reader, sections);
            image.ReadCLIHeader(reader);
            image.ReadMetadata(reader);
            return image;
        }

        void ReadOptionalHeaders(BinaryReader reader)
        {
            var stream = reader.BaseStream;
            // - PEOptionalHeader
            //   - StandardFieldsHeader

            // pe32 = 0x10b     pe64 = 0x20b
            // Magic				2
            var pe64 = reader.ReadUInt16() == 0x20b;

            //						pe32 || pe64

            // LMajor				1
            // LMinor				1
            // CodeSize				4
            // InitializedDataSize	4
            // UninitializedDataSize4
            // EntryPointRVA		4
            // BaseOfCode			4
            // BaseOfData			4 || 0

            //   - NTSpecificFieldsHeader

            // ImageBase			4 || 8
            // SectionAlignment		4
            // FileAlignement		4
            // OSMajor				2
            // OSMinor				2
            // UserMajor			2
            // UserMinor			2
            // SubSysMajor			2
            // SubSysMinor			2
            // Reserved				4
            // ImageSize			4
            // HeaderSize			4
            // FileChecksum			4
            stream.Seek(66, SeekOrigin.Current);

            // SubSystem			2
            var subsystem = reader.ReadUInt16();
            if ((Characteristics & 0x2000) != 0)
                Kind = PEFileKinds.Dll;
            else
            {
                switch (subsystem)
                {
                    case 1:
                        Kind = PEFileKinds.Dll;
                        break;
                    case 2:
                    case 9: // WinCE
                        Kind = PEFileKinds.WindowApplication;
                        break;
                    case 3:
                        Kind = PEFileKinds.ConsoleApplication;
                        break;
                    default:
                        Kind = (PEFileKinds)subsystem;
                        break;
                }
            }

            // DLLFlags				2
            // StackReserveSize		4 || 8
            // StackCommitSize		4 || 8
            // HeapReserveSize		4 || 8
            // HeapCommitSize		4 || 8
            // LoaderFlags			4
            // NumberOfDataDir		4

            //   - DataDirectoriesHeader

            // ExportTable			8
            // ImportTable			8
            // ResourceTable		8
            // ExceptionTable		8
            // CertificateTable		8
            // BaseRelocationTable	8

            stream.Seek(pe64 ? 90 : 74, SeekOrigin.Current);

            // Debug				8
            //image.Debug = new DataDirectory(reader.ReadUInt32(), reader.ReadUInt32());
            stream.Seek(8, SeekOrigin.Current);

            // Copyright			8
            // GlobalPtr			8
            // TLSTable				8
            // LoadConfigTable		8
            // BoundImport			8
            // IAT					8
            // DelayImportDescriptor8
            stream.Seek(56, SeekOrigin.Current);

            // CLIHeader			8
            cli = new DataDirectory(reader);
            //stream.Seek(8, SeekOrigin.Current);

            //if (cli.IsZero)
            //    throw new BadImageFormatException();

            // Reserved				8
            stream.Seek(8, SeekOrigin.Current);
        }

        void ReadSections(BinaryReader reader, UInt16 count)
        {
            var stream = reader.BaseStream;
            var sections = new Section[count];

            for (var i = 0; i < count; i++)
            {
                var section = new Section
                {

                    // Name
                    Name = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0')
                };

                // VirtualSize		4
                stream.Seek(4, SeekOrigin.Current);

                // VirtualAddress	4
                section.VirtualAddress = reader.ReadUInt32();
                // SizeOfRawData	4
                section.SizeOfRawData = reader.ReadUInt32();
                // PointerToRawData	4
                section.PointerToRawData = reader.ReadUInt32();

                // PointerToRelocations		4
                // PointerToLineNumbers		4
                // NumberOfRelocations		2
                // NumberOfLineNumbers		2
                // Characteristics			4
                stream.Seek(16, SeekOrigin.Current);
                sections[i] = section;
            }
            Sections = sections;
        }

        bool ReadCLIHeader(BinaryReader reader)
        {
            var stream = reader.BaseStream;
            var addr = ResolveVirtualAddress(cli.VirtualAddress);
            if (addr < 0) return false;
            stream.Position = addr;

            // - CLIHeader

            // Cb						4
            // MajorRuntimeVersion		2
            // MinorRuntimeVersion		2
            stream.Seek(8, SeekOrigin.Current);

            // Metadata					8
            metadata = new DataDirectory(reader);
            // Flags					4
            ExecutableKind = (PortableExecutableKinds)reader.ReadUInt32();
            // EntryPointToken			4
            //image.EntryPointToken = ReadUInt32();
            // Resources				8
            //image.Resources = ReadDataDirectory();
            // StrongNameSignature		8
            // CodeManagerTable			8
            // VTableFixups				8
            // ExportAddressTableJumps	8
            // ManagedNativeHeader		8

            return true;
        }

        bool ReadMetadata(BinaryReader reader)
        {
            var stream = reader.BaseStream;
            var addr = ResolveVirtualAddress(metadata.VirtualAddress);
            if (addr < 0) return false;
            stream.Position = addr;
            if (reader.ReadUInt32() != 0x424a5342) return false;

            // MajorVersion			2
            // MinorVersion			2
            // Reserved				4
            stream.Seek(8, SeekOrigin.Current);
            var buf = reader.ReadBytes(reader.ReadInt32());
            if (buf != null && buf.Length > 0)
            {
                var version = Encoding.ASCII.GetString(buf).TrimEnd('\0').TrimStart('v');
                try
                {
                    Version = new Version(version);
                }
                catch { }
            }
            return true;
        }
        #endregion

        #region 辅助
        /// <summary>能否在指定版本下加载程序集</summary>
        /// <remarks>
        /// 必须加强过滤，下面一旦只读加载，就再也不能删除文件
        /// </remarks>
        /// <param name="file">程序集文件</param>
        /// <param name="ver">指定模版，默认空表示当前版本</param>
        /// <param name="debug">是否输出调试日志</param>
        /// <returns></returns>
        public static bool CanLoad(string file, Version ver = null, bool debug = false)
        {
            if (file.IsNullOrEmpty()) return false;
            if (!File.Exists(file)) return false;
            // 仅加载.Net文件，并且小于等于当前版本
            var pe = Read(file);
            if (pe == null || !pe.IsNet) return false;
            if (ver == null) ver = new Version(Assembly.GetExecutingAssembly().ImageRuntimeVersion.TrimStart('v'));
            // 只判断主次版本，只要这两个相同，后面可以兼容
            var pv = pe.Version;
            if (pv.Major > ver.Major || pv.Major == ver.Major && pv.Minor > ver.Minor)
            {                
                return false;
            }
            // 必须加强过滤，下面一旦只读加载，就再也不能删除文件
            if (!pe.ExecutableKind.Has(PortableExecutableKinds.ILOnly))
            {
                // 判断x86/x64兼容。无法区分x86/x64的SQLite驱动
                //XTrace.WriteLine("{0,12} {1} {2}", item, pe.Machine, pe.ExecutableKind);
                //var x64 = pe.ExecutableKind.Has(PortableExecutableKinds.Required32Bit);
                //var x64 = pe.Machine == ImageFileMachine.AMD64;
                var x64 = pe.Machine == ImageFileMachine.AMD64;
                if (Runtime.Is64BitProcess ^ x64)
                {                   
                    return false;
                }
            }
            return true;
        }

        long ResolveVirtualAddress(RVA rva)
        {
            var section = GetSectionAtVirtualAddress(rva);
            if (section == null) return -1;
            return rva + section.PointerToRawData - section.VirtualAddress;
        }

        Section GetSectionAtVirtualAddress(RVA rva)
        {
            foreach (var section in Sections)
            {
                if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.SizeOfRawData)
                    return section;
            }
            return null;
        }
        #endregion

        #region 内部类
        struct DataDirectory
        {
            public readonly RVA VirtualAddress;
            public readonly RVA Size;

            public bool IsZero { get { return VirtualAddress == 0 && Size == 0; } }

            public DataDirectory(RVA rva, RVA size)
            {
                VirtualAddress = rva;
                Size = size;
            }

            public DataDirectory(BinaryReader reader) : this(reader.ReadUInt32(), reader.ReadUInt32()) { }
        }

        sealed class Section
        {
            public string Name;
            public RVA VirtualAddress;
            //public uint VirtualSize;
            public RVA SizeOfRawData;
            public RVA PointerToRawData;
            //public byte[] Data;
        }
        #endregion
    }
}