CREATE PROCEDURE GetReservationDetailsFromView
AS
BEGIN
    SELECT 
		Id,
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
