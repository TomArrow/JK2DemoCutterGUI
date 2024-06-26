﻿using DemoCutterGUI.Tools;
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
