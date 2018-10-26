using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using NSharp.SearchEngine.Lucene.Analysis.Cjk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lucene.Net分词的使用
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 一元分词
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Analyzer analyzer = new PanGuAnalyzer();
            TokenStream tokenStream = analyzer.TokenStream("", new StringReader("面向对象编程"));
            Net.Analysis.Token token = null;
            while((token=tokenStream.Next())!=null)
            {
                Console.WriteLine(token.TermText());
            }
        }

        

      
    }
}
