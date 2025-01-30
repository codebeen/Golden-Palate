CREATE PROCEDURE CreateReservation
    @ReservationDate DATE,
    @ReservationTime TIME,
    @TotalPrice DECIMAL(10,2),
    @BuffetType VARCHAR(255),
    @SpecialRequest VARCHAR(255) = NULL,
    @TableId INT,
    @CustomerId INT
AS
BEGIN
    INSERT INTO Reservations (ReservationDate, ReservationTime, TotalPrice, BuffetType, SpecialRequest, TableId, CustomerId)
    VALUES (@ReservationDate, @ReservationTime, @TotalPrice, @BuffetType, @SpecialRequest, @TableId, @CustomerId);
END;
