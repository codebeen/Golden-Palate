CREATE PROCEDURE UpdateReservationStatus
    @Id INT,
    @Status VARCHAR(50)
AS
BEGIN
    UPDATE Reservations
    SET Status = @Status,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;