using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace PhonoWriterWord.Database.Controllers
{
    public class ImagesController : AbstractController
    {
        public static string createQuery = "INSERT INTO image (file_name, is_updated) VALUES (:fileName, :isUpdated)";
        public static string researchQuery = "SELECT id, file_name, is_updated FROM image WHERE id=:id";
        public static string researchLastImage = "SELECT id, file_name, is_updated FROM image WHERE id = (SELECT MAX(id) FROM image)";
        public static string researchAllImagesQuery = "SELECT id, file_name, is_updated FROM image";
        public static string researchByWordQuery = "SELECT i.id, i.file_name, i.is_updated FROM image i JOIN word w ON w.image_id=i.id WHERE w.id=:wordId";
        public static string researchByEmptyWord = "SELECT i.id, i.file_name, i.is_updated FROM image i LEFT JOIN word w ON i.id=w.image_id WHERE w.image_id IS NULL AND i.file_name LIKE :fileName ORDER BY i.id DESC  LIMIT 12 OFFSET :endNumberImage";
        public static string researchJoinWords = "SELECT i.id, i.file_name, i.is_updated FROM image i JOIN word w on w.image_id=i.id ORDER BY image.is_updated DESC";
        public static string updateQuery = "UPDATE image SET file_name:=fileName, is_updated=:isUpdated WHERE id=:id";
        public static string deleteQuery = "DELETE FROM image WHERE id=:id";
        public static string purgeImagesQuery = "DELETE FROM image WHERE is_updated=1";

        public ImagesController(DatabaseController controller) : base(controller)
        {
        }

        public int Create(Image image)
        {
            // Exit if image is null.
            if (image == null)
                return -1;

            return (int)DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter paramFileName = new SQLiteParameter("fileName", image.FileName);
                SQLiteParameter paramIsUpdated = new SQLiteParameter("isUpdated", Convert.ToInt32(image.IsUpdated));

                //paramFileName.Value = image.FileName;
                //paramIsUpdated.Value = Convert.ToInt32(image.IsUpdated);

                command.CommandText = ImagesController.createQuery;
                command.Parameters.Add(paramFileName);
                command.Parameters.Add(paramIsUpdated);

                command.ExecuteNonQuery();

                // Return the new ID.
                command.CommandText = "SELECT last_insert_rowid()";
                int.TryParse(command.ExecuteScalar().ToString(), out int id);

                return id;
            });
        }

        public void Delete(int id)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter param = new SQLiteParameter("id", id);

                //param.Value = id;

                command.CommandText = ImagesController.deleteQuery;
                command.Parameters.Add(param);

                command.ExecuteNonQuery();
            });
        }

        public Image Research(Int32 id)
        {
            if (id == 0)
                return null;

            Image image = null;

            return (Image)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter param = new SQLiteParameter("id", id);

                //param.Value = id;
                command.CommandText = ImagesController.researchQuery;
                command.Parameters.Add(param);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                try
                {
                    DataRow dr = dt.Rows[0];
                    if (dr != null)
                    {
                        image = new Image();
                        image.Id = Convert.ToInt32(dr.ItemArray[0]);
                        image.FileName = Convert.ToString(dr.ItemArray[1]);
                        image.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                    }
                    else
                    {
                        throw new ImageNotFoundException("Image not found");
                    }
                }
                catch
                {
                    Console.WriteLine("IMAGE FAIL : " + id);
                    return null;
                }

                return image;
            });
        }

        public Image ResearchByWord(Word word)
        {
            return (Image)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter param = new SQLiteParameter("wordId", word.Id);

                //param.Value = word.Id;
                command.CommandText = ImagesController.researchByWordQuery;
                command.Parameters.Add(param);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Image image = null;

                if (dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];

                    if (dr != null)
                    {
                        image = new Image();
                        image.Id = Convert.ToInt32(dr.ItemArray[0]);
                        image.FileName = Convert.ToString(dr.ItemArray[1]);
                        image.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);

                    }
                    else
                    {
                        throw new ImageNotFoundException("Image not found");
                    }
                }

                return image;
            });
        }

        public List<Image> ResearchAllImages()
        {
            return (List<Image>)DatabaseController.DoCommand((command) =>
            {
                command.CommandText = researchAllImagesQuery;

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Image> images = new List<Image>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    var image = new Image();
                    image.Id = Convert.ToInt32(dr.ItemArray[0]);
                    image.FileName = Convert.ToString(dr.ItemArray[1]);
                    image.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                    images.Add(image);
                }

                return images;
            });
        }

        public List<Image> ResearchByEmptyWord(int sectionImage, string textSearch)
        {
            return (List<Image>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramSectionImage = new SQLiteParameter("endNumberImage", sectionImage);
                SQLiteParameter paramtextSearch = new SQLiteParameter("fileName", "%" + textSearch + "%");

                //paramSectionImage.Value = sectionImage;
                //paramtextSearch.Value = "%" + textSearch + "%";

                command.CommandText = ImagesController.researchByEmptyWord;

                command.Parameters.Add(paramtextSearch);
                command.Parameters.Add(paramSectionImage);

                Image image;
                List<Image> lImg = new List<Image>();
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    if (dr != null)
                    {
                        image = new Image();
                        image.Id = Convert.ToInt32(dr.ItemArray[0]);
                        image.FileName = Convert.ToString(dr.ItemArray[1]);
                        image.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                        lImg.Add(image);
                    }
                }

                return lImg;
            });
        }

        public List<Image> ResearchJoinWords()
        {
            return (List<Image>)DatabaseController.DoCommand((command) =>
            {
                command.CommandText = researchJoinWords;

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Image> lImg = new List<Image>();
                Image image;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    if (dr != null)
                    {
                        image = new Image();
                        image.Id = Convert.ToInt32(dr.ItemArray[0]);
                        image.FileName = Convert.ToString(dr.ItemArray[1]);
                        image.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                        lImg.Add(image);
                    }
                }

                return lImg;
            });
        }

        public Image ResearchLastImage()
        {
            return (Image)DatabaseController.DoCommand((command) =>
            {
                command.CommandText = ImagesController.researchLastImage;

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Image image = null;

                try
                {
                    DataRow dr = dt.Rows[0];
                    if (dr != null)
                    {
                        image = new Image();
                        image.Id = Convert.ToInt32(dr.ItemArray[0]);
                        image.FileName = Convert.ToString(dr.ItemArray[1]);
                        image.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                    }
                    else
                    {
                        throw new ImageNotFoundException("Image not found");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ResearchLastImage::Exception catched : " + e.StackTrace);
                }

                return image;
            });
        }

        public void Update(Image image)
        {

        }

        public void PurgeImages()
        {

        }

        public List<int> Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}
