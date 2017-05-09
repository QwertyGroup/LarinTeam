using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace FaceRecognition.Core
{
	public abstract class FileManager
	{
		public abstract void UnsafeSave(string path, string filename, object obj);
		public abstract T UnsafeLoad<T>(string path, string filename);
		public abstract void Delete(string path, string filename);

		public virtual void CheckPath(string path)
		{
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
		}

		public virtual List<string> GetFilesInDirectory(string dir, bool withExtension = true, bool withPath = true)
		{
			var files = Directory.GetFiles(dir).ToList();
			if (!withPath)
			{
				files = files.Select(f => f = f.Replace(dir + "\\", string.Empty)).ToList();
			}
			if (!withExtension)
			{
				files = files.Select(f =>
				{
					var dotPos = f.LastIndexOfAny(new char[] { '.' });
					f = f.Remove(dotPos, f.Length - dotPos);
					return f;
				}).ToList();
			}
			return files;
		}
	}

	public class FileManagerJson : FileManager
	{
		public override void Delete(string path, string filename)
		{
			if (path != string.Empty)
				path += "/";
			File.Delete(path + filename + ".json");
		}

		public override void UnsafeSave(string path, string filename, object obj)
		{
			if (path != string.Empty)
				path += "/";
			CheckPath(path);
			var writer = new StreamWriter(path + filename + ".json");
			var jser = new JsonSerializer();
			jser.Formatting = Formatting.Indented;
			jser.Serialize(writer, obj);
			writer.Close();
		}

		public override T UnsafeLoad<T>(string path, string filename)
		{
			if (path != string.Empty)
				path += "/";
			CheckPath(path);
			var jser = new JsonSerializer();
			var reader = new StreamReader(path + filename + ".json");
			var data = jser.Deserialize(reader, typeof(T));
			reader.Close();
			return (T)data;
		}
	}

	public class FileManagerXml : FileManager
	{
		public override void Delete(string path, string filename)
		{
			if (path != string.Empty)
				path += "/";
			File.Delete(path + filename + ".xml");
		}

		public override T UnsafeLoad<T>(string path, string filename)
		{
			if (path != string.Empty)
				path += "/";
			var xser = new XmlSerializer(typeof(T));
			var reader = new StreamReader(path + filename + ".xml");
			var data = xser.Deserialize(reader);
			reader.Close();
			return (T)data;
		}

		public override void UnsafeSave(string path, string filename, object obj)
		{
			if (path != string.Empty)
				path += "/";
			CheckPath(path);
			var xser = new XmlSerializer(obj.GetType());
			var writer = new StreamWriter(path + filename + ".xml");
			xser.Serialize(writer, obj);
			writer.Close();
		}
	}
}
