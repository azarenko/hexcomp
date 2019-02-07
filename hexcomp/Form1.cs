using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace hexcomp
{
    public partial class Form1 : Form
    {
        DataTable table = new DataTable();

        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < 16; i++)
            {
                DataColumn column = new DataColumn(i.ToString("X2"));
                table.Columns.Add(column);
            }

            grid.DataSource = table;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileList.Text += openFileDialog1.FileName + "\r\n";
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            FileList.Clear();
        }

        private void Compare_Click(object sender, EventArgs e)
        {
            string[] files = FileList.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<FileStream> streams = new List<FileStream>(files.Length);

            foreach(string filepath in files)
            {
                if (File.Exists(filepath))
                {
                    FileStream fs = new FileStream(filepath, FileMode.Open);
                    streams.Add(fs);
                }
            }

            table.Clear();

            byte[] currentBytes = new byte[streams.Count];
            string[] currentConvertedBytes = new string[streams.Count];
            List<int> shouldMarked = new List<int>();            
            
            do
            {
                DataRow row = table.NewRow();
                shouldMarked.Clear();

                for (int i = 0; i < 16; i++)
                {
                    for (int streamIndex = 0; streamIndex < streams.Count; streamIndex++)
                    {
                        if (streams[streamIndex].Position == streams[streamIndex].Length)
                        {
                            goto endofcycle;
                        }

                        currentBytes[streamIndex] = (byte)streams[streamIndex].ReadByte();
                        currentConvertedBytes[streamIndex] = currentBytes[streamIndex].ToString("X2");
                    }

                    for (int j = 1; j < currentBytes.Length; j++)
                    {
                        if (currentBytes[j - 1] != currentBytes[j])
                        {
                            shouldMarked.Add(i);
                            break;
                        }
                    }

                    row[i] = string.Join(" ", currentConvertedBytes);                                        
                }                        

                table.Rows.Add(row);
                DataGridViewRow dgvr = grid.Rows[table.Rows.Count - 1];
                dgvr.HeaderCell.Value = ((table.Rows.Count - 1) * 16).ToString("X6");

                foreach (int cellIndex in shouldMarked)
                {
                    DataGridViewCell cell = dgvr.Cells[cellIndex];
                    cell.Style.ForeColor = Color.Blue;
                }

                continue;

                endofcycle:
                break;
            }
            while(true);

            foreach (FileStream fs in streams)
            {
                fs.Close();
                fs.Dispose();
            }
        }
    }
}
