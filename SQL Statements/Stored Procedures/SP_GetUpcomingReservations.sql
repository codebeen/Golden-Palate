CREATE PROCEDURE GetUpcomingReservations
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
        ReservationDate > GETDATE() AND ReservationStatus = 'Pending'
	ORDER BY 
        CASE 
            WHEN ReservationStatus = 'Pending' THEN 1
            ELSE 2
        END,
        ReservationDate ASC;
END;