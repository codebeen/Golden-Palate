CREATE PROCEDURE CreateReservation
    @ReservationDate DATE,
    @ReservationTime TIME,
    @TotalPrice DECIMAL(10,2),
    @BuffetTypeId INT,
    @SpecialRequest VARCHAR(255) = NULL,
    @TableId INT,
    @CustomerId INT
AS
BEGIN
    INSERT INTO Reservations (ReservationDate, ReservationTime, TotalPrice, BuffetTypeId, SpecialRequest, TableId, CustomerId)
    VALUES (@ReservationDate, @ReservationTime, @TotalPrice, @BuffetTypeId, @SpecialRequest, @TableId, @CustomerId);
END;
