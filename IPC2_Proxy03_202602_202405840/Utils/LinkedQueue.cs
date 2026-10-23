namespace IPC2_Proxy03_202602_202405840.Utils;

/// TDA: Cola enlazada FIFO implementada 
public class LinkedQueue<T>
{
    private class Nodo { public T Valor = default!; public Nodo? Siguiente; }
    private Nodo? _frente, _fin;

    public int Count { get; private set; }
    public bool IsEmpty => _frente is null;

    public void Enqueue(T item)
    {
        var n = new Nodo { Valor = item };
        if (_fin is null) { _frente = _fin = n; }
        else { _fin.Siguiente = n; _fin = n; }
        Count++;
    }

    public T Dequeue()
    {
        if (_frente is null) throw new InvalidOperationException("Cola vacía.");
        var v = _frente.Valor;
        _frente = _frente.Siguiente;
        if (_frente is null) _fin = null;
        Count--;
        return v;
    }
}