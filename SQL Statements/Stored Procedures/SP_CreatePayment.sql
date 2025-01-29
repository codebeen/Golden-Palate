CREATE PROCEDURE CreatePayment
    @ReservationId INT,
    @Amount DECIMAL(10,2),
    @Description VARCHAR(100)
AS
BEGIN
    INSERT INTO Payments (ReservationId, Amount, Description, CreatedAt)
    VALUES (@ReservationId, @Amount, @Description, GETDATE());
END;
