// ReSharper disable InconsistentNaming

using System;

namespace DankWaifu.Android
{
    public enum DpiType
    {
        LDPI = 120,
        MDPI = 160,
        HDPI = 240,
        XHDPI = 320,
        XXHDPI = 480,
        XXXHDPI = 640
    }

    public struct DpiInfo
    {
        public DpiInfo(string dpiType)
        {
            if (dpiType == null)
                throw new ArgumentNullException(nameof(dpiType));

            if (string.IsNullOrWhiteSpace(dpiType))
                throw new ArgumentException($"{nameof(dpiType)} is whitespace", nameof(dpiType));

            if (dpiType.Contains("~"))
                dpiType = dpiType.Split('~')[0];

            DpiType tmp;
            if (!Enum.TryParse(dpiType, true, out tmp))
                switch (dpiType)
                {
                    case "LowDpi":
                        tmp = DpiType.LDPI;
                        break;

                    case "MediumDpi":
                        tmp = DpiType.MDPI;
                        break;

                    case "HighDpi":
                        tmp = DpiType.HDPI;
                        break;

                    case "XHighDpi":
                        tmp = DpiType.XHDPI;
                        break;

                    case "XXHighDpi":
                        tmp = DpiType.XXHDPI;
                        break;

                    case "XXXHighDpi":
                        tmp = DpiType.XXXHDPI;
                        break;

                    default:
                        throw new InvalidOperationException("failed to parse DpiType");
                }

            DpiType = tmp;
            Dpi = (int)DpiType;
            SizeScale = 0.0f;
            SetSizeScale();
        }

        public DpiInfo(DpiType dpiType)
        {
            DpiType = dpiType;
            Dpi = (int)DpiType;
            SizeScale = 0.0f;
            SetSizeScale();
        }

        private void SetSizeScale()
        {
            switch (DpiType)
            {
                case DpiType.LDPI:
                    SizeScale = 0.75f;
                    break;

                case DpiType.MDPI:
                    SizeScale = 1.0f;
                    break;

                case DpiType.HDPI:
                    SizeScale = 1.5f;
                    break;

                case DpiType.XHDPI:
                    SizeScale = 2.0f;
                    break;

                case DpiType.XXHDPI:
                    SizeScale = 3.0f;
                    break;

                case DpiType.XXXHDPI:
                    SizeScale = 4.0f;
                    break;

                default:
                    SizeScale = 2.0f;
                    break;
            }
        }

        public DpiType DpiType { get; }
        public int Dpi { get; }
        public float SizeScale { get; private set; }

        public override string ToString()
        {
            return $"{DpiType}~{Dpi}~{SizeScale}";
        }

        public static bool TryParse(string input, out DpiInfo dpiInfo)
        {
            dpiInfo = default(DpiInfo);
            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                dpiInfo = new DpiInfo(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}