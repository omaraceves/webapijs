static class CodeGenerator
{
    private static Queue<string> _codeQueue = new Queue<string>();

    private static void Initialize()
    {
        _codeQueue.Enqueue("ABCD");
        _codeQueue.Enqueue("XXXX");
        _codeQueue.Enqueue("AAAA");
        _codeQueue.Enqueue("BBBB");
        _codeQueue.Enqueue("CCCC");
        _codeQueue.Enqueue("RRRR");
        _codeQueue.Enqueue("RATO");
        _codeQueue.Enqueue("CATO");
        _codeQueue.Enqueue("FATO");
        _codeQueue.Enqueue("POUL");
        _codeQueue.Enqueue("AXFF");
        _codeQueue.Enqueue("ARFC");
        _codeQueue.Enqueue("WRTY");
        _codeQueue.Enqueue("WERT");
        _codeQueue.Enqueue("WSXC");
        _codeQueue.Enqueue("WSXZ");
        _codeQueue.Enqueue("WSAQ");
        _codeQueue.Enqueue("QASW");
        _codeQueue.Enqueue("QASD");
        _codeQueue.Enqueue("RTYY");
        _codeQueue.Enqueue("UIOP");
        _codeQueue.Enqueue("LKJH");
        _codeQueue.Enqueue("MGHJ");
    }
    
    public static string GetActivationCode() 
    {
        if(!_codeQueue.Any())
            Initialize();
   
        if(_codeQueue.TryDequeue(out var result))
            return result;
        else
            throw new Exception("Codes source is empty.");
    }

    public static void PushCode(string code)
    {
        _codeQueue.Enqueue(code);
    }
}