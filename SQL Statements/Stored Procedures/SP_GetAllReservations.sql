CREATE PROCEDURE GetAllReservations
AS
BEGIN
    SELECT Id, ReservationNumber, ReservationDate, TotalPrice, BuffetType, SpecialRequest, Status, TableId, CustomerId, CreatedAt, UpdatedAt
    FROM Reservations;
END;
