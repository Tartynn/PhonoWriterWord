using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Models
{
    public class Definition
    {
        private int id;
        private string text;
        private int isUpdated;

        public Definition()
        {
            id = 0;
            text = "";
            isUpdated = 0;
        }

        public Definition(string text)
        {
            this.text = text;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public bool IsUpdated
        {
            get { return isUpdated == 1; }
            set { isUpdated = value ? 1 : 0; }
        }
    }
}
