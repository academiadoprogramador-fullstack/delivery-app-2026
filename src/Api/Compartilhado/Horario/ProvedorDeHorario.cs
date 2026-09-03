using DeliveryApp.Dominio.Compartilhado;
using Microsoft.Extensions.Options;

namespace DeliveryApp.WebApi.Compartilhado.Horario;

public sealed class ProvedorDeHorario(
    IOptions<HorarioOptions> options,
    TimeProvider timeProvider
) : IProvedorDeHorario
{
    private readonly TimeZoneInfo fusoHorario = TimeZoneInfo.FindSystemTimeZoneById(
        options.Value.FusoHorario
    );

    public TimeOnly ObterHorarioAtual()
    {
        var dataLocal = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), fusoHorario);
        return TimeOnly.FromDateTime(dataLocal.DateTime);
    }

    public static bool FusoHorarioValido(string fusoHorario)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(fusoHorario);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
