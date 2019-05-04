using System.Text;

namespace DependencyInjectionWorkshop.Adapters
{
    public interface IHash
    {
        string GetHash(string password);
    }

    public class Sha256Adapter : IHash
    {
        public string GetHash(string password)
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