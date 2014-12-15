Public Class listbox_item_data
    Public linkdovideo As String

    Public nomedovideo As String


    Public Sub New(ByVal NovoValor As String, ByVal NovaDescricao As String)

        linkdovideo = NovoValor

        nomedovideo = NovaDescricao

    End Sub



    Public Overrides Function ToString() As String

        Return nomedovideo

    End Function
End Class
