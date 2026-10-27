
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CampusLostFound.Models
{
    public class ItemRepository
    {
        private readonly string connectionString;

        public ItemRepository()
        {
            connectionString = ConfigurationManager
                .ConnectionStrings["CampusLostFoundConnection"]
                .ConnectionString;
        }

        // Get all items
        public List<Item> GetAll(string search = "", string category = "")
        {
            List<Item> items = new List<Item>();

            string sql = @"SELECT * FROM Items
                           WHERE (@Search = ''
                           OR ItemName LIKE @Pattern
                           OR Description LIKE @Pattern)
                           AND (@Category = ''
                           OR Category = @Category)
                           ORDER BY ItemId DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100)
                    .Value = search ?? "";

                cmd.Parameters.Add("@Pattern", SqlDbType.NVarChar, 102)
                    .Value = "%" + (search ?? "") + "%";

                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50)
                    .Value = category ?? "";

                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapItem(reader));
                    }
                }
            }

            return items;
        }

        // Get one item by ID
        public Item GetById(int id)
        {
            Item item = null;

            string sql = "SELECT * FROM Items WHERE ItemId = @ItemId";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@ItemId", SqlDbType.Int).Value = id;

                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item = MapItem(reader);
                    }
                }
            }

            return item;
        }

        // Insert a lost or found report
        public int Insert(Item item)
        {
            string sql = @"
                INSERT INTO Items
                (ItemName, Category, Description, ItemType,
                 ItemDate, Location, ContactName, ContactInfo,
                 ImagePath, Status)
                VALUES
                (@ItemName, @Category, @Description, @ItemType,
                 @ItemDate, @Location, @ContactName, @ContactInfo,
                 @ImagePath, @Status);

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                AddItemParameters(cmd, item);
                con.Open();

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Update an existing report
        public bool Update(Item item)
        {
            string sql = @"
                UPDATE Items SET
                    ItemName = @ItemName,
                    Category = @Category,
                    Description = @Description,
                    ItemType = @ItemType,
                    ItemDate = @ItemDate,
                    Location = @Location,
                    ContactName = @ContactName,
                    ContactInfo = @ContactInfo,
                    ImagePath = @ImagePath,
                    Status = @Status
                WHERE ItemId = @ItemId";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                AddItemParameters(cmd, item);

                cmd.Parameters.Add("@ItemId", SqlDbType.Int)
                    .Value = item.ItemId;

                con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Delete a report
        public bool Delete(int id)
        {
            string sql = "DELETE FROM Items WHERE ItemId = @ItemId";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@ItemId", SqlDbType.Int).Value = id;

                con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Demonstrate DataAdapter and DataTable
        public DataTable GetAllAsDataTable()
        {
            DataTable table = new DataTable();

            string sql = "SELECT * FROM Items ORDER BY ItemId DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, con))
            {
                DataSet dataSet = new DataSet();

                adapter.Fill(dataSet, "Items");

                table = dataSet.Tables["Items"];
            }

            return table;
        }

        private void AddItemParameters(SqlCommand cmd, Item item)
        {
            cmd.Parameters.Add("@ItemName", SqlDbType.NVarChar, 100)
                .Value = item.ItemName;

            cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50)
                .Value = item.Category;

            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500)
                .Value = (object)item.Description ?? DBNull.Value;

            cmd.Parameters.Add("@ItemType", SqlDbType.NVarChar, 10)
                .Value = item.ItemType;

            cmd.Parameters.Add("@ItemDate", SqlDbType.Date)
                .Value = item.ItemDate.Date;

            cmd.Parameters.Add("@Location", SqlDbType.NVarChar, 150)
                .Value = item.Location;

            cmd.Parameters.Add("@ContactName", SqlDbType.NVarChar, 100)
                .Value = item.ContactName;

            cmd.Parameters.Add("@ContactInfo", SqlDbType.NVarChar, 100)
                .Value = item.ContactInfo;

            cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 255)
                .Value = (object)item.ImagePath ?? DBNull.Value;

            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20)
                .Value = string.IsNullOrWhiteSpace(item.Status)
                    ? "Open" : item.Status;
        }

        private Item MapItem(SqlDataReader reader)
        {
            return new Item
            {
                ItemId = Convert.ToInt32(reader["ItemId"]),
                ItemName = Convert.ToString(reader["ItemName"]),
                Category = Convert.ToString(reader["Category"]),
                Description = reader["Description"] == DBNull.Value
                    ? "" : Convert.ToString(reader["Description"]),
                ItemType = Convert.ToString(reader["ItemType"]),
                ItemDate = Convert.ToDateTime(reader["ItemDate"]),
                Location = Convert.ToString(reader["Location"]),
                ContactName = Convert.ToString(reader["ContactName"]),
                ContactInfo = Convert.ToString(reader["ContactInfo"]),
                ImagePath = reader["ImagePath"] == DBNull.Value
                    ? "" : Convert.ToString(reader["ImagePath"]),
                Status = Convert.ToString(reader["Status"])
            };
        }
    }
}
