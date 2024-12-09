namespace CubeLogic.TransactionsConverter.Entities;

public record Config
(
    string Timezone,
    List<Instrument> Instruments
);