using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Reflection;
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
            try
            {
                using (var package = new ExcelPackage(new FileInfo(fullPath)))
                {
                    for (var sheetIndex = 1; sheetIndex <= package.Workbook.Worksheets.Count; sheetIndex++)
                    {
                        var table = new DataTable();
                        var sheet = package.Workbook.Worksheets[sheetIndex];
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
                        list.Add(table);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "EPPlus读取Excel(2003/2007)");
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
            try
            {
                var dictHeader = new Dictionary<string, int>();
                var file = new FileInfo(fullPath);
                using (var package = new ExcelPackage(file))
                {
                    for (var sheetIndex = 1; sheetIndex <= package.Workbook.Worksheets.Count; sheetIndex++)
                    {
                        var list = new List<T>();
                        var worksheet = package.Workbook.Worksheets[sheetIndex];
                        var colStart = worksheet.Dimension.Start.Column;//工作区开始列
                        var colEnd = worksheet.Dimension.End.Column;//工作区结束列
                        var rowStart = worksheet.Dimension.Start.Row;//工作区开始行号
                        var rowEnd = worksheet.Dimension.End.Row;//工作区结束行号
                        //将每列标题添加到字典中                                               
                        for (int i = colStart; i <= colEnd; i++)
                        {
                            dictHeader[worksheet.Cells[rowStart, i].Value.ToString()] = i;
                        }
                        var propertyInfoList = new List<PropertyInfo>(typeof(T).GetProperties());
                        for (int row = rowStart + 1; row <= rowEnd; row++)
                        {
                            var result = new T();
                            //为对象T的各属性赋值
                            foreach (var p in propertyInfoList)
                            {
                                //与属性名对应的单元格
                                var cell = worksheet.Cells[row, dictHeader[p.Name]];
                                if (cell.Value == null) continue;
                                switch (p.PropertyType.Name.ToLower())
                                {
                                    case "string":
                                        p.SetValue(result, cell.GetValue<String>(), null);
                                        break;
                                    case "int16":
                                        p.SetValue(result, cell.GetValue<Int16>(), null);
                                        break;
                                    case "int32":
                                        p.SetValue(result, cell.GetValue<Int32>(), null);
                                        break;
                                    case "int64":
                                        p.SetValue(result, cell.GetValue<Int32>(), null);
                                        break;
                                    case "decimal":
                                        p.SetValue(result, cell.GetValue<Decimal>(), null);
                                        break;
                                    case "double":
                                        p.SetValue(result, cell.GetValue<Double>(), null);
                                        break;
                                    case "datetime":
                                        p.SetValue(result, cell.GetValue<DateTime>(), null);
                                        break;
                                    case "boolean":
                                        p.SetValue(result, cell.GetValue<Boolean>(), null);
                                        break;
                                    case "byte":
                                        p.SetValue(result, cell.GetValue<Byte>(), null);
                                        break;
                                    case "char":
                                        p.SetValue(result, cell.GetValue<Char>(), null);
                                        break;
                                    case "single":
                                        p.SetValue(result, cell.GetValue<Single>(), null);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            list.Add(result);
                        }
                        lists.Add(list);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "EPPlus读取Excel(2003/2007)");
            }
            return lists;
        }
        #endregion

        #region 导出Excel
        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="table">源DataTable</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="responseEnd">是否输出结束，默认：是</param>
        public static void EPPlusExportExcel(DataTable table, string fileName, string ext = ".xlsx", bool responseEnd = true)
        {
            if (table != null && table.Rows.Count > 0)
            {
                try
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
                        var sheet = package.Workbook.Worksheets.Add(string.IsNullOrEmpty(table.TableName) ? table.GetType().Name : table.TableName);
                        sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light10);
                        //写到客户端（下载）
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.Charset = "utf-8";
                        HttpContext.Current.Response.AddHeader("content-disposition", $"attachment;filename={HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8)}");
                        HttpContext.Current.Response.ContentType = "application/ms-excel";
                        HttpContext.Current.Response.ContentEncoding = Encoding.GetEncoding("utf-8");
                        HttpContext.Current.Response.BinaryWrite(package.GetAsByteArray());
                        HttpContext.Current.Response.Flush();
                        if (responseEnd) HttpContext.Current.Response.End();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex, "EPPlus导出Excel(2003/2007)");
                }
            }
            else
            {
                MessageHelper.Alert("暂无数据可导出！", HttpContext.Current.Request.Url.ToString(), 2);
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="ds">源DataSet</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名(.xls|.xlsx)可选参数</param>
        /// <param name="responseEnd">是否输出结束，默认：是</param>
        public static void EPPlusExportExcel(DataSet ds, string fileName, string ext = ".xlsx", bool responseEnd = true)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                try
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
                        for (var i = 0; i < ds.Tables.Count; i++)
                        {
                            var table = ds.Tables[i];
                            if (table != null && table.Rows.Count > 0)
                            {
                                var sheet = package.Workbook.Worksheets.Add(string.IsNullOrEmpty(table.TableName) ? table.GetType().Name + (i + 1).ToString() : table.TableName);
                                sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light10);
                            }
                        }
                        //写到客户端（下载）
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.Charset = "utf-8";
                        HttpContext.Current.Response.AddHeader("content-disposition", $"attachment;filename={HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8)}");
                        HttpContext.Current.Response.ContentType = "application/ms-excel";
                        HttpContext.Current.Response.ContentEncoding = Encoding.GetEncoding("utf-8");
                        HttpContext.Current.Response.BinaryWrite(package.GetAsByteArray());
                        HttpContext.Current.Response.Flush();
                        if (responseEnd) HttpContext.Current.Response.End();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex, "EPPlus导出Excel(2003/2007)");
                }
            }
            else
            {
                MessageHelper.Alert("暂无数据可导出！", HttpContext.Current.Request.Url.ToString(), 2);
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
        /// <param name="responseEnd">是否输出结束，默认：是</param>
        public static void EPPlusExportExcel<T>(List<T> list, string fileName, string[] columnName = null, string ext = ".xlsx", bool responseEnd = true) where T : class, new()
        {
            if (list != null && list.Count > 0)
            {
                try
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
                        var sheet = package.Workbook.Worksheets.Add(typeof(T).Name);
                        //设置Excel头部标题
                        if (columnName != null)
                        {
                            for (var i = 0; i < columnName.Length; i++)
                            {
                                sheet.Cells[1, i + 1].Value = columnName[i];
                            }
                            sheet.Cells["A2"].LoadFromCollection(list, false, TableStyles.Light10);
                        }
                        else
                        {
                            sheet.Cells["A1"].LoadFromCollection(list, true, TableStyles.Light10);
                        }
                        //写到客户端（下载）
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.Charset = "utf-8";
                        HttpContext.Current.Response.AddHeader("content-disposition", $"attachment;filename={HttpUtility.UrlEncode(fileName + ext, Encoding.UTF8)}");
                        HttpContext.Current.Response.ContentType = "application/ms-excel";
                        HttpContext.Current.Response.ContentEncoding = Encoding.GetEncoding("utf-8");
                        HttpContext.Current.Response.BinaryWrite(package.GetAsByteArray());
                        HttpContext.Current.Response.Flush();
                        if (responseEnd) HttpContext.Current.Response.End();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex, "EPPlus导出Excel(2003/2007)");
                }
            }
            else
            {
                MessageHelper.Alert("暂无数据可导出！", HttpContext.Current.Request.Url.ToString(), 2);
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="table">源DataTable</param>
        /// <param name="savePath">保存路径</param>
        public static void EPPlusExportExcelToFile(DataTable table, string savePath)
        {
            if (table != null && table.Rows.Count > 0)
            {
                try
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
                        var sheet = package.Workbook.Worksheets.Add(string.IsNullOrEmpty(table.TableName) ? table.GetType().Name : table.TableName);
                        sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light10);
                        package.Save();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex, "EPPlus导出Excel(2003/2007)");
                }
            }
            else
            {
                MessageHelper.Alert("暂无数据可导出！", HttpContext.Current.Request.Url.ToString(), 2);
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <param name="ds">源DataSet</param>
        /// <param name="savePath">保存路径</param>
        public static void EPPlusExportExcelToFile(DataSet ds, string savePath)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                try
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
                        for (var i = 0; i < ds.Tables.Count; i++)
                        {
                            var table = ds.Tables[i];
                            if (table != null && table.Rows.Count > 0)
                            {
                                var sheet = package.Workbook.Worksheets.Add(string.IsNullOrEmpty(table.TableName) ? table.GetType().Name + (i + 1).ToString() : table.TableName);
                                sheet.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Light10);
                            }
                        }
                        package.Save();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex, "EPPlus导出Excel(2003/2007)");
                }
            }
            else
            {
                MessageHelper.Alert("暂无数据可导出！", HttpContext.Current.Request.Url.ToString(), 2);
            }
        }

        /// <summary>
        /// EPPlus导出Excel(2003/2007)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">源泛型集合</param>        
        /// <param name="savePath">保存路径</param>
        /// <param name="columnName">表头数组</param>
        public static void EPPlusExportExcelToFile<T>(List<T> list, string savePath, string[] columnName = null) where T : class, new()
        {
            if (list != null && list.Count > 0)
            {
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(savePath)))
                    {
                        var sheet = package.Workbook.Worksheets.Add(typeof(T).Name);
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
                        //设置Excel头部标题
                        if (columnName != null)
                        {
                            for (var i = 0; i < columnName.Length; i++)
                            {
                                sheet.Cells[1, i + 1].Value = columnName[i];
                            }
                            sheet.Cells["A2"].LoadFromCollection(list, false, TableStyles.Light10);
                        }
                        else
                        {
                            sheet.Cells["A1"].LoadFromCollection(list, true, TableStyles.Light10);
                        }
                        package.Save();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex, "EPPlus导出Excel(2003/2007)");
                }
            }
            else
            {
                MessageHelper.Alert("暂无数据可导出！", HttpContext.Current.Request.Url.ToString(), 2);
            }
        }
        #endregion
    }
}
