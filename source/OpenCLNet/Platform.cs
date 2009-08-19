﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace OpenCLNet
{

    unsafe public class Platform : InteropTools.IPropertyContainer
    {
        public OpenCLAPI CL { get; protected set; }
        public IntPtr PlatformID { get; protected set; }
        public string Profile { get { return InteropTools.ReadString( this, (uint)PlatformInfo.PROFILE ); } }
        public string Version { get { return InteropTools.ReadString( this, (uint)PlatformInfo.VERSION ); } }
        public string Name { get { return InteropTools.ReadString( this, (uint)PlatformInfo.NAME ); } }
        public string Vendor { get { return InteropTools.ReadString( this, (uint)PlatformInfo.VENDOR ); } }
        public string Extension { get { return InteropTools.ReadString( this, (uint)PlatformInfo.EXTENSIONS ); } }

        protected Dictionary<IntPtr,Device> _Devices =  new Dictionary<IntPtr, Device>();
        Device[] DeviceList;
        IntPtr[] DeviceIDs;

        public Platform( OpenCLAPI cl, IntPtr platformID )
        {
            CL = cl;
            PlatformID = platformID;

            // Create a local representation of all devices
            DeviceIDs = QueryDeviceIntPtr( DeviceType.ALL );
            for( int i=0; i<DeviceIDs.Length; i++ )
                _Devices[DeviceIDs[i]] = new Device( this, DeviceIDs[i] );
            DeviceList = InteropTools.ConvertDeviceIDsToDevices( this, DeviceIDs );
        }

        public Context CreateDefaultContext( ContextNotify notify, IntPtr userData )
        {
            IntPtr[] properties = new IntPtr[]
            {
                new IntPtr((long)ContextProperties.PLATFORM), PlatformID,
                IntPtr.Zero,
            };

            IntPtr contextID;
            ErrorCode result;

            contextID = (IntPtr)CL.CreateContext( properties,
                (uint)DeviceIDs.Length,
                DeviceIDs,
                notify,
                userData,
                out result );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "CreateContext failed with error code: "+result );
            return new Context( this, contextID );
        }

        public Device GetDevice( IntPtr index )
        {
            return _Devices[index];
        }

        protected IntPtr[] QueryDeviceIntPtr( DeviceType deviceType )
        {
            ErrorCode result;
            uint numberOfDevices;
            IntPtr[] deviceIDs;

            result = (ErrorCode)CL.GetDeviceIDs( PlatformID, deviceType, 0, null, out numberOfDevices );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetDeviceIDs failed: "+((ErrorCode)result).ToString() );

            deviceIDs = new IntPtr[numberOfDevices];
            result = (ErrorCode)CL.GetDeviceIDs( PlatformID, deviceType, numberOfDevices, deviceIDs, out numberOfDevices );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetDeviceIDs failed: "+((ErrorCode)result).ToString() );

            return deviceIDs;
        }

        public Device[] QueryDevices( DeviceType deviceType )
        {
            IntPtr[] deviceIDs;

            deviceIDs = QueryDeviceIntPtr( deviceType );
            return InteropTools.ConvertDeviceIDsToDevices( this, deviceIDs );
        }

        public static implicit operator IntPtr( Platform p )
        {
            return p.PlatformID;
        }

        #region IPropertyContainer Members

        public IntPtr GetPropertySize( uint key )
        {
            IntPtr propertySize;
            ErrorCode result;

            result = (ErrorCode)CL.GetPlatformInfo( PlatformID, key, IntPtr.Zero, null, out propertySize );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "Unable to get platform info for platform "+PlatformID+": "+result );
            return propertySize;

        }

        public void ReadProperty( uint key, IntPtr keyLength, void* pBuffer )
        {
            IntPtr propertySize;
            ErrorCode result;

            result = (ErrorCode)CL.GetPlatformInfo( PlatformID, key, keyLength, (void*)pBuffer, out propertySize );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "Unable to get platform info for platform "+PlatformID+": "+result );
        }

        #endregion
    }
}