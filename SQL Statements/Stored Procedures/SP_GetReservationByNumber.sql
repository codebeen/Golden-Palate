CREATE PROCEDURE GetReservationByNumber
    @ReservationNumber VARCHAR(255)
AS
BEGIN
    SELECT Id,ReservationNumber, ReservationDate, TotalPrice, BuffetType, SpecialRequest, Status, TableId, CustomerId, CreatedAt, UpdatedAt
    FROM Reservations
    WHERE ReservationNumber = @ReservationNumber;
END;
