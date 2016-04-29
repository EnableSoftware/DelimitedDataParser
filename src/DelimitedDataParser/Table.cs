﻿using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DelimitedDataParser
{
    internal class Table
    {
        private readonly IList<string[]> _rows = new List<string[]>();

        private StringBuilder _currentCell = new StringBuilder();
        private IList<string> _currentRow = new List<string>();
        private bool _useFirstRowAsColumnHeaders = true;

        public virtual bool UseFirstRowAsColumnHeaders
        {
            get
            {
                return _useFirstRowAsColumnHeaders;
            }

            set
            {
                _useFirstRowAsColumnHeaders = value;
            }
        }

        public virtual void AddToCurrentCell(char c)
        {
            _currentCell.Append(c);
        }

        public virtual void AddToCurrentCell(char value, int repeatCount)
        {
            _currentCell.Append(value, repeatCount);
        }

        public virtual void FlushCell()
        {
            _currentRow.Add(_currentCell.ToString());
            _currentCell = new StringBuilder();
        }

        public virtual void FlushRow()
        {
            _rows.Add(_currentRow.ToArray());
            _currentRow = new List<string>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual DataTable ToDataTable()
        {
            FlushCell();
            FlushRow();

            var table = new DataTable
            {
                Locale = CultureInfo.CurrentCulture
            };

            if (_rows.Count == 0)
            {
                return table;
            }

            var colCount = _rows.Count > 0 ? _rows.Max(r => r.Length) : 0;

            var usedColumnHeaders = new List<string>();

            for (int i = 0; i < colCount; i++)
            {
                if (_useFirstRowAsColumnHeaders && i < _rows[0].Length && !usedColumnHeaders.Any(h => h == _rows[0][i]))
                {
                    table.Columns.Add(_rows[0][i]);
                    usedColumnHeaders.Add(_rows[0][i]);
                }
                else
                {
                    table.Columns.Add();
                }
            }

            if (_useFirstRowAsColumnHeaders && _rows.Count == 1)
            {
                return table;
            }

            if (_useFirstRowAsColumnHeaders)
            {
                _rows.RemoveAt(0);
            }

            while (_rows.Count > 0 && !_rows[_rows.Count - 1].Any(c => !string.IsNullOrEmpty(c)))
            {
                _rows.RemoveAt(_rows.Count - 1);
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                table.Rows.Add(_rows[i]);
            }

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    if (row.IsNull(col))
                    {
                        row[col] = string.Empty;
                    }
                }
            }

            return table;
        }
    }
}