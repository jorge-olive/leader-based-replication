﻿$timer = new-timespan -Minutes 5
$clock = [diagnostics.stopwatch]::StartNew()
while ($clock.elapsed -lt $timer){
$command=docker exec pg_replica psql -U postgres -c "SELECT  'pg_replica' as cluster, pg_is_in_recovery() AS is_slave,  pg_last_wal_receive_lsn() AS receive,  pg_last_wal_replay_lsn() AS replay,  pg_last_wal_receive_lsn() = pg_last_wal_replay_lsn() AS ynced,  (EXTRACT(EPOCH FROM now()) -   EXTRACT(EPOCH FROM pg_last_xact_replay_timestamp()))::int AS lag;"
cls
$command
start-sleep -seconds 5
}
write-host "Timer end"




$timer = new-timespan -Minutes 5
$clock = [diagnostics.stopwatch]::StartNew()
while ($clock.elapsed -lt $timer){
$command=docker exec pg_replica2 psql -U postgres -c "SELECT  'pg_replica2' as cluster, pg_is_in_recovery() AS is_slave,  pg_last_wal_receive_lsn() AS receive,  pg_last_wal_replay_lsn() AS replay,  pg_last_wal_receive_lsn() = pg_last_wal_replay_lsn() AS ynced,  (EXTRACT(EPOCH FROM now()) -   EXTRACT(EPOCH FROM pg_last_xact_replay_timestamp()))::int AS lag;"
cls
$command
start-sleep -seconds 5
}
write-host "Timer end"
