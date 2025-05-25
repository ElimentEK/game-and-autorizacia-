using System;
using System.Collections.Generic;
using System.Text;
using SQLite;


namespace autorizacia.Models
{
    [Table("SavedUsers")]
    public class SavedUser
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime SaveDate { get; set; }
    }
}
