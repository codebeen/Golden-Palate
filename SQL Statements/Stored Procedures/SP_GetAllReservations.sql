CREATE PROCEDURE GetAllReservations
AS
BEGIN
    SELECT Id, ReservationDate, ReservationTime, TotalPrice, BuffetTypeId, SpecialRequest, Status, TableId, CustomerId, CreatedAt, UpdatedAt
    FROM Reservations;
END;
