using System;
using System.Collections.Generic;
using System.IO;

namespace HoloArchiver
{
    public class SimpleArchive : IDisposable
    {
        Dictionary<string, long> _index = new Dictionary<string, long>();
        Stream _fileStream;
        BufferedStream _buffer;
        BinaryWriter _fileWriter;
        BinaryReader _fileReader;
        bool _isDisposing;
        private SimpleArchive() { }
        public static SimpleArchive OpenOrCreate(Stream stream)
        {
            var instance = new SimpleArchive();
            instance._fileStream = stream;
            instance._buffer = new BufferedStream(instance._fileStream, 1024 * 1024);            
            instance._fileReader = new BinaryReader(instance._buffer);
            if (stream.CanWrite)
            {
                instance._fileWriter = new BinaryWriter(instance._buffer);
            }
            return instance;
        }
        public static SimpleArchive OpenOrCreate(string fileName)
        {
            return OpenOrCreate(File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
        }
        // GetNextEntry will start again from zero
        public void Flush()
        {
            lock (this)
            {
                _buffer.Flush();
                _fileStream.Flush();
                _buffer.Position = 0;
            }
        }
        public byte[] GetEntry(string name)
        {
            lock (this)
            {
                if (_index.ContainsKey(name))
                {
                    _buffer.Position = _index[name];
                }
                else
                {
                    _buffer.Position = 0;
                }
                return GetNextEntry(name);
            }
        }
        byte[] GetNextEntry(string name)
        {
            lock (this)
            {
                while (_buffer.Position < _buffer.Length)
                {                    
                    string entryName = _fileReader.ReadString();
                    UInt32 lenght = _fileReader.ReadUInt32();
                    if (name == entryName)
                    {
                        return _fileReader.ReadBytes((int)lenght);
                    }
                    _buffer.Position += lenght;
                }
                return null;
                //throw new Exception("Entry not found");
            }
        }
        public IReadOnlyList<string> GetEntryNames()
        {
            lock (this)
            {
                var names = new List<string>();
                _buffer.Position = 0;
                while (_buffer.Position < _buffer.Length)
                {
                    long offset = _buffer.Position;
                    string entryName = _fileReader.ReadString();
                    UInt32 lenght = _fileReader.ReadUInt32();
                    _index.Add(entryName, offset);
                    names.Add(entryName);
                    _buffer.Position += lenght;
                }
                return names;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposing)
            {
                if (disposing)
                {
                    _fileWriter?.Close();
                    _fileReader?.Close();
                    _buffer?.Close();
                    _fileStream?.Close();
                }
                _isDisposing = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}