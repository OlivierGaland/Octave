Public Class Class1
    Implements GOverlayPlugin.Interfaces.IPlugin
    Private objHost As GOverlayPlugin.Interfaces.IHost
    Public Sub Initialize(ByVal Host As GOverlayPlugin.Interfaces.IHost) Implements GOverlayPlugin.Interfaces.IPlugin.Initialize
        objHost = Host
    End Sub
    Public ReadOnly Property Name() As String Implements GOverlayPlugin.Interfaces.IPlugin.Name
        'Return your plugin name
        Get
            Return "Octave (Arduino basic plugin)"
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
            Return "Arduino plugin on COM port" & vbCrLf & vbCrLf & "Compatible Arduino API :" & vbCrLf & "Serial port communication commands :" & vbCrLf & "GET : return sensor state (should return formatted : GET|sensor1:val1|sensor2:val2|...|sensorn:valn)"
        End Get
    End Property
    Function CallBack(method As String) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.CallBacks
        'Not available Yet
    End Function
    Function ComboBoxes() As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.ComboBoxes
        'Create custom ComboBox for your configuration to use
        Dim boxes As New Hashtable
        Dim myboxOptions As New Hashtable
        'Set each one of the Combobox options as value, Display Name
        myboxOptions.Add("text1", "Display Hello World")
        myboxOptions.Add("text2", "Display Bye World")

        'Add the combobox options as the combobox "Octave.textSelector"
        boxes.Add("Octave.textSelector", myboxOptions)
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
        options.Add(2, New ArrayList({"ComboYesNo", "Append Text", "appendyesno"}))
        options.Add(3, New ArrayList({"Text", "Append to the text", "append"}))
        Return options
    End Function
    Public Function PluginOptionsDefault() As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.PluginOptionsDefault
        'Set the default values you want to have on plugin, if the user doesnt change any option, he will have this settings
        Dim options As New Hashtable
        options("appendyesno") = "No"
        options("append") = ""
        options("port") = "0"
        options("speed") = "9600"
        Return options
    End Function
    Function ReceiveSerialData() As String
        ' Receive strings from a serial port.
        Dim returnStr As String = "NA"
        Dim com As IO.Ports.SerialPort = Nothing
        Try
            com = My.Computer.Ports.OpenSerialPort("COM4", 9600)
            com.ReadTimeout = 1000
            com.WriteLine("GET")
            Dim Incoming As String = com.ReadLine()
            If Not Incoming Is Nothing Then
                returnStr &= Incoming & vbCrLf
            End If
        Catch ex As TimeoutException
            returnStr = "Error: Serial Port read timed out."
        Finally
            If com IsNot Nothing Then com.Close()
        End Try
        Return returnStr
    End Function
    Function AvailableSensors(pluginOptions As Hashtable) As System.Collections.Generic.Dictionary(Of String, String) Implements GOverlayPlugin.Interfaces.IPlugin.AvailableSensors
        'Create the list of the sensors/elements this plugin has
        'You can access your pluginOptions here as pluginOptions(your_option)    
        'Options: SensorTag, Sensor Display-Name
        Dim sensors As New System.Collections.Generic.Dictionary(Of String, String)
        Dim strArr() As String
        Dim count As Integer
        strArr = ReceiveSerialData().Split("|")
        'If String.Compare(strArr(0), "GET") = 0 Then
        For count = 1 To strArr.Length - 1
            sensors.Add("Octave." & strArr(count).Split(":")(0), strArr(count).Split(":")(0))
        Next
        'End If
        Return sensors
    End Function
    Function LCDSys2_AvailableSensors(pluginOptions As Hashtable) As System.Collections.Generic.Dictionary(Of String, String) Implements GOverlayPlugin.Interfaces.IPlugin.LCDSys2_AvailableSensors
        Return AvailableSensors(pluginOptions)
    End Function
    Public Function CreateOptions(sensorId As String, elementData As Hashtable) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.CreateOptions
        'Set the options the user will have when clicking on the element

        Dim options As New Hashtable
        If sensorId = "Octave.helloworld" Then
            'Option: option_index as integer, option_data as ArrayList
            'Option_Data: option_type as string, option_label as string, option_name as string (no spaces, no _), help text
            options.Add(0, New ArrayList({"ColorBasic", "Color of the Text", "color", "Pick a color for your text"}))
            options.Add(1, New ArrayList({"Octave.textSelector", "Text to Display", "textSelected", "Do you want to display HelloWorld or ByeWorld?"}))
        End If

        Return options

    End Function
    Public Function LCDSys2_CreateOptions(sensorId As String, elementData As Hashtable) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.LCDSys2_CreateOptions
        Return CreateOptions(sensorId, elementData)
    End Function
    Public Function SetDefaultOptions(sensorId As String, elementData As Hashtable) As Hashtable Implements GOverlayPlugin.Interfaces.IPlugin.SetDefaultOptions
        'Set the default values you want to have on your sensor when its created, if the user doesnt change any option, he will have this settings

        If sensorId = "Octave.helloworld" Then
            elementData("width") = 100
            elementData("height") = 41
            elementData("color") = 0
            elementData("textSelected") = "text1"
        Else
            elementData("width") = 50   'There must be at least one width and height set, otherwise the element wont show on the display-emulator window because it has no size
            elementData("height") = 50
        End If

        Return elementData
    End Function
    Public Function DisplayOnLCD(sensorId As String, elementData As Hashtable, pluginOptions As Hashtable, cacheRuns As Integer) As ArrayList Implements GOverlayPlugin.Interfaces.IPlugin.DisplayOnLCD
        'Draw on the screen

        If sensorId = "Octave.helloworld" Then
            Dim x = elementData("x")    'grab X position of the element
            Dim y = elementData("y")    'grab Y position of the element

            Dim commandList As New ArrayList()
            Dim textToDraw As String = ""
            If elementData("textSelected") = "text1" Then
                textToDraw = "Hello World"
            Else
                textToDraw = "Bye World"
            End If

            If pluginOptions("appendyesno") = "Yes" Then
                textToDraw = textToDraw & pluginOptions("append")
            End If

            'Draw Text (command as string, x as integer, y as integer, text as string, reserve_width as integer, unused as bool, unused as bool, unused as integer, basic_color as integer
            commandList.Add(New ArrayList({"text", x, y, textToDraw, 0, False, False, 0, elementData("color")}))

        End If
    End Function
    Public Function LCDSys2_DisplayOnLCD(sensorId As String, elementData As Hashtable, pluginOptions As Hashtable, cacheRuns As Integer) As ArrayList Implements GOverlayPlugin.Interfaces.IPlugin.LCDSys2_DisplayOnLCD
        Return DisplayOnLCD(sensorId, elementData, pluginOptions, cacheRuns)
    End Function
    Function SensorHasCustomDraw(sensor_name As String) As Boolean Implements GOverlayPlugin.Interfaces.IPlugin.SensorHasCustomDraw
        'All sensors are drawn with GOverlay default drawing
        Return False
    End Function
End Class

