 namespace CubeLogic.TransactionsConverter.Entities;

 public record InputTransaction
 (
     string OrderId,
     string Type,
     string DateTime, 
     decimal Price,
     int InstrumentId 
 );