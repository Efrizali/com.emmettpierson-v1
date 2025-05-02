using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace EmmettPierson.com.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(30)]
        public string FirstName { get; set; }

        [MaxLength(30)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        public double PositiveInterest { get; set; }

        public double NegativeInterest { get; set; }

        [MaxLength(50)]
        public string Salt { get; set; }

        [MaxLength(200)]
        public string Hash { get; set; }

        [MaxLength(100)]
        public string Password { get; set; } // Awful no good practice that I will never actually implement I just don't trust myself to remember their passwords

        public Account()
        {
        }

        public Account(string firstName, string lastName, string email, string password, double positiveInterest, double negativeInterest) 
        {
            Id = 0;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Salt = GetNewSalt();
            Hash = GetHash(Salt, password);
            Password = password; // Amazingly secure, I know
            PositiveInterest = positiveInterest;
            NegativeInterest = negativeInterest;
        }

        public void ChangePassword(string newPassword)
        {
            Password = newPassword;

            Hash = GetHash(Salt, newPassword);
        }

        public static string GetHash(string salt, string password)
        {
            StringBuilder output = new StringBuilder();

            using (var hasher = SHA256.Create()) 
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hasher.ComputeHash(enc.GetBytes(salt + password));

                foreach (byte b in result) 
                    output.Append(b.ToString("x2"));
            }

            return output.ToString();
        }

        public static string GetNewSalt()
        {
            char[] chars = new char[70];
            chars = " $.,!%^*abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

            int delta = 50-30;
            int rSize = 30;

            byte[] data = goodRandom(30);

            rSize += delta % (int)((data.Sum(x => (int)x) / 512) + 1);

            StringBuilder result = new StringBuilder(rSize);

            byte[] variableLengthData = goodRandom(rSize);

            foreach (byte b in variableLengthData)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        private static byte[] goodRandom(int arrayLength)
        {
            RandomNumberGenerator crypto = RandomNumberGenerator.Create();
            byte[] data = new byte[arrayLength];
            crypto.GetNonZeroBytes(data);
            return data;
        }
    }
}
