using AntiBeca.Models;

namespace AntiBeca.Services;

public static class CalculadoraCuenta
{
    public static List<ResumenPersona> Calcular(
        IEnumerable<string> personas,
        IEnumerable<Articulo> articulos,
        decimal propinaPorcentaje)
    {
        var resultado = new List<ResumenPersona>();

        foreach (var persona in personas)
        {
            var subtotal = articulos
                .Where(a => a.Persona == persona)
                .Sum(a => a.Precio);

            var propina = Math.Round(subtotal * propinaPorcentaje / 100m, 2,
                                     MidpointRounding.AwayFromZero);

            resultado.Add(new ResumenPersona(persona, subtotal, propina, subtotal + propina));
        }

        return resultado;
    }
}