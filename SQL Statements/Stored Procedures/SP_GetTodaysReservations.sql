CREATE PROCEDURE GetTodaysReservations
AS
BEGIN
    SELECT 
        Id,
		ReservationNumber,
        ReservationDate,
        TotalPrice,
        TableNumber,
        CustomerFullName,
        BuffetType,           
        SpecialRequest,       
        ReservationStatus
    FROM 
        ReservationDetails
    WHERE 
        CAST(ReservationDate AS DATE) = CAST(GETDATE() AS DATE)
		AND ReservationStatus = 'Pending'
END;