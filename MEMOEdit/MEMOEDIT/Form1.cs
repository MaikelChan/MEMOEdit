using System;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace MEMOEDIT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Memo

        MemoData MemoD;
        MemoFlag MemoF;

        string MemoDFile;
        string MemoFFile;

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ODlg = new OpenFileDialog();
            ODlg.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            ODlg.Title = "Select MemoData file";
            if (ODlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            MemoDFile = ODlg.FileName;

            ODlg = new OpenFileDialog();
            ODlg.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            ODlg.Title = "Select MemoFlag file";
            if (ODlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            MemoFFile = ODlg.FileName;

            MemoD = new MemoData(MemoDFile, Path.Combine(Application.StartupPath, "table.txt"));
            MemoF = new MemoFlag(MemoFFile);

            numericUpDown1.Maximum = MemoD.PageCount - 2;
            if (numericUpDown1.Value == 0)
                ShowTexts(0);
            else
                numericUpDown1.Value = 0;

            EnableControls(true);
        }

        void ShowTexts(int Index)
        {
            string[] Texts = MemoD.GetPageText(Index);
            UInt16[] Flags = MemoF.GetFlags(Index);

            textBox1.Text = Texts[0];
            textBox2.Text = Texts[1];
            textBox3.Text = Texts[2];
            textBox4.Text = Texts[3];
            textBox5.Text = Texts[4];
            textBox6.Text = Texts[5];
            textBox7.Text = Texts[6];
            textBox8.Text = Texts[7];
            textBox9.Text = Texts[8];
            textBox10.Text = Texts[9];

            textBox11.Text = Flags[0].ToString("X4");
            textBox12.Text = Flags[1].ToString("X4");
            textBox13.Text = Flags[2].ToString("X4");
            textBox14.Text = Flags[3].ToString("X4");
            textBox15.Text = Flags[4].ToString("X4");
            textBox16.Text = Flags[5].ToString("X4");
            textBox17.Text = Flags[6].ToString("X4");
            textBox18.Text = Flags[7].ToString("X4");
            textBox19.Text = Flags[8].ToString("X4");
            textBox20.Text = Flags[9].ToString("X4");
        }

        void SetTexts(int Index)
        {
            string[] Texts = new string[10];
            UInt16[] Flags = new UInt16[10];

            Texts[0] = textBox1.Text;
            Texts[1] = textBox2.Text;
            Texts[2] = textBox3.Text;
            Texts[3] = textBox4.Text;
            Texts[4] = textBox5.Text;
            Texts[5] = textBox6.Text;
            Texts[6] = textBox7.Text;
            Texts[7] = textBox8.Text;
            Texts[8] = textBox9.Text;
            Texts[9] = textBox10.Text;

            Flags[0] = Convert.ToUInt16(textBox11.Text, 16);
            Flags[1] = Convert.ToUInt16(textBox12.Text, 16);
            Flags[2] = Convert.ToUInt16(textBox13.Text, 16);
            Flags[3] = Convert.ToUInt16(textBox14.Text, 16);
            Flags[4] = Convert.ToUInt16(textBox15.Text, 16);
            Flags[5] = Convert.ToUInt16(textBox16.Text, 16);
            Flags[6] = Convert.ToUInt16(textBox17.Text, 16);
            Flags[7] = Convert.ToUInt16(textBox18.Text, 16);
            Flags[8] = Convert.ToUInt16(textBox19.Text, 16);
            Flags[9] = Convert.ToUInt16(textBox20.Text, 16);

            MemoD.SetPageText(Texts, Index);
            MemoF.SetFlags(Flags, Index);
        }

        void EnableControls(bool e)
        {
            numericUpDown1.Enabled = e;
            button2.Enabled = e;
            button3.Enabled = e;
            textBox1.Enabled = e;
            textBox2.Enabled = e;
            textBox3.Enabled = e;
            textBox4.Enabled = e;
            textBox5.Enabled = e;
            textBox6.Enabled = e;
            textBox7.Enabled = e;
            textBox8.Enabled = e;
            textBox9.Enabled = e;
            textBox10.Enabled = e;

            textBox11.Enabled = e;
            textBox12.Enabled = e;
            textBox13.Enabled = e;
            textBox14.Enabled = e;
            textBox15.Enabled = e;
            textBox16.Enabled = e;
            textBox17.Enabled = e;
            textBox18.Enabled = e;
            textBox19.Enabled = e;
            textBox20.Enabled = e;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ShowTexts((int)numericUpDown1.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SetTexts((int)numericUpDown1.Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MemoD.SaveData(MemoDFile);
            MemoF.SaveData(MemoFFile);
        }

        #endregion

        #region Bunki

        Bunki Bunk;
        string BunkiFile;

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ODlg = new OpenFileDialog();
            ODlg.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            ODlg.Title = "Select Bunki file";
            if (ODlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            BunkiFile = ODlg.FileName;

            Bunk = new Bunki(BunkiFile, Path.Combine(Application.StartupPath, "table.txt"));

            EnableBunkiControls(true);

            LoadTexts();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveTexts();
        }

        void EnableBunkiControls(bool e)
        {
            button5.Enabled = e;
            dataGridView1.Enabled = e;
        }

        void LoadTexts()
        {
            string[] Texts = Bunk.GetTexts();

            dataGridView1.Rows.Clear();

            for (int n = 0; n < Texts.Length; n += 2)
            {
                dataGridView1.Rows.Add(Texts[n], Texts[n + 1]);
            }
        }

        void SaveTexts()
        {
            string[] Texts = new string[dataGridView1.Rows.Count << 1];

            for (int n = 0; n < Texts.Length; n += 2)
            {
                Texts[n] = ObjectToString(dataGridView1.Rows[n >> 1].Cells[0].Value);
                Texts[n + 1] = ObjectToString(dataGridView1.Rows[n >> 1].Cells[1].Value);
            }

            Bunk.SetTexts(Texts);
            Bunk.SaveData(BunkiFile);
        }

        string ObjectToString(object obj)
        {
            if (obj == null) return String.Empty;
            else return obj.ToString();
        }

        #endregion
    }
}
