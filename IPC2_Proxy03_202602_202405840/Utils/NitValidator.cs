namespace IPC2_Proxy03_202602_202405840.Utils;

public static class NitValidator
{
    /// Algoritmo módulo 11 del PDF:
    ///  1) Multiplicar cada dígito por su posición (posición 1 = extremo derecho, contando desde el dígito verificador).
    ///  2) Sumar, mod 11, 11 - mod, mod 11; si es 10 → 'K'.
    /// El dígito verificador NO se multiplica.
    public static bool EsNitValido(string nit)
    {
        if (string.IsNullOrWhiteSpace(nit) || nit.Length < 2) return false;
        nit = nit.ToUpper().Trim();

        char verificador = nit[^1];
        string cuerpo = nit[..^1];

        int suma = 0;
        for (int i = 0; i < cuerpo.Length; i++)
        {
            if (!char.IsDigit(cuerpo[i])) return false;
            int posicion = cuerpo.Length - i + 1;
            suma += (cuerpo[i] - '0') * posicion;
        }

        int mod = suma % 11;
        int resta = 11 - mod;
        int final = resta % 11;
        string esperado = final == 10 ? "K" : final.ToString();
        return verificador.ToString() == esperado;
    }
}