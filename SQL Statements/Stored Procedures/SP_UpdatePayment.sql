CREATE PROCEDURE UpdatePayment
    @Id INT,
    @ReservationId INT,
    @Amount DECIMAL(10,2),
    @Description VARCHAR(100)
AS
BEGIN
    UPDATE Payments
    SET ReservationId = @ReservationId,
        Amount = @Amount,
        Description = @Description,
        CreatedAt = GETDATE()
    WHERE Id = @Id;
END;
