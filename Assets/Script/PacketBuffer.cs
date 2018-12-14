using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class PacketBuffer : IDisposable {

    List<byte> _bufferList;
    byte[] _readbuffer;
    int _readPos;
    bool _buffUpdate = false;

    public PacketBuffer() {
        _bufferList = new List<byte>();
        _readPos = 0;
    }

    public int GetReadPos() {
        return _readPos;
    }

    public byte[] ToArray() {
        return _bufferList.ToArray();
    }

    public int Count() {
        return _bufferList.Count;
    }

    public int Length() {
        return Count() - _readPos;
    }

    public void Clear() {
        _bufferList.Clear();
        _readPos = 0;
    }

    // Write data
    public void WriteBytes(byte[] input) {
        _bufferList.AddRange(input);
        _buffUpdate = true;
    }

    public void WriteByte(byte input) {
        _bufferList.Add(input);
        _buffUpdate = true;
    }

    public void WriteInteger(int input) {
        _bufferList.AddRange(BitConverter.GetBytes(input));
        _buffUpdate = true;
    }

    public void WriteFloat(float input) {
        _bufferList.AddRange(BitConverter.GetBytes(input));
        _buffUpdate = true;
    }

    public void WriteString(string input) {
        _bufferList.AddRange(BitConverter.GetBytes(input.Length));
        _bufferList.AddRange(Encoding.ASCII.GetBytes(input));
        _buffUpdate = true;
    }

    // Read data
    public int ReadInteger(bool peek = true) {
        if(_bufferList.Count > _readPos) {

            if(_buffUpdate == true) {
                _readbuffer = _bufferList.ToArray();
                _buffUpdate = false;
            }

            int value = BitConverter.ToInt32(_readbuffer, _readPos);

            if(peek && _bufferList.Count > _readPos) {
                _readPos += 4;
            }
            return value;
        }
        else {
            throw new Exception("ReadInteger -> Buffer is past its limit");
        }
    }

    public float ReadFloat(bool peek = true) {
        if(_bufferList.Count > _readPos) {
            if(_buffUpdate == true) {
                _readbuffer = _bufferList.ToArray();
                _buffUpdate = false;
            }

            float value = BitConverter.ToSingle(_readbuffer, _readPos);

            if(peek & _bufferList.Count > _readPos) {
                _readPos += 4;
            }
            return value;
        }
        else {
            throw new Exception("ReadFloat -> Buffer is past its limit");
        }
    }

    public byte ReadByte(bool peek = true) {
        if(_bufferList.Count > _readPos) {
            if(_buffUpdate == true) {
                _readbuffer = _bufferList.ToArray();
                _buffUpdate = false;
            }

            byte value = _readbuffer[_readPos];

            if(peek & _bufferList.Count > _readPos) {
                _readPos += 1;
            }
            return value;
        }
        else {  
            throw new Exception("ReadByte -> Buffer is past its limit");
        }
    }

    public byte[] ReadBytes(int length, bool peek = true) {
        if(_buffUpdate == true) {
            _readbuffer = _bufferList.ToArray();
            _buffUpdate = false;
        }

        byte[] value = _bufferList.GetRange(_readPos, length).ToArray();

        if(peek & _bufferList.Count > _readPos) {
            _readPos += length;
        }

        return value;
    }

    public string ReadString(bool peek = true) {

        int length = ReadInteger(true);

        if(_buffUpdate == true) {
            _readbuffer = _bufferList.ToArray();
            _buffUpdate = false;
        }

        string value = Encoding.ASCII.GetString(_readbuffer, _readPos, length);

        if(peek & _bufferList.Count > _readPos) {
            _readPos += length;
        }
        return value;
    }

    // IDisposable
    private bool disposedValue = false;
    protected virtual void Dispose(bool disposing) {
        if(!disposedValue) {
            if(disposing) {
                _bufferList.Clear();
            }
            _readPos = 0;
            disposedValue = true;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}
