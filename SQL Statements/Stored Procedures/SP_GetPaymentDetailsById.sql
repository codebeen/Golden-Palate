CREATE PROCEDURE GetPaymentDetailsById
	@Id INT
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
	WHERE
		Id = @Id
END;
