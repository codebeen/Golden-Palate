CREATE PROCEDURE GetAllPaymentDetailsFromView
AS
BEGIN
    SELECT 
		Id,
		Amount,
		Description,
		ModeOfPayment,
		ReservationNumber,
		UserFullName,
		CustomerFullName,
		CreatedAt
    FROM 
		PaymentDetailsView
	ORDER BY
		CreatedAt DESC
END;
