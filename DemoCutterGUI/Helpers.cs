using DemoCutterGUI.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DemoCutterGUI
{

    public static class SpiralLocationHelper {

        // 
        // from:https://github.com/wecand0/base91
        /*
        MIT License

        Copyright (c) 2024 Vadim

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
        */
        public static readonly char[] base91BasicAlphabet_ = new char[91] {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', //00..12
		        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', //13..25
		        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', //26..38
		        'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', //39..51
		        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '!', '#', '$', //52..64
		        '%', '&', '(', ')', '*', '+', ',', '.', '/', ':', ';', '-', '=', //65..77
		        '\\', '?', '@', '[', ']', '^', '_', '`', '{', '|', '}', '~', '\''//78..90
        };

        public static readonly byte[] base91DecAlphabet_ = new byte[256]  {
                91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//000..015
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//016..031
		        91, 62, 91, 63, 64, 65, 66, 90, 67, 68, 69, 70, 71, 76, 72, 73,//032..047 // @34: ", @39: ', @45: -
		        52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 74, 75, 91, 77, 91, 79,//048..063 // @60: <, @62: >
		        80, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,          //064..079
		        15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 81, 78, 82, 83, 84,//080..095 // @92: slash
		        85, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,//096..111
		        41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 86, 87, 88, 89, 91,//112..127
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//128..143
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//144..159
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//160..175
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//176..191
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//192..207
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//208..223
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91,//224..239
		        91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91, 91 //240..255
        };
        // END from https://github.com/wecand0/base91


        // 
        // from: https://github.com/chmike/fpsqrt (MIT license)
        /*
        MIT License

        Copyright (c) 2019 Christophe Meessen

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
        */
        // sqrt_i64 computes the squrare root of a 64bit integer and returns
        // a 64bit integer value. It requires that v is positive.
        static Int64 sqrt_i64(Int64 v)
        {
            UInt64 b = ((UInt64)1) << 62, q = 0, r = (UInt64)v;
            while (b > r)
                b >>= 2;
            while (b > 0)
            {
                UInt64 t = q + b;
                q >>= 1;
                if (r >= t)
                {
                    r -= t;
                    q += b;
                }
                b >>= 2;
            }
            return (Int64)q;
        }
        // fpsqrt end


        public static int encode2dspiral(int x, int y)
        { // sudely there has to be a more efficient way...
            if (x ==0 && y ==0) return 0;
            int ring = Math.Max(Math.Abs(x), Math.Abs(y));
            int sidelen = (ring - 1) * 2 + 1;
            int index = sidelen * sidelen;
            sidelen += 2;
            int premultiplier = sidelen - 1;

            if (x == ring)
            {
                index += (ring - y);
            }
            else if (y == -ring)
            {
                index += premultiplier + (ring - x);
            }
            else if (x == -ring)
            {
                index += premultiplier * 2 + (y + ring);
            }
            else
            {
                index += premultiplier * 3 + (x + ring);
            }

            return index;
        }

        public static void decode2dspiral(int index, ref int x, ref int y)
        {
            if (index == 0)
            {
                x = 0;
                y = 0;
                return;
            }
            int ring = (int)sqrt_i64(index);
            ring = ((ring - 1) / 2) + 1;
            int sidelen = (ring - 1) * 2 + 1;
            index -= sidelen * sidelen;
            sidelen += 2;

            int premultiplier = sidelen - 1;

            if (index >= premultiplier * 3)
            {
                y = ring;
                x = index - premultiplier * 3 - ring;
            }
            else if (index >= premultiplier * 2)
            {
                y = index - premultiplier * 2 - ring;
                x = -ring;
            }
            else if (index > premultiplier)
            {
                x = premultiplier + ring - index;
                y = -ring;
            }
            else
            {
                x = ring;
                y = ring - index;
            }

        }

        static OpenTK.Mathematics.Vector2[] decodeSpiralStringAType(ReadOnlySpan<char> encodedLocations, int charcount)
        {

            if(charcount == 0)
            {
                // speciall case. its just a 0,0 locatioon
                return new OpenTK.Mathematics.Vector2[] { new OpenTK.Mathematics.Vector2(0,0) };
            }
            List<OpenTK.Mathematics.Vector2> retVal = new List<OpenTK.Mathematics.Vector2>();

            int sizeleft = encodedLocations.Length;
            int index = 0;
            while(sizeleft >= charcount)
            {
                int number = 0;
                int multiplier = 1;
                for(int i = 0; i < charcount; i++)
                {
                    char charHere = encodedLocations[index + i];
                    if(charHere < 256)
                    {
                        charHere = (char)base91DecAlphabet_[charHere];
                        if(charHere < 91)
                        {
                            number += multiplier * charHere;
                        }
                    }
                    multiplier *= 91;
                }

                int x=0, y=0;
                decode2dspiral(number, ref x, ref y);
                retVal.Add(new OpenTK.Mathematics.Vector2(x*500,y*500));

                sizeleft -= charcount;
                index += charcount;
            }



            return retVal.ToArray();
        }

        public static OpenTK.Mathematics.Vector2[] decodeSpiralString(string encodedLocations)
        {
            if(encodedLocations.Length == 0)
            {
                return null;
            }
            char startChar = encodedLocations[0];
            if(startChar >= '0' && startChar <= '9')
            {
                return decodeSpiralStringAType(encodedLocations.AsSpan(1),startChar-'0');
            }
            return null;
        }

    }





    static class Helpers
    {
        static public unsafe string DemoCuttersanitizeFilename(string input, bool allowExtension)
        {
            if (input == null) return null;
            byte[] byteArray = new byte[input.Length + 1];
            byte[] byteArrayOut = new byte[input.Length + 1];
            for (int i = 0; i < input.Length; i++)
            {
                byteArray[i] = (byte)input[i];
            }
            byteArray[input.Length] = 0;

            int outLength = 0;
            fixed (byte* inP = byteArray, outP = byteArrayOut)
            {
                DemoCuttersanitizeFilenameReal(inP, outP, allowExtension, ref outLength);
            }
            return Encoding.ASCII.GetString(byteArrayOut, 0, Math.Min(input.Length, outLength));
        }
        static unsafe void DemoCuttersanitizeFilenameReal(byte* input, byte* output, bool allowExtension, ref int outLength)
        {
            byte* outStart = output;
            byte* lastDot = (byte*)0;
            byte* inputStart = input;
            while (*input != 0)
            {
                if (*input == '.' && input != inputStart)
                { // Even tho we allow extensions (dots), we don't allow the dot at the start of the filename.
                    lastDot = output;
                }
                if ((*input == 32) // Don't allow ! exclamation mark. Linux doesn't like that.
                    || (*input >= 35 && *input < 42)
                    || (*input >= 43 && *input < 46)
                    || (*input >= 48 && *input < 58)
                    || (*input >= 59 && *input < 60)
                    || (*input == 61)
                    || (*input >= 64 && *input < 92)
                    || (*input >= 93 && *input < 96) // Don't allow `. Linux doesn't like that either, at least not in shell scripts.
                    || (*input >= 97 && *input < 124)
                    || (*input >= 125 && *input < 127)
                    )
                {
                    *output++ = *input;
                }
                else if (*input == '|')
                {

                    *output++ = (byte)'I';
                }

                else
                {
                    *output++ = (byte)'-';
                }
                input++;
            }
            *output = 0;
            outLength = (int)(output - outStart);

            if (allowExtension && lastDot != (byte*)0)
            {
                *lastDot = (byte)'.';
            }
        }


        // IDK if this works reliably. Test it if you need it.
        public static DependencyObject GetChildOfType<T>(this DependencyObject obj)
        {
            int depth = 0;
            return obj.GetChildOfType<T>(ref depth);
        }

        // IDK if this works reliably. Test it if you need it.
        public static DependencyObject GetChildOfType<T>(this DependencyObject obj, ref int depth)
        {
            int lowestDepthFindDepth = int.MaxValue;
            DependencyObject lowestDepthFind = null;
            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject thisChild = VisualTreeHelper.GetChild(obj, i);
                if(thisChild is T)
                {
                    return thisChild;
                } else
                {
                    int findDepth = 0;
                    DependencyObject nestedFind = thisChild.GetChildOfType<T>(ref findDepth);
                    if(nestedFind != null)
                    {
                        findDepth++;
                        if(findDepth < lowestDepthFindDepth) // Wanna find the highest up child of type T
                        {
                            lowestDepthFindDepth = findDepth;
                            lowestDepthFind = nestedFind;
                        }
                    }
                }
            }
            depth += lowestDepthFindDepth;
            return lowestDepthFind;
        }

        public static T ReadBytesAsType<T>(BinaryReader br, long byteOffset = -1, SeekOrigin seekOrigin = SeekOrigin.Begin)
        {
            if(!(byteOffset == -1 && seekOrigin == SeekOrigin.Begin))
            {
                br.BaseStream.Seek(byteOffset, seekOrigin);
            }
            byte[] bytes = br.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T retVal = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return retVal;
        }
        public static T ArrayBytesAsType<T,B>(B data, int byteOffset=0)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T retVal = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject()+ byteOffset, typeof(T));
            handle.Free();
            return retVal;
        }

        public static float zCross2d(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3)
        {
            return ((p2.X - p1.X) * (p3.Y - p2.Y)) - ((p2.Y - p1.Y) * (p3.X - p2.X));
        }

        public static bool pointInTriangle2D(ref Vector3 point, ref Vector3 t1, ref Vector3 t2, ref Vector3 t3)
        {
            float a = zCross2d(ref t1, ref t2, ref point);
            float b = zCross2d(ref t2, ref t3, ref point);
            float c = zCross2d(ref t3, ref t1, ref point);

            return a > 0 && b > 0 && c > 0 || a < 0 && b < 0 && c < 0;
        }

        public static ByteImage BitmapToByteArray(Bitmap bmp)
        {

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int stride = Math.Abs(bmpData.Stride);
            int bytes = stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            bmp.UnlockBits(bmpData);

            return new ByteImage(rgbValues, stride, bmp.Width, bmp.Height, bmp.PixelFormat);
        }

        public static Bitmap ByteArrayToBitmap(ByteImage byteImage)
        {
            Bitmap myBitmap = new Bitmap(byteImage.width, byteImage.height, byteImage.pixelFormat);
            Rectangle rect = new Rectangle(0, 0, myBitmap.Width, myBitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                myBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                myBitmap.PixelFormat);

            bmpData.Stride = byteImage.stride;

            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(byteImage.imageData, 0, ptr, byteImage.imageData.Length);

            myBitmap.UnlockBits(bmpData);
            return myBitmap;

        }
    }

    class UnixEpochDateTimeOffsetConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(0 < (options.NumberHandling & JsonNumberHandling.AllowReadingFromString) && reader.TokenType == JsonTokenType.String)
            {
                Int64 numberTry;
                string numberString = reader.GetString();
                if (Int64.TryParse(numberString, out numberTry))
                {
                    return DateTime.UnixEpoch.AddSeconds(numberTry);
                }
                else
                {
                    return null;
                }
            } else
            {
                return DateTime.UnixEpoch.AddSeconds(reader.GetInt64());
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(((DateTimeOffset)value).ToUnixTimeSeconds());
        }
    }







    // This class is from: https://stackoverflow.com/questions/1427471/observablecollection-not-noticing-when-item-in-it-changes-even-with-inotifyprop
    public class FullyObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property is changed within an item.
        /// </summary>
        public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;

        public FullyObservableCollection() : base()
        { }

        public FullyObservableCollection(List<T> list) : base(list)
        {
            ObserveAll();
        }

        public FullyObservableCollection(IEnumerable<T> enumerable) : base(enumerable)
        {
            ObserveAll();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= ChildPropertyChanged;
            }

            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.NewItems)
                    item.PropertyChanged += ChildPropertyChanged;
            }

            base.OnCollectionChanged(e);
        }

        protected void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(this, e);
        }

        protected void OnItemPropertyChanged(int index, PropertyChangedEventArgs e)
        {
            OnItemPropertyChanged(new ItemPropertyChangedEventArgs(index, e));
        }

        protected override void ClearItems()
        {
            foreach (T item in Items)
                item.PropertyChanged -= ChildPropertyChanged;

            base.ClearItems();
        }

        private void ObserveAll()
        {
            foreach (T item in Items)
                item.PropertyChanged += ChildPropertyChanged;
        }

        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T typedSender = (T)sender;
            int i = Items.IndexOf(typedSender);

            if (i < 0)
                throw new ArgumentException("Received property notification from item not in collection");

            OnItemPropertyChanged(i, e);
        }
    }

    /// <summary>
    /// Provides data for the <see cref="FullyObservableCollection{T}.ItemPropertyChanged"/> event.
    /// </summary>
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the index in the collection for which the property change has occurred.
        /// </summary>
        /// <value>
        /// Index in parent collection.
        /// </value>
        public int CollectionIndex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index in the collection of changed item.</param>
        /// <param name="name">The name of the property that changed.</param>
        public ItemPropertyChangedEventArgs(int index, string name) : base(name)
        {
            CollectionIndex = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : this(index, args.PropertyName)
        { }
    }
}
