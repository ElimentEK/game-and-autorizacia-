using autorizacia.Models;
using SQLite;
using System;
using System.IO;
using System.Linq;

namespace autorizacia
{
    public class DatabaseService
    {
        private readonly SQLiteConnection _db;

        public DatabaseService()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "users.db3");
            _db = new SQLiteConnection(dbPath);
            _db.CreateTable<User>();
            _db.CreateTable<SavedUser>();
        }

        public void SaveUser(User user)
        {
            // Проверяем, существует ли пользователь с таким email
            var existingUser = _db.Table<User>().FirstOrDefault(u => u.Email == user.Email);
            if (existingUser == null)
            {
                _db.Insert(user);
            }
            else
            {
                // Обновляем существующего пользователя
                user.Id = existingUser.Id; // Предполагается, что у User есть поле Id
                _db.Update(user);
            }
        }

        public User GetUser(string email, string password)
        {
            return _db.Table<User>().FirstOrDefault(u => u.Email == email && u.Password == password);
        }

        public bool UserExists(string email)
        {
            return _db.Table<User>().Any(u => u.Email == email);
        }

        public void SaveUserCredentials(string email, string password)
        {
            // Удаляем все сохраненные данные пользователей
            _db.DeleteAll<SavedUser>();

            var savedUser = new SavedUser
            {
                Email = email,
                Password = password,
                SaveDate = DateTime.Now
            };

            _db.Insert(savedUser);
        }

        public SavedUser GetSavedUser()
        {
            return _db.Table<SavedUser>().FirstOrDefault();
        }

        public void ClearSavedUser()
        {
            _db.DeleteAll<SavedUser>();
        }
    }
}
