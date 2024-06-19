Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Net.Cache
Imports System.Reflection
Imports System.Text
Imports System.Threading

''' <summary>
''' Code được viết vào những ngày khó khăn nhất của tôi !
''' RIP những ngày tháng 5 năm 2020.
''' 19/06/2024 Cũng đang trong tâm trạng như vậy.
''' 2CongLc.Vn@gmail.com
''' </summary>

Public Class Form1

    Private TotalFile As Integer = 0
    Private FileDownload As Integer = 0
    Private FileError As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs)

    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If Not String.IsNullOrWhiteSpace(TextBox1.Text) = True Then
            If Directory.Exists(TextBox1.Text) Then

                If Not TextBox2.Text.StartsWith("*.") Then MessageBox.Show("Định dạng đầu vào phải là *.") : Return

                Dim lst As List(Of String) = Enumerable.ToList(Of String)(Directory.GetFiles(TextBox1.Text, TextBox2.Text, SearchOption.AllDirectories))

                Dim sb As New StringBuilder()

                Dim resourceURL As String = String.Empty



                For i As Integer = 0 To lst.Count - 1
                    'Lấy thông tin tệp
                    Dim f As New FileInfo(lst(i))
                    'Lấy tên tệp tin
                    Dim fname As String = IO.Path.GetFileName(f.FullName)
                    'Lấy đường dẫn tệp tin
                    Dim fpath As String = IO.Path.GetDirectoryName(f.FullName)
                    'Lấy thư mục gốc để chuyển đổi thành url
                    Dim root As String = IO.Path.GetPathRoot(fpath)
                    'Lấy phần đường dẫn sau phần thư mục gốc
                    Dim body As String = fpath.Substring(root.Length, fpath.Length - root.Length)
                    'Kiểm tra đường dẫn địa chỉ url
                    Dim url As String = If(TextBox4.Text.EndsWith("/"), TextBox4.Text, TextBox4.Text + "/")
                    'Kiểm tra đường dẫn phần tệp tin
                    Dim tbody As String = If(body.EndsWith("\"), body, body + "\")
                    'kiểm tra đầu vào url
                    If String.IsNullOrWhiteSpace(TextBox4.Text) = False Then
                        resourceURL = url + tbody.Replace("\", "/") + fname
                    Else
                        resourceURL = f.FullName
                    End If
                    'kết xuất kết quả
                    sb.Append(resourceURL).AppendLine()
                Next i
                'Kiểm tra và tạo thư mục lưu trữ
                If Not Directory.Exists(IO.Directory.GetCurrentDirectory() & "\data") Then Directory.CreateDirectory(IO.Directory.GetCurrentDirectory() & "\data")
                'Tạo tên tệp tin
                Dim temp As String = "flist_" & DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss-tt") & ".txt"

                Dim outFile As String = String.Empty

                If String.IsNullOrWhiteSpace(TextBox3.Text) = False Then
                    outFile = TextBox3.Text
                Else
                    outFile = temp
                End If

                'Kết xuất tạo tệp
                File.WriteAllText(IO.Directory.GetCurrentDirectory() & "\data\" & outFile, sb.ToString())
                'Thông báo hoàn thành
                MessageBox.Show("Đã tạo xong sách tệp, và được lưu tại : " &
                                vbNewLine &
                               IO.Directory.GetCurrentDirectory() & "\data\" & outFile &
                                vbNewLine &
                                "Bạn có thể sử dụng tính năng downloader hoặc chương trình hỗ trợ như IDMan để tải danh sách")
            End If
        End If
    End Sub
    ''' <summary>
    ''' Đưa danh sách tệp vào combobox
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ComboBox1_Click(sender As Object, e As EventArgs) Handles ComboBox1.Click
        Dim files As String() = Directory.GetFiles(IO.Directory.GetCurrentDirectory() & "\data\", "*.txt", SearchOption.AllDirectories)
        ComboBox1.Items.Clear()
        ComboBox1.Items.AddRange(files)
    End Sub
    ''' <summary>
    ''' lựa chọn tệp trong danh sách tệp
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim cbo = DirectCast(sender, ComboBox)
        If cbo.SelectedIndex = -1 Then Return
        Dim f As New FileInfo(cbo.SelectedItem)
    End Sub

    Private Sub Form1_Load_1(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            TextBox6.Text = IO.Directory.GetCurrentDirectory() & "\Download\"
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            TextBox5.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Try
            If String.IsNullOrWhiteSpace(ComboBox1.Text) = False Then
                If Directory.Exists(ComboBox1.Text) OrElse File.Exists(ComboBox1.Text) Then
                    Process.Start("explorer.exe", "/select," & ComboBox1.Text)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Try


            'Kiểm tra đầu vào
            If String.IsNullOrWhiteSpace(ComboBox1.Text) = False Then
                If File.Exists(ComboBox1.Text) Then

                    Button5.Enabled = False
                    Button5.Text = "Đang tải ..."
                    FileDownload = 0
                    FileError = 0
                    TotalFile = 0
                    ProgressBar1.Value = 0
                    ProgressBar2.Value = 0
                    ProgressBar3.Value = 0

                    'Đọc tất cả các dòng có trong tệp
                    Dim src As String() = File.ReadAllLines(ComboBox1.Text)
                    'ghi nhật ký các tệp đã tải
                    Dim tempFiles As New StringBuilder()
                    'Tổng số tệp trong danh sách
                    TotalFile = src.Count()
                    ProgressBar2.Maximum = TotalFile
                    ProgressBar3.Maximum = TotalFile
                    'Kiểm tra thư mục lưu
                    Dim projectName As String = String.Empty
                    If String.IsNullOrWhiteSpace(TextBox5.Text) = False Then
                        projectName = TextBox5.Text
                    Else
                        projectName = "project1"
                    End If
                    Dim projectLocal As String = String.Empty
                    If String.IsNullOrWhiteSpace(TextBox6.Text) = False Then
                        projectLocal = TextBox6.Text & "\" & projectName
                    Else
                        projectLocal = IO.Directory.GetCurrentDirectory() & "\Download\" & projectName
                    End If
                    If Not Directory.Exists(projectLocal) Then Directory.CreateDirectory(projectLocal)


                    'Tiến hành dò tìm tệp
                    For i As Integer = 0 To src.Count - 1

                        Dim u As Uri = New Uri(src(i))
                        'Lấy giao thức http:// hay https://
                        Dim protocol As String = u.Scheme
                        'Lấy tên Domain 
                        Dim uhost As String = u.Host
                        'Lấy cổng giao tiếp
                        Dim uport As String = u.Port
                        'Lấy đường dẫn sau domain
                        Dim upath As String = u.LocalPath
                        'Lấy tên tệp tin
                        Dim uname As String = String.Empty
                        If u.IsFile = True Then
                            uname = Path.GetFileName(upath)
                        End If

                        'Tạo đường dẫn têp tin trên máy tính
                        Dim flocal As String = projectLocal + upath.Replace("/", "\")

                        'Lấy tên tệp tin trên máy tính
                        Dim fname As String = Path.GetFileName(flocal)

                        'Kiểm tra đường dẫn
                        Dim fpath As String = Path.GetDirectoryName(flocal)

                        Try
                            'Tạo thư mục chứa tệp tin
                            If Not Directory.Exists(fpath) Then Directory.CreateDirectory(fpath)

                            'Tiến hành tải tệp tin
                            Dim wc As WebClient = New WebClient()

                            AddHandler wc.DownloadFileCompleted, New AsyncCompletedEventHandler(AddressOf wc_DownloadFileCompleted)
                            AddHandler wc.DownloadProgressChanged, New DownloadProgressChangedEventHandler(AddressOf wc_DownloadProgressChanged)

                            wc.DownloadFileAsync(u, flocal)

                            'Lưu danh sách tệp đã tải
                            tempFiles.Append(flocal).AppendLine()

                        Catch ex As Exception

                        End Try
                    Next i

                    'Kiểm tra và tạo thư mục lưu trữ & lỗi
                    If Not Directory.Exists(IO.Directory.GetCurrentDirectory() & "\temp") Then Directory.CreateDirectory(IO.Directory.GetCurrentDirectory() & "\temp")
                    'Kết xuất tạo tệp trong thư mục temp & error
                    File.WriteAllText(IO.Directory.GetCurrentDirectory() & "\temp\" & projectName & "-tmp.txt", tempFiles.ToString())

                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub wc_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs)
        Try
            If (e.Error IsNot Nothing) Then
                '   MessageBox.Show("Error: " + e.Error.Message)
                FileError += 1
                ProgressBar3.Value = FileError
                Dim Failed As Double = (FileError / TotalFile) * 100.0
                Dim TotalFail As Integer = Integer.Parse(Math.Truncate(Failed).ToString())
                Label16.Text = TotalFail & " %"
            Else
                '    MessageBox.Show("DownloadCompleted !")

                FileDownload += 1
                ProgressBar2.Value = FileDownload
                Dim Completed As Double = (FileDownload / TotalFile) * 100.0
                Dim TotalDownload As Integer = Integer.Parse(Math.Truncate(Completed).ToString())
                Label12.Text = TotalDownload & " %"
            End If
            If (FileDownload + FileError) = TotalFile Then
                Button5.Enabled = True
                Button5.Text = "Bắt đầu"
                MessageBox.Show("Đã tải xong")
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Sub wc_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs)
        Dim bytesIn As Double = Double.Parse(e.BytesReceived.ToString())
        Dim totalBytes As Double = Double.Parse(e.TotalBytesToReceive.ToString())
        Dim percentage As Double = bytesIn / totalBytes * 100.0
        Dim percente As Integer = Integer.Parse(Math.Truncate(percentage).ToString())
        ProgressBar1.Value = percente


        Label9.Text = String.Format("{0} MB / {1} MB", (e.BytesReceived / 1024.0R / 1024.0R).ToString("0.00"), (e.TotalBytesToReceive / 1024.0R / 1024.0R).ToString("0.00"))
        Label10.Text = percente & " %"

    End Sub

End Class
