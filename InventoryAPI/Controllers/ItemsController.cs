// File: ItemsController.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: handles all inventory item operations
//              uses parameterized queries to prevent sql injection (Week 3)

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using InventoryAPI.Models;

namespace InventoryAPI.Controllers
{
    // this controller handles all inventory item endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        // database helper to connect to sql server
        private readonly DatabaseHelper _db;

        // constructor - gets the database helper from dependency injection
        public ItemsController(DatabaseHelper db)
        {
            // store the database helper
            _db = db;
        }

        // gets all items or searches by name or category
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string? name, [FromQuery] string? category)
        {
            // list to store all found items
            List<Item> items = new List<Item>();

            // connect to the database
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // build query with parameterized search (Week 3)
                string query = "SELECT item_id, item_name, category, quantity, description, added_by, created_at FROM Items WHERE 1=1";

                // add name filter if provided
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query += " AND item_name LIKE @Name";
                }

                // add category filter if provided
                if (!string.IsNullOrWhiteSpace(category))
                {
                    query += " AND category = @Category";
                }

                // run the query
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add name parameter safely
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        command.Parameters.AddWithValue("@Name", "%" + name + "%");
                    }

                    // add category parameter safely
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        command.Parameters.AddWithValue("@Category", category);
                    }

                    // read all the results
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // build each item from the database result
                            items.Add(new Item
                            {
                                ItemId = (int)reader["item_id"],
                                ItemName = reader["item_name"].ToString() ?? "",
                                Category = reader["category"].ToString() ?? "",
                                Quantity = (int)reader["quantity"],
                                Description = reader["description"].ToString() ?? "",
                                AddedBy = (int)reader["added_by"],
                                CreatedAt = (DateTime)reader["created_at"]
                            });
                        }
                    }
                }
            }

            // return the list of items
            return Ok(items);
        }

        // adds a new inventory item to the database
        [HttpPost("add")]
        public IActionResult AddItem([FromBody] ItemRequest request, [FromQuery] int userId)
        {
            // check if required fields are empty
            if (string.IsNullOrWhiteSpace(request.ItemName) ||
                string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest(new { message = "Item name and category are required" });
            }

            // check if quantity is valid
            if (request.Quantity < 0)
            {
                return BadRequest(new { message = "Quantity cannot be negative" });
            }

            // connect to the database and insert the item
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "INSERT INTO Items (item_name, category, quantity, description, added_by) VALUES (@ItemName, @Category, @Quantity, @Description, @AddedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add all parameters safely
                    command.Parameters.AddWithValue("@ItemName", request.ItemName);
                    command.Parameters.AddWithValue("@Category", request.Category);
                    command.Parameters.AddWithValue("@Quantity", request.Quantity);
                    command.Parameters.AddWithValue("@Description", request.Description);
                    command.Parameters.AddWithValue("@AddedBy", userId);

                    // run the insert command
                    command.ExecuteNonQuery();
                }
            }

            // return success message
            return Ok(new { message = "Item added successfully" });
        }

        // updates an existing inventory item
        [HttpPut("update/{itemId}")]
        public IActionResult UpdateItem(int itemId, [FromBody] ItemRequest request)
        {
            // check if required fields are empty
            if (string.IsNullOrWhiteSpace(request.ItemName) ||
                string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest(new { message = "Item name and category are required" });
            }

            // connect to the database and update the item
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "UPDATE Items SET item_name = @ItemName, category = @Category, quantity = @Quantity, description = @Description WHERE item_id = @ItemId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add all parameters safely
                    command.Parameters.AddWithValue("@ItemName", request.ItemName);
                    command.Parameters.AddWithValue("@Category", request.Category);
                    command.Parameters.AddWithValue("@Quantity", request.Quantity);
                    command.Parameters.AddWithValue("@Description", request.Description);
                    command.Parameters.AddWithValue("@ItemId", itemId);

                    // run the update command
                    command.ExecuteNonQuery();
                }
            }

            // return success message
            return Ok(new { message = "Item updated successfully" });
        }

        // deletes an item, only admins can do this
        [HttpDelete("delete/{itemId}")]
        public IActionResult DeleteItem(int itemId, [FromQuery] string role)
        {
            // check if the user is an admin (Week 4 - authorization)
            if (role != "admin")
            {
                return Unauthorized(new { message = "Only admins can delete items" });
            }

            // connect to the database and delete the item
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "DELETE FROM Items WHERE item_id = @ItemId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add the item id parameter safely
                    command.Parameters.AddWithValue("@ItemId", itemId);

                    // run the delete command
                    command.ExecuteNonQuery();
                }
            }

            // return success message
            return Ok(new { message = "Item deleted successfully" });
        }
    }
}