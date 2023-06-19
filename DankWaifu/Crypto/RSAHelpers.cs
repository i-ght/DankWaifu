//**********************************************************************
//
// pempublic
// .NET 1.1/2.0  PEM SubjectPublicKeyInfo Public Key Reader

/*
Copyright (c)  2006 - 2014   JavaScience Consulting,  Michel Gallant

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

//
//**********************************************************************
//
// pempublic.c
//
// Reads a PEM encoded in b64 format (with or without header/footer
// lines) or binary RSA public key file in SubjectPublicKeyInfo asn.1 format.
// Removes header/footer lines and b64 decodes for b64 case.
// Parses asn.1 encoding to extract exponent and modulus byte[].
// Creates byte[] modulus and byte[] exponent
// Instantiates RSACryptoServiceProvider
//*************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace YandexCreator.Work
{
    public static class RSAHelpers
    {
        // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
        private static readonly byte[] SeqOid;

        static RSAHelpers()
        {
            SeqOid = new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        }

        public static RSAParameters Base64KeyToRsaParameters(string base64)
        {
            var sb = new StringBuilder(base64);
            sb.Replace("-----BEGIN PUBLIC KEY-----", string.Empty);  //remove headers/footers, if present
            sb.Replace("-----END PUBLIC KEY-----", string.Empty);

            var x509Key = Convert.FromBase64String(sb.ToString());

            using (var mem = new MemoryStream(x509Key))
            {
                using (var binr = new BinaryReader(mem))
                {
                    var twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        throw new InvalidOperationException("twobytes had invalid value at RSAHelpers.Base64KeyToRsaParameters");

                    var seq = binr.ReadBytes(15);
                    if (!seq.SequenceEqual(SeqOid))
                        throw new InvalidOperationException("incorrect oid sequence");

                    twobytes = binr.ReadUInt16();
                    switch (twobytes)
                    {
                        case 0x8103:
                            binr.ReadByte();    //advance 1 byte
                            break;

                        case 0x8203:
                            binr.ReadInt16();   //advance 2 bytes
                            break;

                        default:
                            throw new InvalidOperationException("twobytes had invalid value at RSAHelpers.Base64KeyToRsaParameters");
                    }

                    var bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        throw new InvalidOperationException("bt had invalid value at RSAHelpers.Base64KeyToRsaParameters");

                    twobytes = binr.ReadUInt16();
                    switch (twobytes)
                    {
                        case 0x8130:
                            binr.ReadByte();    //advance 1 byte
                            break;

                        case 0x8230:
                            binr.ReadInt16();   //advance 2 bytes
                            break;

                        default:
                            throw new InvalidOperationException("twobytes had invalid value at RSAHelpers.Base64KeyToRsaParameters");
                    }

                    twobytes = binr.ReadUInt16();
                    byte lowbyte;
                    byte highbyte = 0x00;

                    switch (twobytes)
                    {
                        case 0x8102:
                            lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                            break;

                        case 0x8202:
                            highbyte = binr.ReadByte(); //advance 2 bytes
                            lowbyte = binr.ReadByte();
                            break;

                        default:
                            throw new InvalidOperationException();
                    }

                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    var modsize = BitConverter.ToInt32(modint, 0);

                    var firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    var modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        throw new InvalidOperationException();

                    var expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    var exponent = binr.ReadBytes(expbytes);

                    var ret = new RSAParameters
                    {
                        Exponent = exponent,
                        Modulus = modulus
                    };

                    return ret;
                }
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            int count;
            var bt = binr.ReadByte();
            if (bt != 0x02) //expect integer
                return 0;
            bt = binr.ReadByte();

            switch (bt)
            {
                case 0x81:
                    count = binr.ReadByte(); // data size in next byte
                    break;

                case 0x82:
                    var highbyte = binr.ReadByte();
                    var lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                    break;

                default:
                    count = bt; // we already have the data size
                    break;
            }

            while (binr.ReadByte() == 0x00)
            {   //remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);       //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        public static RSAParameters DecodeRSAPrivateKey(byte[] privkey)
        {
            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            using (var mem = new MemoryStream(privkey))
            {
                //wrap Memory Stream with BinaryReader for easy reading
                using (var binr = new BinaryReader(mem))
                {
                    var twobytes = binr.ReadUInt16();
                    switch (twobytes)
                    {
                        case 0x8130:
                            binr.ReadByte(); //advance 1 byte
                            break;

                        case 0x8230:
                            binr.ReadInt16(); //advance 2 bytes
                            break;

                        default:
                            throw new InvalidOperationException(
                                "twobytes had invalid value at RSAHelpers.DecodeRSAPrivateKey");
                    }

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102) //version number
                        throw new InvalidOperationException(
                            "twobytes had invalid value at RSAHelpers.DecodeRSAPrivateKey");
                    var bt = binr.ReadByte();
                    if (bt != 0x00)
                        throw new InvalidOperationException("bt had invalid value at RSAHelpers.DecodeRSAPrivateKey");

                    //------  all private key components are Integer sequences ----
                    var elems = GetIntegerSize(binr);
                    var modulus = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    var e = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    var d = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    var p = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    var q = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    var dp = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    var dq = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    var iq = binr.ReadBytes(elems);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    var rsaParams = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = e,
                        D = d,
                        P = p,
                        Q = q,
                        DP = dp,
                        DQ = dq,
                        InverseQ = iq
                    };
                    return rsaParams;
                }
            }
        }
    }
}