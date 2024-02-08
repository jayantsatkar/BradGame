using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;


namespace Slot_Simulator
{
    class ExcelFile : IDisposable
    {
        internal string FilePath, FileName;
        Excel.Application m_application;
        Excel.Workbook m_workbook;
        Excel.Worksheet m_worksheet;
        internal Excel.Range Range;
        private bool m_keepOpen = false;
        internal ExcelFile(string _fileName)
        {

            FilePath = _fileName;
            FileInfo fileInfo = new FileInfo(FilePath);
            FileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);

            m_application = new Excel.Application();
            m_application.DisplayAlerts = false;
            m_workbook = m_application.Workbooks.Open(_fileName);
            m_application.Calculation = Excel.XlCalculation.xlCalculationManual;
            m_worksheet = (Excel.Worksheet)m_workbook.Worksheets.get_Item(1);
            Range = m_worksheet.UsedRange;
        }
        internal static void SaveExcel(List<List<string>> _data, string _filePath)
        {
            using (ExcelFile excelFile = new ExcelFile(_data, false))
                excelFile.SaveFile(_filePath);
        }
        internal static void CreateExcel(List<List<string>> _data)
        {
            ExcelFile excelFile = new ExcelFile(_data, true);
            excelFile.Dispose();
        }
        internal ExcelFile(List<List<string>> _data, bool _keepOpen = false)
        {
            m_keepOpen = _keepOpen;
            m_application = new Excel.Application();
            m_workbook = m_application.Workbooks.Add();
            m_worksheet = (Excel.Worksheet)m_workbook.Worksheets.get_Item(1);

            int maxColumns = 1;
            foreach (List<string> row in _data)
                maxColumns = Math.Max(maxColumns, row.Count);
            Excel.Range startCell = m_worksheet.Cells[1, 1];
            Excel.Range endCell = m_worksheet.Cells[_data.Count, maxColumns];
            Range = m_worksheet.Range[startCell, endCell];
            object[,] dataCells = Range.Value2;
            for (int row = 0; row < _data.Count; row++)
                if (_data[row].Count > 0)
                {
                    for (int col = 0; col < _data[row].Count; col++)
                    {
                        dataCells[row + 1, col + 1] = _data[row][col];
                        if (_data[row][col].Contains('%'))
                        {
                            Excel.Range asdf = m_worksheet.Cells[row + 1, col + 1];
                            asdf.NumberFormat = "0.00%";
                        }
                    }
                }
            //Range.Style = "Normal";
            //Range.NumberFormat = "";
            Range.Value2 = dataCells;
            Range.Columns.AutoFit();
        }
        internal void SaveFile(string _filePath)
        {
            m_application.DisplayAlerts = false;
            try
            {
                m_workbook.SaveAs(_filePath, Type.Missing, Type.Missing, Type.Missing, false);
            }
            catch
            {
                m_keepOpen = true;
            }
            m_application.DisplayAlerts = true;
        }
        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
        public void Dispose()
        {
            if (m_keepOpen)
            {
                m_application.ScreenUpdating = true;
                m_application.Visible = true;
                releaseObject(Range);
                releaseObject(m_worksheet);
            }
            else
            {
                m_application.DisplayAlerts = false;
                m_workbook.Close();
                m_application.Quit();
                m_application.DisplayAlerts = true;
                releaseObject(m_workbook);
                releaseObject(m_application);
            }
        }
        //Static/////////////////////////////////////////
    }
}
