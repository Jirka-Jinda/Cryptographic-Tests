using System.Text;

namespace Kryptografie_ukoly
{
    public class shift_cipher
    {
        public int shift { get; set; }
        private char[] alphabet_upper = Enumerable.Range('A', 26).Select(x => (char)x).ToArray();
        private char[] alphabet_lower = Enumerable.Range('a', 26).Select(x => (char)x).ToArray();

        public shift_cipher(int shift)
        {
            this.shift = shift;
        }

        public string cipher(string plain_text)
        {
            var res = new StringBuilder(plain_text.Length);
            foreach (char letter in plain_text)
            {
                if (char.IsAsciiLetterOrDigit(letter))
                {
                    if (char.IsLower(letter))
                    {
                        int index = ((letter - 'a') + shift) % alphabet_lower.Length;
                        res.Append(alphabet_lower[index]);
                    }

                    else
                        res.Append(alphabet_upper[((letter - 'A') + shift) % alphabet_upper.Length]);
                }
                else if (char.IsWhiteSpace(letter))
                    res.Append(letter);
                else
                    throw new Exception("Please use only ascii letters.");
            }
            return res.ToString();
        }

        public string decipher(string cipher_text)
        {
            int prev_shift = shift;
            shift = int.Abs(26 - shift) % 26;
            string res = cipher(cipher_text);
            shift = prev_shift;
            return res;
        }
    }
}
