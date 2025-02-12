CREATE PROCEDURE GetReservationDetailsFromView
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
        ReservationDetails;
END;
