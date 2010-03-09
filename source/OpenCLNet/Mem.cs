﻿/*
 * Copyright (c) 2009 Olav Kalgraf(olav.kalgraf@gmail.com)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace OpenCLNet
{
    unsafe public class Mem : IDisposable, InteropTools.IPropertyContainer
    {
        // Track whether Dispose has been called.
        private bool disposed = false;

        private TextureInfo TxInfo;

        public IntPtr MemID { get; protected set; }
        public Context Context { get; protected set; }
        public MemObjectType MemType { get { return (MemObjectType)InteropTools.ReadUInt( this, (uint)MemInfo.TYPE ); } }
        public MemFlags MemFlags { get { return (MemFlags)InteropTools.ReadULong( this, (uint)MemInfo.FLAGS ); } }
        public IntPtr MemSize { get { return InteropTools.ReadIntPtr( this, (uint)MemInfo.SIZE ); } }
        public IntPtr HostPtr { get { return InteropTools.ReadIntPtr( this, (uint)MemInfo.HOST_PTR ); } }
        public uint MapCount { get { return InteropTools.ReadUInt( this, (uint)MemInfo.MAP_COUNT ); } }
        public uint ReferenceCount { get { return InteropTools.ReadUInt( this, (uint)MemInfo.REFERENCE_COUNT ); } }

        public uint TextureTarget { get { return InteropTools.ReadUInt(TxInfo, (uint)CLGLTextureInfo.TEXTURE_TARGET); } }
        public int MipMapLevel { get { return InteropTools.ReadInt(TxInfo, (uint)CLGLTextureInfo.MIPMAP_LEVEL);  } }


        #region Construction / Destruction

        internal Mem( Context context, IntPtr memID )
        {
            Context = context;
            MemID = memID;
            TxInfo = new TextureInfo(this);
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Mem()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose( false );
        }

        #endregion

        #region IDisposable Members

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose( true );
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize( this );
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose( bool disposing )
        {
            // Check to see if Dispose has already been called.
            if( !this.disposed )
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if( disposing )
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                OpenCL.ReleaseMemObject( MemID );
                MemID = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion


        public void GetGLObjectInfo(out CLGLObjectType glObjectType, out IntPtr glObjectName)
        {
            ErrorCode result;
            uint type;
            uint name;

            result = OpenCL.GetGLObjectInfo(MemID, out type, out name);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("GetGLObjectInfo failed: " + result, result);
            glObjectType = (CLGLObjectType)type;
            glObjectName = (IntPtr)name;
        }


        #region IPropertyContainer Members

        unsafe public virtual IntPtr GetPropertySize( uint key )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetMemObjectInfo( MemID, key, IntPtr.Zero, null, out size );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetMemObjectInfo failed: "+result, result );
            return size;
        }

        unsafe public virtual void ReadProperty( uint key, IntPtr keyLength, void* pBuffer )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetMemObjectInfo( MemID, key, keyLength, pBuffer, out size );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetMemObjectInfo failed: "+result, result );
        }

        #endregion


        class TextureInfo : InteropTools.IPropertyContainer
        {
            Mem Mem;

            public TextureInfo(Mem mem)
            {
                Mem = mem;
            }

            #region IPropertyContainer Members

            public IntPtr GetPropertySize(uint key)
            {
                ErrorCode result;
                IntPtr size;

                result = (ErrorCode)OpenCL.GetGLTextureInfo(Mem.MemID, key, IntPtr.Zero, null, out size);
                if (result != ErrorCode.SUCCESS)
                    throw new OpenCLException("GetGLTextureInfo failed with error code " + result, result);

                return size;
            }

            public void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
            {
                ErrorCode result;
                IntPtr size;

                result = (ErrorCode)OpenCL.GetGLTextureInfo(Mem.MemID, key, keyLength, pBuffer, out size);
                if (result != ErrorCode.SUCCESS)
                    throw new OpenCLException("GetGLTextureInfo failed with error code " + result, result);
            }

            #endregion
        }

    }
}
