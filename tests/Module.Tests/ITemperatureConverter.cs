namespace Lab.Interfaces;

public interface ITemperatureConverter
{
    double KelvinToCelsius(double value);
    double KelvinToFahrenheit(double value);
    double FahrenheitToCelsius(double value);
    double FahrenheitToKelvin(double value);
    double CelsiusToKelvin(double value);
    double CelsiusToFahrenheit(double value);
}