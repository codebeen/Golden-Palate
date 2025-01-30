CREATE PROCEDURE GetAvailableTablesForToday_12PMto2PM
AS
BEGIN
    -- Select all tables that are available today and not reserved between 12 PM and 2 PM
    SELECT *
    FROM Tables AS t
    WHERE t.IsDeleted = 0
      AND t.Status = 'Available'  -- Ensure only available tables
      AND t.Id NOT IN (
          SELECT r.TableId
          FROM Reservations AS r
          WHERE CAST(r.ReservationDate AS DATE) = CAST(GETDATE() AS DATE)  -- Check for today's date
            AND CAST(r.ReservationTime AS TIME) BETWEEN '12:00:00' AND '14:00:00'  -- Corrected time range (12 PM - 2 PM)
            AND r.Status != 'Cancelled'  -- Exclude cancelled reservations
      );
END;
