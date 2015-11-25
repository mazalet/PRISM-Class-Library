Option Strict On

'File:  ShareConnector.vb
'File Contents:  ShareConnector class that connects a machine to an SMB/CIFS share
'                using a password and user name.
'Author(s):  Nathan Trimble
'
'Modifed: DAC (7/2/2004) -- Provided overloading for constructor, added property for share name

Namespace Files
    ''' <summary>Connects to a file share using a password and user name.
    ''' </summary>
    ''' <remarks>
    ''' The default behaviour when connecting to SMB/CIFS file shares is for
    ''' the system to supply the user name and password used to logon to the local machine.
    ''' This class allows you to connect to SMB/CIFS file shares when the use of
    ''' SSPI isn't availabe and/or when you don't wish to use the default behaviour.
    ''' It's quite comparable to the "Connect using a different user name." option in the Map Network Drive
    ''' utility in Windows.  Much of this code came from Microsoft Knowledge Base Article - 173011.  It was
    ''' then modified to fit our needs.
    ''' </remarks>
    Public Class ShareConnector

		Private mErrorMessage As String = ""

        Public Enum ResourceScope As Integer
            Connected = 1
            GlobalNetwork
            Remembered
            Recent
            Context
        End Enum

        Public Enum ResourceType As Integer
            Any = 0
            Disk = 1
            Print = 2
            Reserved = 8
        End Enum

        Public Enum ResourceDisplaytype As Integer
            Generic = &H0
            Domain = &H1
            Server = &H2
            Share = &H3
            File = &H4
            Group = &H5
            Network = &H6
            Root = &H7
            Shareadmin = &H8
            Directory = &H9
            Tree = &HA
            Ndscontainer = &HB
        End Enum

		''' <summary>This structure is used to group a bunch of member variables.</summary>
		Private Structure udtNetResource
            Dim dwScope As ResourceScope
            Dim dwType As ResourceType
            Dim dwDisplayType As ResourceDisplaytype
			Dim dwUsage As Integer
			Dim lpLocalName As String
			Dim lpRemoteName As String
			Dim lpComment As String
			Dim lpProvider As String
		End Structure

		Private Const NO_ERROR As Short = 0
		Private Const CONNECT_UPDATE_PROFILE As Short = &H1S

        '' ''' <summary> Constant that may be used by NETRESOURCE->dwScope </summary>
        ''Private Const RESOURCE_CONNECTED As Short = &H1S
        '' ''' <summary> Constant that may be used by NETRESOURCE->dwScope </summary>
        ''Private Const RESOURCE_GLOBALNET As Short = &H2S

        '' ''' <summary> Constant that may be used by NETRESOURCE->dwType </summary>
        ''Private Const RESOURCETYPE_DISK As Short = &H1S
        '' ''' <summary> Constant that may be used by NETRESOURCE->dwType </summary>
        ''Private Const RESOURCETYPE_PRINT As Short = &H2S
        '' ''' <summary> Constant that may be used by NETRESOURCE->dwType </summary>
        ''Private Const RESOURCETYPE_ANY As Short = &H0S

        '' ''' <summary> Constant that may be used by NETRESOURCE->dwDisplayType </summary>
        ''Private Const RESOURCEDISPLAYTYPE_DOMAIN As Short = &H1S
        '' ''' <summary> Constant that may be used by NETRESOURCE->dwDisplayType </summary>
        ''Private Const RESOURCEDISPLAYTYPE_GENERIC As Short = &H0S
        '' ''' <summary> Constant that may be used by NETRESOURCE->dwDisplayType </summary>
        ''Private Const RESOURCEDISPLAYTYPE_SERVER As Short = &H2S
        '' ''' <summary> Constant that may be used by NETRESOURCE->dwDisplayType </summary>
        ''Private Const RESOURCEDISPLAYTYPE_SHARE As Short = &H3S

        ''' <summary> Constant that may be used by NETRESOURCE->dwUsage </summary>
        Private Const RESOURCEUSAGE_CONNECTABLE As Short = &H1S

		''' <summary> Constant that may be used by NETRESOURCE->dwUsage </summary>
		Private Const RESOURCEUSAGE_CONTAINER As Short = &H2S

        Private Declare Function WNetAddConnection2 Lib "mpr.dll" Alias "WNetAddConnection2A" (ByRef lpNetResource As udtNetResource, lpPassword As String, lpUserName As String, dwFlags As Integer) As Integer
        Private Declare Function WNetCancelConnection2 Lib "mpr.dll" Alias "WNetCancelConnection2A" (lpName As String, dwFlags As Integer, fForce As Integer) As Integer

        Private mNetResource As udtNetResource
        Private mUsername As String
        Private mPassword As String
        Private mShareName As String = String.Empty

        ''' <summary>
        ''' This version of the constructor requires you to specify the sharename by setting the <see cref="Share">Share</see> property.
        ''' </summary>
        ''' <param name="userName">Username</param>
        ''' <param name="userPwd">Password</param>
        ''' <remarks>For local user accounts, it is safest to use HostName\username</remarks>
        Public Sub New(userName As String, userPwd As String)
            RealNew(userName, userPwd)
        End Sub

        ''' <summary>
        ''' This version of the constructor allows you to specify the sharename as an argument.
        ''' </summary>
        ''' <param name="shareName">The name of the file share to which you will connect.</param>
        ''' <param name="userName">Username</param>
        ''' <param name="userPwd">Password</param>
        ''' <remarks>For local user accounts, it is safest to use HostName\username</remarks>  ''' 
        Public Sub New(shareName As String, userName As String, userPwd As String)
            DefineShareName(shareName)
            RealNew(userName, userPwd)
        End Sub

        ''' <summary>
        ''' This routine is called by each of the constructors to make the actual assignments in a consistent fashion.
        ''' </summary>
        ''' <param name="userName">Username</param>
        ''' <param name="userPwd">Password</param>
        Private Sub RealNew(userName As String, userPwd As String)
            mUsername = userName
            mPassword = userPwd
            mNetResource.lpRemoteName = mShareName
            mNetResource.dwType = ResourceType.Disk
            mNetResource.dwScope = ResourceScope.GlobalNetwork
            mNetResource.dwDisplayType = ResourceDisplaytype.Share
            mNetResource.dwUsage = RESOURCEUSAGE_CONNECTABLE
        End Sub

        ''' <summary>
        ''' Sets the name of the file share to which you will connect.
        ''' </summary>
        Public Property Share() As String
            Get
                Return mShareName
            End Get
            Set(Value As String)
                DefineShareName(Value)
                mNetResource.lpRemoteName = mShareName
            End Set
        End Property

        ''' <summary>
        ''' Connects to specified share using account/password specified through the constructor and 
        ''' the file share name passed as an argument.
        ''' </summary>
        ''' <param name="shareName">The name of the file share to which you will connect.</param>
        Public Function Connect(shareName As String) As Boolean

            DefineShareName(shareName)
            mNetResource.lpRemoteName = mShareName
            Return RealConnect()

        End Function

        ''' <summary>
        ''' Connects to specified share using account/password specified through the constructor.
        ''' Requires you to have specifyed the sharename by setting the <see cref="Share">Share</see> property.
        ''' </summary>
        Public Function Connect() As Boolean

            If mNetResource.lpRemoteName = "" Then
                mErrorMessage = "Share name not specified"
                Return False
            End If
            Return RealConnect()

        End Function

        ''' <summary>
        ''' Updates class variable with the specified share path
        ''' </summary>
        ''' <param name="shareName"></param>
        ''' <remarks>If the path ends in a forward slash then the slash will be removed</remarks>
        Private Sub DefineShareName(shareName As String)
            If shareName.EndsWith("\") Then
                mShareName = shareName.TrimEnd("\"c)
            Else
                mShareName = shareName
            End If
        End Sub

		''' <summary>
		''' Connects to specified share using account/password specified previously.
		''' This is the function that actually does the connection based on the setup 
		''' from the <see cref="Connect">Connect</see> functions.
		''' </summary>
		Private Function RealConnect() As Boolean

			Dim errorNum As Integer

			errorNum = WNetAddConnection2(mNetResource, mPassword, mUsername, 0)
			If errorNum = NO_ERROR Then
				Debug.WriteLine("Connected.")
				Return True
			Else
				mErrorMessage = errorNum.ToString()
				Debug.WriteLine("Got error: " & errorNum)
				Return False
			End If

		End Function

		''' <summary>
		''' Disconnects the files share.
		''' </summary>
		Public Function Disconnect() As Boolean
			Dim errorNum As Integer = WNetCancelConnection2(Me.mNetResource.lpRemoteName, 0, CInt(True))
			If errorNum = NO_ERROR Then
				Debug.WriteLine("Disconnected.")
				Return True
			Else
				mErrorMessage = errorNum.ToString()
				Debug.WriteLine("Got error: " & errorNum)
				Return False
			End If
		End Function

		''' <summary>
		''' Gets the error message returned by the <see cref="Connect">Connect</see> and <see cref="Disconnect">Disconnect</see> functions.
		''' </summary>
		Public ReadOnly Property ErrorMessage() As String
			Get
				Return mErrorMessage
			End Get
		End Property
    End Class
End Namespace