CREATE PROCEDURE UpdateReservation
    @Id INT,
    @ReservationDate DATE,
    @ReservationTime TIME,
    @TotalPrice DECIMAL(10,2),
    @BuffetTypeId INT,
    @SpecialRequest VARCHAR(255) = NULL,
    @Status VARCHAR(50),
    @TableId INT,
    @CustomerId INT
AS
BEGIN
    UPDATE Reservations
    SET ReservationDate = @ReservationDate,
        ReservationTime = @ReservationTime,
        TotalPrice = @TotalPrice,
        BuffetTypeId = @BuffetTypeId,
        SpecialRequest = @SpecialRequest,
        Status = @Status,
        TableId = @TableId,
        CustomerId = @CustomerId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;
