using System;
using System.Runtime.InteropServices;

public struct SHFILEINFO
{
	public IntPtr hIcon;
	public IntPtr iIcon;
	public uint dwAttributes;
	
	[MarshalAs(UnmanagedType.ByValTStr,SizeConst = 260)] 
	public string szDisplayName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
    public string szTypeName;


}


public class Win32
{
	public const uint SHGFI_ICON = 0x100;
	public const uint SHGFI_LARGEICON = 0x0;
	public const uint SHGFI_SMALLICON = 0x1;

	public Win32()
	{
	}
	[DllImport("shell32.dll",CharSet = CharSet.Unicode)]
	public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,uint cbSizeFileInfo , uint uFlags);
}
