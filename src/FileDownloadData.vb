Public Class FileDownloadData
    Private _url As String
    Private _fileName As String
    Public ReadOnly Property Url() As String
        Get
            Return _url
        End Get
    End Property
    Public ReadOnly Property FileName() As String
        Get
            Return _fileName
        End Get
    End Property
    Public Sub New(url As String, fileName As String)
        _url = url
        _fileName = fileName
    End Sub
End Class
