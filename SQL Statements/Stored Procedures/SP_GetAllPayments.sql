CREATE PROCEDURE GetAllPayments
AS
BEGIN
    SELECT Id, ReservationId, Amount, Description, CreatedAt
    FROM Payments;
END;
