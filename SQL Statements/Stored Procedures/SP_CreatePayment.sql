CREATE PROCEDURE CreatePayment
    @Amount DECIMAL(10,2),
    @Description VARCHAR(255) = NULL,
    @ReservationId INT,
	@UserId INT,
    @ModeOfPayment VARCHAR(50)
AS
BEGIN
    INSERT INTO Payments(Amount, Description, ReservationId, UserId, ModeOfPayment)
    VALUES (@Amount, @Description, @ReservationId, @UserId, @ModeOfPayment)
END
