CREATE PROCEDURE UpdateReservation
    @Id INT,
    @ReservationDate DATE,
    @TotalPrice DECIMAL(10,2),
    @BuffetType VARCHAR(255),
    @SpecialRequest VARCHAR(255) = NULL,
    @Status VARCHAR(50),
    @TableId INT,
    @CustomerId INT
AS
BEGIN
    UPDATE Reservations
    SET ReservationDate = @ReservationDate,
        TotalPrice = @TotalPrice,
        BuffetType = @BuffetType,
        SpecialRequest = @SpecialRequest,
        Status = @Status,
        TableId = @TableId,
        CustomerId = @CustomerId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;
