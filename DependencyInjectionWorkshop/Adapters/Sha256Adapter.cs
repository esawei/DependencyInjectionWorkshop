using System.Text;

namespace DependencyInjectionWorkshop.Adapters
{
    public class Sha256Adapter
    {
        public string HashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hashedInputPassword = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hashedInputPassword.Append(theByte.ToString("x2"));
            }

            return hashedInputPassword.ToString();
        }
    }
}