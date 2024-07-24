using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kryptografie_ukoly
{
    internal class vigenere_cipher
    {
        public string keyword { get; set; }
        private shift_cipher[] shifts;

        public vigenere_cipher(string keyword)
        {
            foreach (char c in keyword)
                if (!char.IsLower(c))
                {
                    Console.WriteLine("Keyword has been transformed to lower case characters.");
                    break;
                }

            this.keyword = keyword.ToLower();
            shifts = new shift_cipher[keyword.Length];
            for (int k = 0; k < keyword.Length; k++)
            {
                shifts[k] = new shift_cipher(keyword[k] - 'a');
            }
        }

        public string cipher(string plain_text)
        {
            var res = new StringBuilder(plain_text.Length);
            int counter = 0;

            foreach (char c in plain_text)
            {
                res.Append(shifts[counter].cipher(c.ToString()));

                counter++;
                if (counter >= keyword.Length)
                    counter %= keyword.Length;
            }

            return res.ToString();
        }

        public string decipher(string cipher_text)
        {
            var res = new StringBuilder(cipher_text.Length);
            int counter = 0;

            foreach (char c in cipher_text)
            {
                res.Append(shifts[counter].decipher(c.ToString()));

                counter++;
                if (counter >= keyword.Length)
                    counter %= keyword.Length;
            }

            return res.ToString();
        }

    }
}
