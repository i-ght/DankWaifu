using DankWaifu.Sys;
using System;

namespace DankWaifu.Celly
{
    public struct CellyCarrierInfo
    {
        public CellyCarrierInfo(string carrierName, string mcc, string mnc)
        {
            CarrierName = carrierName ?? throw new ArgumentNullException(nameof(carrierName));
            MCC = mcc;
            MNC = mnc;
        }

        public CellyCarrierInfo(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrWhiteSpace(input) || !input.Contains("~"))
                throw new ArgumentException($"{nameof(input)} is whitespace or did not contain split char '~'", nameof(input));

            var split = input.Split('~');
            if (split.Length != 3)
                throw new InvalidOperationException("split has invalid length");

            CarrierName = split[0];
            MCC = split[1];
            MNC = split[2];
        }

        /// <summary>
        /// Name of the celly company
        /// </summary>
        public string CarrierName { get; }

        /// <summary>
        /// Mobile country code
        /// </summary>
        public string MCC { get; }

        /// <summary>
        /// Mobile network code
        /// </summary>
        public string MNC { get; }

        /// <summary>
        /// String representation of mobile country code and mobile network code
        /// </summary>
        /// <returns></returns>
        public string MCCMNC(bool withDash = false)
        {
            if (withDash)
                return $"{MCC}-{MNC}";

            return $"{MCC}{MNC}";
        }

        /// <summary>
        /// Returns a random IMSI number based on specified carrier info in this
        /// </summary>
        /// <returns></returns>
        public string RandomIMSI()
        {
            return CellyHelpers.RandomIMSI(MCCMNC());
        }

        public override string ToString()
        {
            return $"{CarrierName}~{MCC}~{MNC}";
        }

        public static bool TryParse(string input, out CellyCarrierInfo carrierInfo)
        {
            carrierInfo = default(CellyCarrierInfo);
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var split = input.Split('~');
            if (split.Length != 3)
                return false;

            var name = split[0];
            var mcc = split[1];
            var mnc = split[2];
            if (StringHelpers.AnyNullOrEmpty(name, mcc, mnc))
                return false;

            carrierInfo = new CellyCarrierInfo(name, mcc, mnc);
            return true;
        }
    }
}