CREATE PROCEDURE GetReservationById
    @Id INT
AS
BEGIN
    SELECT Id, ReservationDate, ReservationTime, TotalPrice, BuffetTypeId, SpecialRequest, Status, TableId, CustomerId, CreatedAt, UpdatedAt
    FROM Reservations
    WHERE Id = @Id;
END;
