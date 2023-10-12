RESTORE DATABASE TwitterClone 
FROM DISK = '/data/TwitterClone.bak' 
WITH MOVE 'TwitterClone' TO '/var/opt/mssql/data/TwitterClone.mdf', 
MOVE 'TwitterClone_log' TO '/var/opt/mssql/data/TwitterClone.ldf';
