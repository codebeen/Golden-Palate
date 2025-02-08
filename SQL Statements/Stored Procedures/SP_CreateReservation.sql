CREATE PROCEDURE CreateReservation
    @ReservationDate DATE,
    @TotalPrice DECIMAL(10,2),
    @BuffetType VARCHAR(255),
    @SpecialRequest VARCHAR(255) = NULL,
    @TableId INT,
    @CustomerId INT
AS
BEGIN
    INSERT INTO Reservations (ReservationDate, TotalPrice, BuffetType, SpecialRequest, TableId, CustomerId)
    VALUES (@ReservationDate, @TotalPrice, @BuffetType, @SpecialRequest, @TableId, @CustomerId);
END;
