using NUnit.Framework;
using Lab.Interfaces;
using System.Linq;

namespace Lab.Tests;

[TestFixture(typeof(Lab.Implementations.GenCode1.TemperatureConverter), Category = "GenCode1")]
[TestFixture(typeof(Lab.Implementations.GenCode2.TemperatureConverter), Category = "GenCode2")]
[TestFixture(typeof(Lab.Implementations.GenCode3.TemperatureConverter), Category = "GenCode3")]

public class TemperatureConverterTests
{
    private ITemperatureConverter _converter;

    public TemperatureConverterTests(Type type)
    {
        _converter = (ITemperatureConverter)Activator.CreateInstance(type);
    }

    // ---------- Тесты "чёрного ящика" (на основе спецификации) ----------

    [Test]
    // Пограничное значение: абсолютный ноль в цельсиях в кельвины (-273.15°C)
    public void CelsiusToKelvin_AtAbsoluteZero_ReturnsZeroKelvin()
    {
        double result = _converter.CelsiusToKelvin(-273.15);
        Assert.That(result, Is.EqualTo(0).Within(0.001));
    }

    [Test]
    // Пограничное значение: абсолютный ноль в цельсиях в фаренгейты (-273.15°C)
    public void CelsiusToFahrenheit_AtAbsoluteZero_ReturnsZeroFahrenheit()
    {
        double result = _converter.CelsiusToFahrenheit(-273.15);
        Assert.That(result, Is.EqualTo(-459.67).Within(0.001));
    }

    [Test]
    // Пограничное значение: абсолютный ноль в фаренгейтах в кельвины (-459.67°F)
    public void FahrenheitToKelvin_AtAbsoluteZero_ReturnsZeroKelvin()
    {
        double result = _converter.FahrenheitToKelvin(-459.67);
        Assert.That(result, Is.EqualTo(0).Within(0.001));
    }

    [Test]
    // Пограничное значение: абсолютный ноль в фаренгейтах в цельсии (-459.67°F)
    public void FahrenheitToCelsius_AtAbsoluteZero_ReturnsZeroCelsius()
    {
        double result = _converter.FahrenheitToCelsius(-459.67);
        Assert.That(result, Is.EqualTo(-273.15).Within(0.001));
    }

    [Test]
    // Пограничное значение: абсолютный ноль в кельвинах (0 K)
    public void KelvinToCelsius_AtAbsoluteZero_ReturnsMinus273Point15()
    {
        double result = _converter.KelvinToCelsius(0);
        Assert.That(result, Is.EqualTo(-273.15).Within(0.001));
    }

    [Test]
    // Пограничное значение: абсолютный ноль в кельвинах (0 K)
    public void KelvinToFahrenheit_AtAbsoluteZero_ReturnsMinus459Point67()
    {
        double result = _converter.KelvinToFahrenheit(0);
        Assert.That(result, Is.EqualTo(-459.67).Within(0.001));
    }

    [Test]
    // Значение ниже абсолютного нуля в цельсиях (должно выбросить исключение)
    public void CelsiusToKelvin_BelowAbsoluteZero_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _converter.CelsiusToKelvin(-274));
    }

    [Test]
    // Значение ниже абсолютного нуля в фаренгейтах (должно выбросить исключение)
    public void FahrenheitToKelvin_BelowAbsoluteZero_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _converter.FahrenheitToKelvin(-460));
    }

    [Test]
    // Значение ниже абсолютного нуля в кельвинах (должно выбросить исключение)
    public void KelvinToCelsius_BelowAbsoluteZero_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _converter.KelvinToCelsius(-0.1));
    }

    [Test]
    // Значение точно на границе допустимого диапазона (должно работать)
    public void CelsiusToKelvin_JustAboveAbsoluteZero_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _converter.CelsiusToKelvin(-273.1499999));
    }

    [Test]
    // Значение точно на границе недопустимого диапазона (должно выбрасывать исключение)
    public void CelsiusToKelvin_JustBelowAbsoluteZero_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _converter.CelsiusToKelvin(-273.1500001));
    }

    [Test]
    // Проверка через Convert.ToDouble
    public void AllMethods_WithStringParsing_ThrowsFormatException()
    {
        string invalidInput = "abc";

        // Попытка преобразовать строку в число вызовет исключение
        Assert.Throws<FormatException>(() => { 
            double value = double.Parse(invalidInput); 
            _converter.CelsiusToFahrenheit(value);});
    }

    [Test]
    // Проверка обработки double.NaN
    public void AllMethods_WithNaN_HandlesGracefully()
    {
        double notANumber = double.NaN;

        // Проверяем что методы не падают с исключением
        Assert.DoesNotThrow(() => _converter.CelsiusToFahrenheit(notANumber));
        Assert.DoesNotThrow(() => _converter.CelsiusToKelvin(notANumber));
        Assert.DoesNotThrow(() => _converter.FahrenheitToCelsius(notANumber));
        Assert.DoesNotThrow(() => _converter.FahrenheitToKelvin(notANumber));
        Assert.DoesNotThrow(() => _converter.KelvinToCelsius(notANumber));
        Assert.DoesNotThrow(() => _converter.KelvinToFahrenheit(notANumber));

        // Проверяем что результат тоже NaN
        double result = _converter.CelsiusToFahrenheit(notANumber);
        Assert.That(double.IsNaN(result), Is.True, "Результат должен быть NaN");
    }

    // ---------- Тесты "белого ящика" (на основе анализа реализации) ----------

    [Test]
    // Значения абсолютного нуля должны быть согласованы между собой
    public void AllMethods_CheckConstantsUsage_AbsoluteZeroValuesAreConsistent()
    {
        double absoluteZeroInCelsius = -273.15;

        // Конвертируем 0K в Цельсий (должно быть -273.15)
        double kelvinToCelsius = _converter.KelvinToCelsius(0);
        Assert.That(kelvinToCelsius, Is.EqualTo(absoluteZeroInCelsius).Within(0.001), "0K должен конвертироваться в -273.15°C");

        // Конвертируем -273.15°C в Кельвины (должно быть 0K)
        double celsiusToKelvin = _converter.CelsiusToKelvin(-273.15);
        Assert.That(celsiusToKelvin, Is.EqualTo(0).Within(0.001), "-273.15°C должен конвертироваться в 0K");

        // Конвертируем -459.67°F в Кельвины (должно быть 0K)
        double fahrenheitToKelvin = _converter.FahrenheitToKelvin(-459.67);
        Assert.That(fahrenheitToKelvin, Is.EqualTo(0).Within(0.001), "-459.67°F должен конвертироваться в 0K");
    }

    [Test]
    // Проверка обработки double.Epsilon (самое маленькое положительное число)
    public void AllMethods_HandleEpsilon_WithoutCrashing()
    {
        double epsilon = double.Epsilon;

        Assert.DoesNotThrow(() => _converter.CelsiusToFahrenheit(epsilon));
        Assert.DoesNotThrow(() => _converter.CelsiusToKelvin(epsilon));
        Assert.DoesNotThrow(() => _converter.FahrenheitToCelsius(epsilon));
        Assert.DoesNotThrow(() => _converter.FahrenheitToKelvin(epsilon));
        Assert.DoesNotThrow(() => _converter.KelvinToCelsius(epsilon));
        Assert.DoesNotThrow(() => _converter.KelvinToFahrenheit(epsilon));
    }

    [Test]
    // Проверка обработки double.MaxValue (самое большое число)
    public void AllMethods_HandleMaxValue_WithoutCrashing()
    {
        double maxValue = double.MaxValue;

        Assert.DoesNotThrow(() => _converter.CelsiusToFahrenheit(maxValue));
        Assert.DoesNotThrow(() => _converter.CelsiusToKelvin(maxValue));
    }
    [Test]
    // Проверка на переполнение
    public void AllMethods_EdgeCases_ExtremeHighValues()
    {
        double veryHighCelsius = 1e6; // 1 миллион градусов
        double veryHighFahrenheit = 1e6;
        double veryHighKelvin = 1e6;

        Assert.DoesNotThrow(() => _converter.CelsiusToFahrenheit(veryHighCelsius),
            "Метод CelsiusToFahrenheit должен обрабатывать очень высокие значения");

        Assert.DoesNotThrow(() => _converter.CelsiusToKelvin(veryHighCelsius),
            "Метод CelsiusToKelvin должен обрабатывать очень высокие значения");

        Assert.DoesNotThrow(() => _converter.FahrenheitToCelsius(veryHighFahrenheit),
            "Метод FahrenheitToCelsius должен обрабатывать очень высокие значения");

        Assert.DoesNotThrow(() => _converter.FahrenheitToKelvin(veryHighFahrenheit),
            "Метод FahrenheitToKelvin должен обрабатывать очень высокие значения");

        Assert.DoesNotThrow(() => _converter.KelvinToCelsius(veryHighKelvin),
            "Метод KelvinToCelsius должен обрабатывать очень высокие значения");

        Assert.DoesNotThrow(() => _converter.KelvinToFahrenheit(veryHighKelvin),
            "Метод KelvinToFahrenheit должен обрабатывать очень высокие значения");
    }

    [Test]
    // Точность вычислений должна сохраняться для разных значений
    public void AllMethods_Precision_ShouldMaintainPrecision()
    {
        var testValues = new[]
        {
            -273.15,  // Абсолютный ноль
            -40.0,    // Точка совпадения C и F
            0.0,      // Ноль
            20.5,     // Комнатная температура
            36.6,     // Температура тела
            100.0,    // Кипение воды
            1000.0    // Высокая температура
        };

        foreach (double celsius in testValues)
        {
            if (celsius >= -273.15)
            {
                // Тест: C -> F -> C
                double fahrenheit = _converter.CelsiusToFahrenheit(celsius);
                double backToCelsius = _converter.FahrenheitToCelsius(fahrenheit);
                Assert.That(backToCelsius, Is.EqualTo(celsius).Within(1e-10),
                    $"Потеря точности при конвертации C->F->C для значения {celsius}");

                // Тест: C -> K -> C
                double kelvin = _converter.CelsiusToKelvin(celsius);
                double backToCelsius2 = _converter.KelvinToCelsius(kelvin);
                Assert.That(backToCelsius2, Is.EqualTo(celsius).Within(1e-10),
                    $"Потеря точности при конвертации C->K->C для значения {celsius}");
            }
        }
    }

}