﻿using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System;
using System.Linq;
using System.Windows;

namespace FaceRecognation._1._0
{
	public class ImageProcessing
	{
		private ImageProcessing() { }
		private static ImageProcessing _imageProcessingInstance;
		public static ImageProcessing GetImageProcessingInstance
		{
			get
			{
				if (_imageProcessingInstance == null)
					_imageProcessingInstance = new ImageProcessing();
				return _imageProcessingInstance;
			}
		}

		private Bitmap ConvertImageToBitmap(Image img)
		{
			return new Bitmap(img);
		}

		private Image ConvertBitmapToImage(Bitmap bm)
		{
			return bm;
		}

		private BitmapImage ConvetrBitmapToBitmapImage(Bitmap bm)
		{
			var memstr = new MemoryStream();
			bm.Save(memstr, ImageFormat.Jpeg);
			var bitmapImg = new BitmapImage();
			bitmapImg.BeginInit();
			memstr.Seek(0, SeekOrigin.Begin);
			bitmapImg.StreamSource = memstr;
			bitmapImg.EndInit();
			return bitmapImg;
		}

		public BitmapImage ConvertImageToBitmapImage(Image img)
		{
			return ConvetrBitmapToBitmapImage(ConvertImageToBitmap(img));
		}

		private Bitmap ConvertBitmapImagetoBitmap(BitmapImage bitmapImg)
		{
			using (MemoryStream memstr = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bitmapImg));
				enc.Save(memstr);
				Bitmap bitmap = new Bitmap(memstr);
				return new Bitmap(bitmap);
			}
		}

		private Bitmap ConvertCroppedBitmapToBitmap(CroppedBitmap croppedImg)
		{
			using (MemoryStream memstr = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(croppedImg));
				enc.Save(memstr);
				Bitmap bitmap = new Bitmap(memstr);
				return new Bitmap(bitmap);
			}
		}

		public Image ConvertBitmapImageToImage(BitmapImage bitmapImg)
		{
			return ConvertBitmapToImage(ConvertBitmapImagetoBitmap(bitmapImg));
		}

		public string SaveDirectory { get; set; } = string.Empty;
		public string SaveImageToFile(string filename, Image img)
		{
			return SaveImageToFile(filename, img, img.RawFormat);
		}

		public string SaveImageToFile(string filename, Image img, ImageFormat imgformat)
		{
			var fullpath = UnitePathWithDir(filename) + GetExtention(imgformat);
			img.Save(fullpath, imgformat);
			return fullpath;
		}

		private string GetExtention(ImageFormat imgformat)
		{
			return ImageCodecInfo.GetImageEncoders()
					.First(x => x.FormatID == imgformat.Guid)
					.FilenameExtension
					.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
					.First()
					.Trim('*')
					.ToLower();
		}

		private string UnitePathWithDir(string filename)
		{
			if (SaveDirectory != string.Empty)
			{
				if (!Directory.Exists(SaveDirectory))
					Directory.CreateDirectory(SaveDirectory);
				filename = SaveDirectory + @"\" + filename;
			}
			return filename;
		}

		public Image LoadImageFromFile(string filename)
		{
			filename = UnitePathWithDir(filename);
			return Image.FromFile(filename);
		}

		public Stream ImageToStream(Image img, ImageFormat imgformat)
		{
			var guid = Guid.NewGuid();
			var oldDir = SaveDirectory;
			SaveDirectory = _cacheDir;
			var path = SaveImageToFile(guid.ToString(), img);
			var fs = File.OpenRead(path);
			SaveDirectory = oldDir;
			return fs;
		}

		public Stream ImageToStream(Image img)
		{
			return ImageToStream(img, img.RawFormat);
		}

		private string _cacheDir = "Cache";
		public void ClearCache()
		{
			foreach (var file in new DirectoryInfo(_cacheDir).GetFiles())
				file.Delete();
		}

		public Image CropImage(Image img, Microsoft.ProjectOxford.Face.Contract.FaceRectangle rect)
		{
			var croppedImg = new CroppedBitmap(ConvertImageToBitmapImage(img), new Int32Rect(rect.Left, rect.Top, rect.Width, rect.Height));
			return ConvertBitmapToImage(ConvertCroppedBitmapToBitmap(croppedImg));
		}
	}
}
