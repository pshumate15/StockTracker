SELECT COUNT(sr.StockReferenceID) AS count, s.Symbol, s.Name, s.StockID FROM StockReferences sr
INNER JOIN Stocks s 
    ON sr.StockID = s.StockID
INNER JOIN Comments c
    ON sr.CommentID = c.CommentID
WHERE DATE(c.SavedOn) > DATE('NOW', '-7 day')
GROUP BY sr.StockID, s.Name, s.Symbol, s.StockID
ORDER BY count DESC;

