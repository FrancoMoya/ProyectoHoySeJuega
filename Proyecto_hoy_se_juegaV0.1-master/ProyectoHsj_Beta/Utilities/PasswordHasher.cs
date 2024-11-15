using System.Security.Cryptography;
using System.Text;


namespace ProyectoHsj_Beta.Utilities
{
    public class PasswordHasher
    {
        // Método para hashear la contraseña con una sal
        public static string HashPassword(string password)
        {
            // Genera una sal de 16 bytes
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hashea la contraseña con la sal usando SHA256
            using (var sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(Convert.ToBase64String(salt) + password);
                byte[] hashedBytes = sha256.ComputeHash(saltedPassword);

                // Convierte el hash y la sal a una representación de cadena
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }

                // Retorna el hash y la sal concatenados
                return $"{Convert.ToBase64String(salt)}.{builder}";
            }
        }
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Separa la sal y el hash almacenados
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = parts[0];
            var storedPasswordHash = parts[1];

            // Hashea la contraseña ingresada usando la misma sal
            using (var sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(salt + enteredPassword);
                byte[] enteredHashedBytes = sha256.ComputeHash(saltedPassword);

                // Convierte el hash ingresado a una representación de cadena
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < enteredHashedBytes.Length; i++)
                {
                    builder.Append(enteredHashedBytes[i].ToString("x2"));
                }

                // Compara el hash ingresado con el hash almacenado
                return builder.ToString() == storedPasswordHash;
            }
        }
    }
}
