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
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Style;
using ZqUtils.Extensions;
using System.Threading.Tasks;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Excel工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Excel工具类
    /// </summary>
    public class ExcelHelper
    {
        #region 导入Excel
        /// <summary>
        /// EPPlus读取Excel(2003/2007)
        /// </summary>
        /// <param name="fullPath">Excel路径</param>
        /// <returns>DataTable集合</returns>
        public static List<DataTable> EPPlusReadExcel(string fullPath)
        {
            var list = new List<DataTable>();
            using (var package = new ExcelPackage(new FileInfo(fullPath)))
            {
                for (var sheetIndex = 1; sheetIndex <= package.Workbook.Worksheets.Count; sheetIndex++)
                {
                    var table = new DataTable();
                    using (var sheet = package.Workbook.Worksheets[sheetIndex])
                    {
                        var colCount = sheet.Dimension.End.Column;
                        var rowCount = sheet.Dimension.End.Row;
                        for (var j = 1; j <= colCount; j++)
                        {
                            table.Columns.Add(new DataColumn(sheet.Cells[1, j].Value.ToString()));
                        }
                        for (var i = 2; i <= rowCount; i++)
                        {
                            var row = table.NewRow();
                            for (var j = 1; j <= colCount; j++)
                            {
                                row[j - 1] = sheet.Cells[i, j].Value;
                            }
                            table.Rows.Add(row);
                        }
                    }
                    list.Add(table);
                }
            }
            return list;
        }

        /// <summary>
        /// EPPlus读取Excel(2003/2007)
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <returns>DataTable集合</returns>
        public static List<DataTable> EPPlusReadExcel(Stream fileStream)
        {
            var list = new List<DataTable>();
            using (var package = new ExcelPackage(fileStream))
            {
                for (var sheetIndex = 1; sheetIndex <= package.Workbook.Worksheets.Count; sheetIndex++)
                {
                    var table = new DataTable();
                    using (var sheet = package.Workbook.Worksheets[sheetIndex])
                    {
                        var colCount = sheet.Dimension.End.Column;
                        var rowCount = sheet.Dimension.End.Row;
                        for (var j = 1; j <= colCount; j++)
                        {
                            table.Columns.Add(new DataColumn(sheet.Cells[1, j].Value.ToString()));
                        }
                        for (var i = 2; i <= rowCount; i++)
                        {
                            var row = table.NewRow();
                            for (var j = 1; j <= colCount; j++)
                            {
                                row[j - 1] = sheet.Cells[i, j].Value;
                            }
                            table.Rows.Add(row);
                        }
                    }
                    list.Add(table);
                }
            }
            return list;
        }

        /// <summary>
        /// EPPlus读取Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="fullPath">Excel路径</param>
        /// <returns>泛型集合</returns>
        public static List<List<T>> EPPlusReadExcel<T>(string fullPath) where T : class, new()
        {
            var lists = new List<List<T>>();
            var headers = new Dictionary<string, int>();
            var file = new FileInfo(fullPath);
            using (var package = new ExcelPackage(file))
            {
                for (var sheetIndex = 1; sheetIndex <= package.Workbook.Worksheets.Count; sheetIndex++)
                {
                    var list = new List<T>();
                    using (var worksheet = package.Workbook.Worksheets[sheetIndex])
                    {
                        var colStart = worksheet.Dimension.Start.Column;//工作区开始列
                        var colEnd = worksheet.Dimension.End.Column;//工作区结束列
                        var rowStart = worksheet.Dimension.Start.Row;//工作区开始行号
                        var rowEnd = worksheet.Dimension.End.Row;//工作区结束行号
                                                                 //将每列标题添加到字典中                                               
                        for (int i = colStart; i <= colEnd; i++)
                        {
                            headers[worksheet.Cells[rowStart, i].Value.ToString()] = i;
                        }
                        var propertyInfoList = new List<PropertyInfo>(typeof(T).GetProperties());
                        for (int row = rowStart + 1; row <= rowEnd; row++)
                        {
                            var result = new T();
                            //为对象T的各属性赋值
                            foreach (var p in propertyInfoList)
                            {
                                //与属性名对应的单元格
                                var cell = worksheet.Cells[row, headers[p.GetExcelColumn()]];
                                if (cell.Value == null) continue;
                                switch (p.PropertyType.GetCoreType().Name.ToLower())
                                {
                                    case "string":
                                        p.SetValue(result, cell.GetValue<string>(), null);
                                        break;
                                    case "int16":
                                        p.SetValue(result, cell.GetValue<short>(), null);
                                        break;
                                    case "int32":
                                        p.SetValue(result, cell.GetValue<int>(), null);
                                        break;
                                    case "int64":
                                        p.SetValue(result, cell.GetValue<int>(), null);
                                        break;
                                    case "decimal":
                                        p.SetValue(result, cell.GetValue<decimal>(), null);
                                        break;
                                    case "double":
                                        p.SetValue(result, cell.GetValue<double>(), null);
                                        break;
                                    case "datetime":
                                        p.SetValue(result, cell.GetValue<DateTime>(), null);
                                        break;
                                    case "boolean":
                                        p.SetValue(result, cell.GetValue<bool>(), null);
                                        break;
                                    case "byte":
                                        p.SetValue(result, cell.GetValue<byte>(), null);
                                        break;
                                    case "char":
                                        p.SetValue(result, cell.GetValue<char>(), null);
                                        break;
                                    case "single":
                                        p.SetValue(result, cell.GetValue<float>(), null);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            list.Add(result);
                        }
                    }
                    lists.Add(list);
                }
            }
            return lists;
        }

        /// <summary>
        /// EPPlus读取Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="fileStream">文件流</param>
        /// <returns>泛型集合</returns>
        public static List<List<T>> EPPlusReadExcel<T>(Stream fileStream) where T : class, new()
        {
            var lists = new List<List<T>>();
            var headers = new Dictionary<string, int>();
            using (var package = new ExcelPackage(fileStream))
            {
                for (var sheetIndex = 1; sheetIndex <= package.Workbook.Worksheets.Count; sheetIndex++)
                {
                    var list = new List<T>();
                    using (var worksheet = package.Workbook.Worksheets[sheetIndex])
                    {
                        var colStart = worksheet.Dimension.Start.Column;//工作区开始列
                        var colEnd = worksheet.Dimension.End.Column;//工作区结束列
                        var rowStart = worksheet.Dimension.Start.Row;//工作区开始行号
                        var rowEnd = worksheet.Dimension.End.Row;//工作区结束行号
                                                                 //将每列标题添加到字典中                                               
                        for (int i = colStart; i <= colEnd; i++)
                        {
                            headers[worksheet.Cells[rowStart, i].Value.ToString()] = i;
                        }
                        var propertyInfoList = new List<PropertyInfo>(typeof(T).GetProperties());
                        for (int row = rowStart + 1; row <= rowEnd; row++)
                        {
                            var result = new T();
                            //为对象T的各属性赋值
                            foreach (var p in propertyInfoList)
                            {
                                //与属性名对应的单元格
                                var cell = worksheet.Cells[row, headers[p.GetExcelColumn()]];
                                if (cell.Value == null) continue;
                                switch (p.PropertyType.GetCoreType().Name.ToLower())
                                {
                                    case "string":
                                        p.SetValue(result, cell.GetValue<string>(), null);
                                        break;
                                    case "int16":
                                        p.SetValue(result, cell.GetValue<short>(), null);
                                        break;
                                    case "int32":
                                        p.SetValue(result, cell.GetValue<int>(), null);
                                        break;
                                    case "int64":
                                        p.SetValue(result, cell.GetValue<int>(), null);
                                        break;
                                    case "decimal":
                                        p.SetValue(result, cell.GetValue<decimal>(), null);
                                        break;
                                    case "double":
                                        p.SetValue(result, cell.GetValue<double>(), null);
                                        break;
                                    case "datetime":
                                        p.SetValue(result, cell.GetValue<DateTime>(), null);
                                        break;
                                    case "boolean":
                                        p.SetValue(result, cell.GetValue<bool>(), null);
                                        break;
                                    case "byte":
                                        p.SetValue(result, cell.GetValue<byte>(), null);
                                        break;
                                    case "char":
                                        p.SetValue(result, cell.GetValue<char>(), null);
                                        break;
                                    case "single":
                                        p.SetValue(result, cell.GetValue<float>(), null);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            list.Add(result);
                        }
                    }
                    lists.Add(list);
                }
            }
            return lists;
        }
        #endregion

        #region 导出Excel到字节码
        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="table">源DataTable</param>
        /// <param name="action">sheet自定义处理委托</param>
        /// <returns></returns>
        public static byte[] EPPlusExportExcelToBytes(DataTable table, Action<ExcelWorksheet> action = null)
        {
            if (table?.Rows.Count > 0)
            {
                using (var package = new ExcelPackage())
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add(table.TableName.IsNullOrEmpty() ? "Sheet1" : table.TableName))
                    {
                        sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light1);
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //单独设置单元格
                        action?.Invoke(sheet);

                        return package.GetAsByteArray();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="table">源DataTable</param>
        /// <param name="action">sheet自定义处理委托</param>
        /// <returns></returns>
        /// <returns></returns>
        public static byte[] EPPlusExportExcelToBytes(ExcelHeaderCell headerCell, DataTable table, Action<ExcelWorksheet> action = null)
        {
            if (table?.Rows.Count > 0)
            {
                using (var package = new ExcelPackage())
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add(table.TableName.IsNullOrEmpty() ? "Sheet1" : table.TableName))
                    {
                        //设置边框样式
                        sheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        //水平居中
                        sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        //垂直居中
                        sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //构建表头
                        BuildExcelHeader(null, null, headerCell, sheet);
                        //加载数据
                        var firstCell = headerCell.ChildHeaderCells.FirstOrDefault();
                        sheet.Cells[firstCell.ToRow + 1, firstCell.ToCol].LoadFromDataTable(table, false);
                        //单独设置单元格
                        action?.Invoke(sheet);

                        return package.GetAsByteArray();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="dataSet">源DataSet</param>
        /// <param name="action">sheet自定义处理委托</param>
        /// <returns></returns>
        public static byte[] EPPlusExportExcelToBytes(DataSet dataSet, Action<ExcelWorksheet> action = null)
        {
            if (dataSet?.Tables.Count > 0)
            {
                using (var package = new ExcelPackage())
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    for (var i = 0; i < dataSet.Tables.Count; i++)
                    {
                        var table = dataSet.Tables[i];
                        if (table != null && table.Rows.Count > 0)
                        {
                            var sheet = package.Workbook.Worksheets.Add(table.TableName.IsNullOrEmpty() ? $"Sheet{i + 1}" : table.TableName);
                            sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light1);
                            //单元格自动适应大小
                            sheet.Cells.AutoFitColumns();
                            //单独设置单元格
                            action?.Invoke(sheet);
                        }
                    }

                    return package.GetAsByteArray();
                }
            }

            return null;
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>
        /// <param name="action">sheet自定义处理委托</param>
        /// <returns></returns>
        public static byte[] EPPlusExportExcelToBytes<T>(IEnumerable<T> list, Action<ExcelWorksheet> action = null) where T : class, new()
        {
            if (list?.Count() > 0)
            {
                using (var package = new ExcelPackage())
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add("Sheet1"))
                    {
                        //获取导出列
                        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x =>
                        {
                            var attribute = x.GetAttribute<ExcelColumnAttribute>();
                            return attribute == null || attribute.IsExport;
                        }).ToArray();
                        sheet.Cells["A1"].LoadFromCollection(list, true, TableStyles.Light1, BindingFlags.Public | BindingFlags.Instance, props);
                        //设置Excel头部标题
                        var colStart = sheet.Dimension.Start.Column;//工作区开始列
                        var colEnd = sheet.Dimension.End.Column;//工作区结束列
                        for (var col = colStart; col <= colEnd; col++)
                        {
                            var prop = typeof(T).GetProperty(sheet.Cells[1, col].Value.ToString());
                            var attribute = prop.GetAttribute<ExcelColumnAttribute>();
                            if (attribute != null)
                            {
                                sheet.Cells[1, col].Value = attribute.ColumnName;

                                //公式
                                if (!attribute.Formula.IsNullOrEmpty())
                                    sheet.Cells[2, col, sheet.Dimension.End.Row, col].Formula = attribute.Formula;

                                //格式化
                                if (!attribute.Format.IsNullOrEmpty())
                                {
                                    var format = attribute.Format.Split('@');
                                    //日期
                                    if (format[0] == "date")
                                    {
                                        sheet.Cells[2, col, sheet.Dimension.End.Row, col].Style.Numberformat.Format = format[1];
                                    }
                                    //字典
                                    else if (format[0] == "dic")
                                    {
                                        var dic = format[1].Split(',').ToDictionary(k => k.Split(':')[0], v => v.Split(':')[1]);
                                        for (int i = 2; i <= sheet.Dimension.End.Row; i++)
                                        {
                                            var v = sheet.Cells[i, col].Value?.ToString();
                                            if (v != null && dic.Keys.Contains(v))
                                                sheet.Cells[i, col].Value = dic[v];
                                        }
                                    }
                                }
                            }
                        }
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //单独设置单元格
                        action?.Invoke(sheet);

                        return package.GetAsByteArray();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>
        /// <param name="columnName">表头数组</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static byte[] EPPlusExportExcelToBytes<T>(IEnumerable<T> list, string[] columnName = null, Action<ExcelWorksheet> action = null) where T : class, new()
        {
            if (list?.Count() > 0)
            {
                using (var package = new ExcelPackage())
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add("Sheet1"))
                    {
                        sheet.Cells["A1"].LoadFromCollection(list, true, TableStyles.Light1);
                        //设置Excel头部标题
                        if (columnName?.Length > 0)
                        {
                            for (var i = 0; i < columnName.Length; i++)
                            {
                                sheet.Cells[1, i + 1].Value = columnName[i];
                            }
                        }
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //单独设置单元格
                        action?.Invoke(sheet);

                        return package.GetAsByteArray();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="list">源泛型集合</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static byte[] EPPlusExportExcelToBytes<T>(ExcelHeaderCell headerCell, IEnumerable<T> list, Action<ExcelWorksheet> action = null) where T : class, new()
        {
            if (list?.Count() > 0)
            {
                using (var package = new ExcelPackage())
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add("Sheet1"))
                    {
                        //设置边框样式
                        sheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        //水平居中
                        sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        //垂直居中
                        sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //构建表头
                        BuildExcelHeader(null, null, headerCell, sheet);
                        //加载数据
                        var firstCell = headerCell.ChildHeaderCells.FirstOrDefault();
                        sheet.Cells[firstCell.ToRow + 1, firstCell.ToCol].LoadFromCollection(list, false);
                        //单独设置单元格
                        action?.Invoke(sheet);

                        return package.GetAsByteArray();
                    }
                }
            }

            return null;
        }
        #endregion

        #region 导出Excel到浏览器
        #region 同步方法
        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="table">源DataTable</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcel(DataTable table, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null)
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(table, action);
            if (bytes != null)
            {
                FileHelper.GetFile(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="table">源DataTable</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcel(ExcelHeaderCell headerCell, DataTable table, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null)
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(headerCell, table, action);
            if (bytes != null)
            {
                FileHelper.GetFile(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="dataSet">源DataSet</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcel(DataSet dataSet, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null)
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(dataSet, action);
            if (bytes != null)
            {
                FileHelper.GetFile(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcel<T>(IEnumerable<T> list, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null) where T : class, new()
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(list, action);
            if (bytes != null)
            {
                FileHelper.GetFile(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>
        /// <param name="fileName">文件名</param>
        /// <param name="columnName">表头数组</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcel<T>(IEnumerable<T> list, string fileName, string[] columnName = null, string ext = ".xlsx", Action<ExcelWorksheet> action = null) where T : class, new()
        {
            var bytes = EPPlusExportExcelToBytes(list, columnName, action);
            if (bytes != null)
            {
                FileHelper.GetFile(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="list">源泛型集合</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcel<T>(ExcelHeaderCell headerCell, IEnumerable<T> list, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null) where T : class, new()
        {
            var bytes = EPPlusExportExcelToBytes(headerCell, list, action);
            if (bytes != null)
            {
                FileHelper.GetFile(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="table">源DataTable</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static async Task EPPlusExportExcelAsync(DataTable table, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null)
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(table, action);
            if (bytes != null)
            {
                await FileHelper.GetFileAsync(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="table">源DataTable</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static async Task EPPlusExportExcelAsync(ExcelHeaderCell headerCell, DataTable table, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null)
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(headerCell, table, action);
            if (bytes != null)
            {
                await FileHelper.GetFileAsync(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="dataSet">源DataSet</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static async Task EPPlusExportExcelAsync(DataSet dataSet, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null)
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(dataSet, action);
            if (bytes != null)
            {
                await FileHelper.GetFileAsync(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static async Task EPPlusExportExcelAsync<T>(IEnumerable<T> list, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null) where T : class, new()
        {
            //获取bytes
            var bytes = EPPlusExportExcelToBytes(list, action);
            if (bytes != null)
            {
                await FileHelper.GetFileAsync(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>
        /// <param name="fileName">文件名</param>
        /// <param name="columnName">表头数组</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static async Task EPPlusExportExcelAsync<T>(IEnumerable<T> list, string fileName, string[] columnName = null, string ext = ".xlsx", Action<ExcelWorksheet> action = null) where T : class, new()
        {
            var bytes = EPPlusExportExcelToBytes(list, columnName, action);
            if (bytes != null)
            {
                await FileHelper.GetFileAsync(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="list">源泛型集合</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static async Task EPPlusExportExcelAsync<T>(ExcelHeaderCell headerCell, IEnumerable<T> list, string fileName, string ext = ".xlsx", Action<ExcelWorksheet> action = null) where T : class, new()
        {
            var bytes = EPPlusExportExcelToBytes(headerCell, list, action);
            if (bytes != null)
            {
                await FileHelper.GetFileAsync(bytes, HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8), "application/ms-excel");
            }
        }
        #endregion
        #endregion

        #region 导出Excel到文件
        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="table">源DataTable</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcelToFile(DataTable table, string savePath, Action<ExcelWorksheet> action = null)
        {
            if (table?.Rows.Count > 0)
            {
                using (var package = new ExcelPackage(new FileInfo(savePath)))
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add(table.TableName.IsNullOrEmpty() ? "Sheet1" : table.TableName))
                    {
                        sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light1);
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //单独设置单元格
                        action?.Invoke(sheet);
                        package.Save();
                    }
                }
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="table">源DataTable</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcelToFile(ExcelHeaderCell headerCell, DataTable table, string savePath, Action<ExcelWorksheet> action = null)
        {
            if (table?.Rows.Count > 0)
            {
                using (var package = new ExcelPackage(new FileInfo(savePath)))
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add(table.TableName.IsNullOrEmpty() ? "Sheet1" : table.TableName))
                    {
                        //设置边框样式
                        sheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        //水平居中
                        sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        //垂直居中
                        sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //构建表头
                        BuildExcelHeader(null, null, headerCell, sheet);
                        //加载数据
                        var firstCell = headerCell.ChildHeaderCells.FirstOrDefault();
                        sheet.Cells[firstCell.ToRow + 1, firstCell.ToCol].LoadFromDataTable(table, false);
                        //单独设置单元格
                        action?.Invoke(sheet);
                        package.Save();
                    }
                }
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="dataSet">源DataSet</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcelToFile(DataSet dataSet, string savePath, Action<ExcelWorksheet> action = null)
        {
            if (dataSet?.Tables.Count > 0)
            {
                using (var package = new ExcelPackage(new FileInfo(savePath)))
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    for (var i = 0; i < dataSet.Tables.Count; i++)
                    {
                        var table = dataSet.Tables[i];
                        if (table != null && table.Rows.Count > 0)
                        {
                            var sheet = package.Workbook.Worksheets.Add(table.TableName.IsNullOrEmpty() ? $"Sheet{i + 1}" : table.TableName);
                            sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light1);
                            //单元格自动适应大小
                            sheet.Cells.AutoFitColumns();
                            //单独设置单元格
                            action?.Invoke(sheet);
                        }
                    }
                    package.Save();
                }
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>        
        /// <param name="savePath">保存路径</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcelToFile<T>(IEnumerable<T> list, string savePath, Action<ExcelWorksheet> action = null) where T : class, new()
        {
            if (list?.Count() > 0)
            {
                using (var package = new ExcelPackage(new FileInfo(savePath)))
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add("Sheet1"))
                    {
                        //获取导出列
                        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x =>
                        {
                            var attribute = x.GetAttribute<ExcelColumnAttribute>();
                            return attribute == null || attribute.IsExport;
                        }).ToArray();
                        sheet.Cells["A1"].LoadFromCollection(list, true, TableStyles.Light1, BindingFlags.Public | BindingFlags.Instance, props);
                        //设置Excel头部标题
                        var colStart = sheet.Dimension.Start.Column;//工作区开始列
                        var colEnd = sheet.Dimension.End.Column;//工作区结束列
                        for (var col = colStart; col <= colEnd; col++)
                        {
                            var prop = typeof(T).GetProperty(sheet.Cells[1, col].Value.ToString());
                            var attribute = prop.GetAttribute<ExcelColumnAttribute>();
                            if (attribute != null)
                            {
                                sheet.Cells[1, col].Value = attribute.ColumnName;

                                //公式
                                if (!attribute.Formula.IsNullOrEmpty())
                                    sheet.Cells[2, col, sheet.Dimension.End.Row, col].Formula = attribute.Formula;

                                //格式化
                                if (!attribute.Format.IsNullOrEmpty())
                                {
                                    var format = attribute.Format.Split('@');
                                    //日期
                                    if (format[0] == "date")
                                    {
                                        sheet.Cells[2, col, sheet.Dimension.End.Row, col].Style.Numberformat.Format = format[1];
                                    }
                                    //字典
                                    else if (format[0] == "dic")
                                    {
                                        var dic = format[1].Split(',').ToDictionary(k => k.Split(':')[0], v => v.Split(':')[1]);
                                        for (int i = 2; i <= sheet.Dimension.End.Row; i++)
                                        {
                                            var v = sheet.Cells[i, col].Value?.ToString();
                                            if (v != null && dic.Keys.Contains(v))
                                                sheet.Cells[i, col].Value = dic[v];
                                        }
                                    }
                                }
                            }
                        }
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //单独设置单元格
                        action?.Invoke(sheet);
                        package.Save();
                    }
                }
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>        
        /// <param name="savePath">保存路径</param>
        /// <param name="columnName">表头数组</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcelToFile<T>(IEnumerable<T> list, string savePath, string[] columnName = null, Action<ExcelWorksheet> action = null) where T : class, new()
        {
            if (list?.Count() > 0)
            {
                using (var package = new ExcelPackage(new FileInfo(savePath)))
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add("Sheet1"))
                    {
                        sheet.Cells["A1"].LoadFromCollection(list, true, TableStyles.Light1);
                        //设置Excel头部标题
                        if (columnName?.Length > 0)
                        {
                            for (var i = 0; i < columnName.Length; i++)
                            {
                                sheet.Cells[1, i + 1].Value = columnName[i];
                            }
                        }
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //单独设置单元格
                        action?.Invoke(sheet);
                        package.Save();
                    }
                }
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="headerCell">Excel表头</param>
        /// <param name="list">源泛型集合</param>        
        /// <param name="savePath">保存路径</param>
        /// <param name="action">sheet自定义处理委托</param>
        public static void EPPlusExportExcelToFile<T>(ExcelHeaderCell headerCell, IEnumerable<T> list, string savePath, Action<ExcelWorksheet> action = null) where T : class, new()
        {
            if (list?.Count() > 0)
            {
                using (var package = new ExcelPackage(new FileInfo(savePath)))
                {
                    //配置文件属性
                    package.Workbook.Properties.Category = "类别";
                    package.Workbook.Properties.Author = "作者";
                    package.Workbook.Properties.Comments = "备注";
                    package.Workbook.Properties.Company = "公司名称";
                    package.Workbook.Properties.Keywords = "关键字";
                    package.Workbook.Properties.Manager = "张强";
                    package.Workbook.Properties.Status = "内容状态";
                    package.Workbook.Properties.Subject = "主题";
                    package.Workbook.Properties.Title = "标题";
                    package.Workbook.Properties.LastModifiedBy = "最后一次保存者";
                    using (var sheet = package.Workbook.Worksheets.Add("Sheet1"))
                    {
                        //设置边框样式
                        sheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        sheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        //水平居中
                        sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        //垂直居中
                        sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //单元格自动适应大小
                        sheet.Cells.AutoFitColumns();
                        //构建表头
                        BuildExcelHeader(null, null, headerCell, sheet);
                        //加载数据
                        var firstCell = headerCell.ChildHeaderCells.FirstOrDefault();
                        sheet.Cells[firstCell.ToRow + 1, firstCell.ToCol].LoadFromCollection(list, false);
                        //单独设置单元格
                        action?.Invoke(sheet);
                        package.Save();
                    }
                }
            }
        }
        #endregion

        #region 构建Excel头部
        /// <summary>
        /// 构建Excel表头
        /// </summary>
        /// <param name="prevCell">前一个单元格</param>
        /// <param name="parentSell">父级单元格</param>
        /// <param name="currCell">当前单元格</param>
        /// <param name="sheet">Excel WorkSheet</param>
        public static void BuildExcelHeader(ExcelHeaderCell prevCell, ExcelHeaderCell parentSell, ExcelHeaderCell currCell, ExcelWorksheet sheet)
        {
            currCell.FromRow = prevCell != null ? prevCell.FromRow : (parentSell != null ? parentSell.FromRow + 1 : 1);
            currCell.FromCol = prevCell != null ? prevCell.ToCol + 1 : (parentSell != null ? parentSell.FromCol : 1);
            currCell.ToRow = currCell.FromRow;
            currCell.ToCol = currCell.FromCol;
            if (currCell.IsRowspan)
            {
                currCell.ToRow = currCell.FromRow - 1 + currCell.Rowspan;
            }
            if (currCell.IsColspan)
            {
                currCell.ToCol = currCell.FromCol - 1 + currCell.Colspan;
            }
            var sell = sheet.Cells[currCell.FromRow, currCell.FromCol, currCell.ToRow, currCell.ToCol];
            //设置单元格属性
            sell.Value = currCell.Title;
            sell.Style.Font.Bold = currCell.Bold;
            sell.Style.Font.Color.SetColor(currCell.FontColor);
            if (currCell.HorizontalAlignment != null)
            {
                sell.Style.HorizontalAlignment = currCell.HorizontalAlignment.Value;
            }
            if (currCell.VerticalAlignment != null)
            {
                sell.Style.VerticalAlignment = currCell.VerticalAlignment.Value;
            }
            //合并单元格
            if (currCell.IsColspan || currCell.IsRowspan)
            {
                sheet.Cells[currCell.FromRow, currCell.FromCol, currCell.ToRow, currCell.ToCol].Merge = true;
            }
            //判断是否有子元素，递归调用
            if (currCell.ChildHeaderCells?.Count > 0)
            {
                foreach (var item in currCell.ChildHeaderCells)
                {
                    var index = currCell.ChildHeaderCells.IndexOf(item);
                    BuildExcelHeader(index == 0 ? null : currCell.ChildHeaderCells[index - 1], currCell, item, sheet);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Excel标题单元格实体
    /// </summary>
    public class ExcelHeaderCell
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 是否加粗
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// 字体颜色，默认黑色
        /// </summary>
        public Color FontColor { get; set; } = Color.Black;

        /// <summary>
        /// 水平方向布局
        /// </summary>
        public ExcelHorizontalAlignment? HorizontalAlignment { get; set; }

        /// <summary>
        /// 垂直方向布局
        /// </summary>
        public ExcelVerticalAlignment? VerticalAlignment { get; set; }

        /// <summary>
        /// 是否跨行
        /// </summary>
        public bool IsRowspan { get; set; }

        /// <summary>
        /// 跨行个数
        /// </summary>
        public int Rowspan { get; set; }

        /// <summary>
        /// 是否跨列
        /// </summary>
        public bool IsColspan { get; set; }

        /// <summary>
        /// 跨列个数
        /// </summary>
        public int Colspan { get; set; }

        /// <summary>
        /// 行开始位置
        /// </summary>
        public int FromRow { get; set; }

        /// <summary>
        /// 列开始位置
        /// </summary>
        public int FromCol { get; set; }

        /// <summary>
        /// 行结束位置
        /// </summary>
        public int ToRow { get; set; }

        /// <summary>
        /// 列结束位置
        /// </summary>
        public int ToCol { get; set; }

        /// <summary>
        /// 子单元格集合
        /// </summary>
        public List<ExcelHeaderCell> ChildHeaderCells { get; set; }
    }

    /// <summary>
    /// Excel导出列特性
    /// </summary>
    public class ExcelColumnAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="columnName">自定义列名</param>
        public ExcelColumnAttribute(string columnName = null)
        {
            if (columnName != null)
                this.ColumnName = columnName;
        }

        /// <summary>
        /// 自定义列名称
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 是否导出
        /// </summary>
        public bool IsExport { get; set; } = true;

        /// <summary>
        /// 格式化，暂支持 日期：date@yyyy-MM-dd HH:mm:ss；字典：dic@0:失败,1:成功
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Excel公式
        /// </summary>
        public string Formula { get; set; }
    }
}