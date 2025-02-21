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
		PaymentDetails
	ORDER BY
		CreatedAt DESC
END;
