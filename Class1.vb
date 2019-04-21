Public Class Class1
    Implements GOverlayPlugin.Interfaces.IPlugin
    Private objHost As GOverlayPlugin.Interfaces.IHost

    Public last_state As System.Collections.Generic.Dictionary(Of String, String)
    Public last_update As DateTime = DateTime.Now()
    Public refresh_period As TimeSpan = New TimeSpan(0, 0, 10)
    Public connection As IO.Ports.SerialPort = Nothing
    Public Sub Initialize(ByVal Host As GOverlayPlugin.Interfaces.IHost) Implements GOverlayPlugin.Interfaces.IPlugin.Initialize
        objHost = Host
    End Sub
    Public ReadOnly Property Name() As String Implements GOverlayPlugin.Interfaces.IPlugin.Name
        'Return your plugin name
        Get
            Return "Octave (Serial port plugin)"
        End Get
    End Property
    Public ReadOnly Property Display() As String Implements GOverlayPlugin.Interfaces.IPlugin.Display
        'Return the display this plugin belongs to
        Get
            Return "lcdsys"
        End Get
    End Property
    Public ReadOnly Property Description() As String Implements GOverlayPlugin.Interfaces.IPlugin.Description
        'Return the description of this plugin
        Get
            Return "Serial port plugin " & vbCrLf & vbCrLf &
                    "Compatible API on serial port :" & vbCrLf &
                    "Request : GET" & vbCrLf &
                    "Reply : GET|sensor1:val1|sensor2:val2|...|sensorn:valn" & vbCrLf &
                    "No other text should pass on serial com on request send"
        End Get
    End Property
    Function CallBack(method As String) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.CallBacks
        'Send the request to upload all the files
        Dim returnHT As Hashtable = New Hashtable
        Dim value As String = "NA"

        If method = "willrequestvalues" Then
            'objHost.DebugMessage("Callback")
            'Comes here once per run (only if sensors are used thru here)
            If TimeSpan.Compare(DateTime.Now() - last_update, refresh_period) = 1 Then
                Connect()
                ReceiveSerialData()
                Disconnect()
            End If
        ElseIf method = "willrequestdisplay" Then
            'objHost.DebugMessage("willrequestdisplay")
        Else
            value = last_state.Item(method)
        End If

        returnHT("value") = value

        Return returnHT
    End Function
    Function ComboBoxes() As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.ComboBoxes
        'Create custom ComboBox for your configuration to use
        Dim boxes As New Hashtable
        Return boxes
    End Function
    Public Function PluginOptions(pluginCurrentOptions As Hashtable) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.PluginOptions
        'Set the options the user will have when going to the plugins tab and clicking on your plugin
        'The availalbe option_type are the same as CreateOptions function
        Dim options As New Hashtable
        'Option: option_index as integer, option_data as ArrayList
        'Option_Data: option_type as string, option_label as string, option_name as string (no spaces, no _)
        options.Add(0, New ArrayList({"Text", "COM port number", "port"}))
        options.Add(1, New ArrayList({"Text", "COM speed (baud)", "speed"}))
        options.Add(2, New ArrayList({"Text", "COM Timeout (ms)", "timeout"}))
        options.Add(3, New ArrayList({"Text", "Refresh rate (s) (> Timeout)", "refresh"}))
        Return options
    End Function
    Public Function PluginOptionsDefault() As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.PluginOptionsDefault
        'Set the default values you want to have on plugin, if the user doesnt change any option, he will have this settings
        ' objHost.DebugMessage("PluginOptionsDefault")
        Dim options As New Hashtable
        options("port") = "0"
        options("speed") = "9600"
        options("timeout") = "1000"
        options("refresh") = "10"
        Return options
    End Function
    Sub ReceiveSerialData()
        ' Receive strings from a serial port.
        'objHost.DebugMessage("ReceiveSerialData")
        last_update = DateTime.Now()
        Dim state As New System.Collections.Generic.Dictionary(Of String, String)
        If connection.IsOpen Then
            connection.WriteLine("GET")
            Dim strArr() As String = connection.ReadLine().Split("|")
            If strArr(0).Equals("GET") Then
                For count = 1 To strArr.Length - 1
                    state.Add("Octave." & strArr(count).Split(":")(0), strArr(count).Split(":")(1))
                Next
                last_state = state
            End If
        Else
            connection.Open()
        End If
    End Sub
    Public Function NewConnectionNeeded(pluginOptions As Hashtable) As Boolean
        If connection Is Nothing Then
            Return True
        ElseIf Not connection.PortName.Equals("COM" & pluginOptions("port")) Or connection.BaudRate <> Convert.ToInt32(pluginOptions("speed")) Or connection.ReadTimeout <> Convert.ToInt32(pluginOptions("timeout")) Then
            Return True
        Else
            Return False
        End If
    End Function
    Sub Connect(pluginOptions As Hashtable)
        If NewConnectionNeeded(pluginOptions) Then
            Try
                'objHost.DebugMessage("NewConnectionNeeded : creating")
                connection = My.Computer.Ports.OpenSerialPort("COM" & pluginOptions("port"), Convert.ToInt32(pluginOptions("speed")))
                connection.ReadTimeout = Convert.ToInt32(pluginOptions("timeout"))
            Catch
                objHost.DebugMessage("FATAL : Cannot open COM port")
                connection = Nothing
            End Try
        ElseIf Not connection.IsOpen() Then
            Try
                connection.Open()
            Catch
                objHost.DebugMessage("FATAL : Cannot re-open COM port")
                connection = Nothing
            End Try
        End If
    End Sub
    Sub Connect()
        Try
            If Not connection.IsOpen() Then
                connection.Open()
            End If
        Catch
            objHost.DebugMessage("FATAL : Cannot re-open COM port")
            connection = Nothing
        End Try
    End Sub
    Sub Disconnect()
        If Not connection Is Nothing Then
            connection.Close()
        End If
    End Sub
    Function AvailableSensors(pluginOptions As Hashtable) As System.Collections.Generic.Dictionary(Of String, String) Implements GOverlayPlugin.Interfaces.IPlugin.AvailableSensors
        'Create the list of the sensors/elements this plugin has
        'You can access your pluginOptions here as pluginOptions(your_option)    
        'Options: SensorTag, Sensor Display-Name
        'objHost.DebugMessage("AvailableSensors")
        Dim sensors As New System.Collections.Generic.Dictionary(Of String, String)

        ' Kind of secondary init here, not sure if it's the best place ... create all persistent data (serial connection) and copy options that cannot be retrieved in Callback
        'Try
        Connect(pluginOptions)
        'connection = My.Computer.Ports.OpenSerialPort("COM" & pluginOptions("port"), Convert.ToInt32(pluginOptions("speed")))
        'connection.ReadTimeout = Convert.ToInt32(pluginOptions("timeout"))
        refresh_period = New TimeSpan(0, 0, Convert.ToInt32(pluginOptions("refresh")))
        ReceiveSerialData()
        Disconnect()
        'Catch
        'objHost.DebugMessage("FATAL : Cannot open COM port")
        'End Try

        For Each kvp As KeyValuePair(Of String, String) In last_state
            sensors.Add(kvp.Key, Replace(kvp.Key, "Octave.", ""))
        Next
        Return sensors
    End Function
    Function LCDSys2_AvailableSensors(pluginOptions As Hashtable) As System.Collections.Generic.Dictionary(Of String, String) Implements GOverlayPlugin.Interfaces.IPlugin.LCDSys2_AvailableSensors
        Return AvailableSensors(pluginOptions)
    End Function
    Public Function CreateOptions(sensorId As String, elementData As Hashtable) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.CreateOptions
        'Set the options the user will have when clicking on the element
        Dim options As New Hashtable
        Return options
    End Function
    Public Function LCDSys2_CreateOptions(sensorId As String, elementData As Hashtable) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.LCDSys2_CreateOptions
        Return CreateOptions(sensorId, elementData)
    End Function
    Public Function SetDefaultOptions(sensorId As String, elementData As Hashtable) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.SetDefaultOptions
        'Set the default values you want to have on your sensor when its created, if the user doesnt change any option, he will have this settings

        elementData("width") = 50   'There must be at least one width and height set, otherwise the element wont show on the display-emulator window because it has no size
        elementData("height") = 50

        Return elementData
    End Function
    Public Function DisplayOnLCD(sensorId As String, elementData As Hashtable, pluginOptions As Hashtable, cacheRuns As Integer) As ArrayList Implements GOverlayPlugin.Interfaces.IPlugin.DisplayOnLCD
        Connect(pluginOptions)

        'Draw on the screen
        Dim x = elementData("x")    'grab X position of the element
        Dim y = elementData("y")    'grab Y position of the element

        Dim commandList As New ArrayList()
        Dim textToDraw As String = "TOTO"

        'Draw Text (command as string, x as integer, y as integer, text as string, reserve_width as integer, unused as bool, unused as bool, unused as integer, basic_color as integer
        commandList.Add(New ArrayList({"text", x, y, textToDraw, 0, False, False, 0, elementData("color")}))

        Return commandList
    End Function
    Public Function LCDSys2_DisplayOnLCD(sensorId As String, elementData As Hashtable, pluginOptions As Hashtable, cacheRuns As Integer) As ArrayList Implements GOverlayPlugin.Interfaces.IPlugin.LCDSys2_DisplayOnLCD
        Return DisplayOnLCD(sensorId, elementData, pluginOptions, cacheRuns)
    End Function
    Function SensorHasCustomDraw(sensor_name As String) As Boolean Implements GOverlayPlugin.Interfaces.IPlugin.SensorHasCustomDraw
        'All sensors are drawn with GOverlay default drawing
        Return False
    End Function
End Class

