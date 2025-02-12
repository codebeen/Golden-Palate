CREATE PROCEDURE CreateReservation
	@ReservationNumber VARCHAR(255),
    @ReservationDate DATE,
    @TotalPrice DECIMAL(10,2),
    @BuffetType VARCHAR(255),
    @SpecialRequest VARCHAR(255) = NULL,
    @TableId INT,
    @CustomerId INT
AS
BEGIN
    INSERT INTO Reservations (ReservationNumber, ReservationDate, TotalPrice, BuffetType, SpecialRequest, TableId, CustomerId)
    VALUES (@ReservationNumber, @ReservationDate, @TotalPrice, @BuffetType, @SpecialRequest, @TableId, @CustomerId);
END;
