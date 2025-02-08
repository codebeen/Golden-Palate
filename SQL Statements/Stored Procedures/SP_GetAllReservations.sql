CREATE PROCEDURE GetAllReservations
AS
BEGIN
    SELECT Id, ReservationDate, TotalPrice, BuffetType, SpecialRequest, Status, TableId, CustomerId, CreatedAt, UpdatedAt
    FROM Reservations;
END;
