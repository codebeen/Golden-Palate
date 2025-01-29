CREATE PROCEDURE GetPaymentById
    @Id INT
AS
BEGIN
    SELECT Id, ReservationId, Amount, Description, CreatedAt
    FROM Payments
    WHERE Id = @Id;
END;
