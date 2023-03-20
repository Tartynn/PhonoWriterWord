using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Models
{
    public class Image
    {
        private int id;
        private string fileName;
        private int isUpdated;

        public Image()
        {
            id = 0;
            fileName = "";
            isUpdated = 0;
        }

        public Image(string fileName)
        {
            isUpdated = 0;
            this.fileName = fileName;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public bool IsUpdated
        {
            get { return isUpdated == 1; }
            set { isUpdated = value ? 1 : 0; }
        }

    }
}
