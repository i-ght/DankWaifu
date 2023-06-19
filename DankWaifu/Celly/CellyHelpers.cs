using DankWaifu.Collections;
using DankWaifu.Sys;
using System.Text;

namespace DankWaifu.Celly
{
    public static class CellyHelpers
    {
        private static readonly char[] Numbers;
        private static readonly CellyCarrierInfo[] CarrierInfo;

        static CellyHelpers()
        {
            Numbers = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            CarrierInfo = new[]
            {
                new CellyCarrierInfo("Verizon Wireless", "310", "004"),
                new CellyCarrierInfo("Verizon Wireless", "310", "005"),
                new CellyCarrierInfo("Verizon Wireless", "310", "006"),
                new CellyCarrierInfo("Verizon Wireless", "310", "012"),
                new CellyCarrierInfo("AT&T", "310", "030"),
                new CellyCarrierInfo("AT&T", "310", "070"),
                new CellyCarrierInfo("AT&T", "310", "080"),
                new CellyCarrierInfo("AT&T", "311", "090"),
                new CellyCarrierInfo("T-Mobile", "310", "160"),
                new CellyCarrierInfo("T-Mobile", "310", "260"),
                new CellyCarrierInfo("Sprint", "311", "490")
            };
        }

        /// <summary>
        /// Generates a random IMSI number with the specified mobile country code and mobile network code prepended
        /// </summary>
        /// <param name="mccMnc"></param>
        /// <returns></returns>
        public static string RandomIMSI(string mccMnc)
        {
            return $"{mccMnc}{StringHelpers.RandomString(9, StringDefinition.Digits)}";
        }

        /// <summary>
        /// Returns a randomly selected celly carrier info
        /// </summary>
        /// <returns></returns>
        public static CellyCarrierInfo RandomCellyCarrierInfo()
        {
            return CarrierInfo[RandomHelpers.RandomInt(CarrierInfo.Length)];
        }

        public static string GetLuhnCheckDigit(string number)
        {
            var sum = 0;
            var alt = true;
            var digits = number.ToCharArray();
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var curDigit = (digits[i] - 48);
                if (alt)
                {
                    curDigit *= 2;
                    if (curDigit > 9)
                        curDigit -= 9;
                }
                sum += curDigit;
                alt = !alt;
            }
            if ((sum % 10) == 0)
            {
                return "0";
            }
            var ret = (10 - (sum % 10)).ToString();
            return ret;
        }

        public static bool PassesTheLuhnCheck(string number)
        {
            var total = 0;
            var alt = false;
            var digits = number.ToCharArray();
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                var curDigit = (int)char.GetNumericValue(digits[i]);
                if (alt)
                {
                    curDigit *= 2;
                    if (curDigit > 9)
                        curDigit -= 9;
                }
                total += curDigit;
                alt = !alt;
            }
            return total % 10 == 0;
        }

        public static string RandomIccid(CellyCarrierInfo carrierInfo)
        {
            const int telecomIndustryId = 89;
            const int usCountryCode = 1;

            var sb = new StringBuilder();
            sb.Append(telecomIndustryId);
            sb.Append(usCountryCode);
            sb.Append(carrierInfo.MNC);
            sb.Append(StringHelpers.RandomString(16, StringDefinition.Digits));
            sb.Append(GetLuhnCheckDigit(sb.ToString()));
            return sb.ToString();
        }

        /// <summary>
        /// Generates a random valid Imei identifier
        /// </summary>
        /// <returns></returns>
        public static string RandomImei()
        {
            var charArray = new char[15];
            var sum = 0;
            const int len = 15;

            // Fill in the first two values of the string based with the specified prefix.
            // Reporting Body Identifier list: http://en.wikipedia.org/wiki/Reporting_Body_Identifier
            //

            var rbi = new[] { "01", "10", "30", "33", "35", "44", "45", "49", "50", "51", "52", "53", "54", "86", "91", "98", "99" };
            var arr = rbi[RandomHelpers.RandomInt(rbi.Length)].ToCharArray();
            charArray[0] = arr[0];
            charArray[1] = arr[1];
            var pos = 2;

            //
            // Fill all the remaining numbers except for the last one with random values.
            //

            while (pos < len - 1)
            {
                charArray[pos++] = Numbers.RandomSelection();
            }

            //
            // Calculate the Luhn checksum of the values thus far.
            //

            var lenOffset = (len + 1) % 2;
            for (pos = 0; pos < len - 1; pos++)
            {
                var z = (pos + lenOffset) % 2;
                if (z == 1)
                {
                    var t = charArray[pos] * 2;
                    if (t > 9)
                    {
                        t -= 9;
                    }
                    sum += t;
                }
                else
                {
                    sum += charArray[pos];
                }
            }

            //
            // Choose the last digit so that it causes the entire string to pass the checksum.
            //

            var finalDigit = (10 - (sum % 10)) % 10;
            charArray[len - 1] = finalDigit.ToString().ToCharArray()[0];

            // Output the IMEI value.
            //t = str.join('');
            //t = t.substr(0, len);

            return new string(charArray, 0, len);
        }
    }
}