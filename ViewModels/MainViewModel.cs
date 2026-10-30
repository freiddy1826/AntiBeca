using System.Collections.ObjectModel;
using System.Globalization;
using AntiBeca.Models;
using AntiBeca.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AntiBeca.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<string> Personas { get; } = new();
    public ObservableCollection<Articulo> Articulos { get; } = new();
    public ObservableCollection<ResumenPersona> Resumen { get; } = new();

    [ObservableProperty] private string nombrePersona = "";
    [ObservableProperty] private string nombreArticulo = "";
    [ObservableProperty] private string precioArticulo = "";
    [ObservableProperty] private string? personaSeleccionada;
    [ObservableProperty] private string propinaTexto = "10";
    [ObservableProperty] private string mensaje = "";
    [ObservableProperty] private decimal totalGeneral;

    partial void OnPropinaTextoChanged(string value) => Recalcular();

    [RelayCommand]
    private void AgregarPersona()
    {
        var nombre = NombrePersona.Trim();
        if (nombre.Length == 0)
        {
            Mensaje = "Escribe un nombre.";
            return;
        }
        if (Personas.Any(p => p.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
        {
            Mensaje = "Esa persona ya está en la lista.";
            return;
        }

        Personas.Add(nombre);
        NombrePersona = "";
        PersonaSeleccionada ??= nombre;
        Mensaje = "";
        Recalcular();
    }

    [RelayCommand]
    private void QuitarPersona(string nombre)
    {
        foreach (var a in Articulos.Where(a => a.Persona == nombre).ToList())
            Articulos.Remove(a);

        Personas.Remove(nombre);
        if (PersonaSeleccionada == nombre)
            PersonaSeleccionada = Personas.FirstOrDefault();
        Recalcular();
    }

    [RelayCommand]
    private void AgregarArticulo()
    {
        if (PersonaSeleccionada is null)
        {
            Mensaje = "Primero agrega una persona y elígela.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NombreArticulo))
        {
            Mensaje = "Escribe el nombre del artículo.";
            return;
        }
        if (!decimal.TryParse(PrecioArticulo, NumberStyles.Number,
                CultureInfo.CurrentCulture, out var precio) || precio <= 0)
        {
            Mensaje = "El precio no es válido.";
            return;
        }

        Articulos.Add(new Articulo
        {
            Nombre = NombreArticulo.Trim(),
            Precio = precio,
            Persona = PersonaSeleccionada
        });

        NombreArticulo = "";
        PrecioArticulo = "";
        Mensaje = "";
        Recalcular();
    }

    [RelayCommand]
    private void QuitarArticulo(Articulo articulo)
    {
        Articulos.Remove(articulo);
        Recalcular();
    }

    [RelayCommand]
    private void LimpiarTodo()
    {
        Articulos.Clear();
        Personas.Clear();
        PersonaSeleccionada = null;
        Mensaje = "";
        Recalcular();
    }

    [RelayCommand]
    private void FijarPropina(string porcentaje)
    {
        PropinaTexto = porcentaje;
    }

    private void Recalcular()
    {
        if (!decimal.TryParse(PropinaTexto, NumberStyles.Number,
                CultureInfo.CurrentCulture, out var porcentaje))
            porcentaje = 0;
        porcentaje = Math.Clamp(porcentaje, 0, 100);

        var lista = CalculadoraCuenta.Calcular(Personas, Articulos, porcentaje);

        Resumen.Clear();
        foreach (var r in lista)
            Resumen.Add(r);

        TotalGeneral = lista.Sum(r => r.Total);
    }
}