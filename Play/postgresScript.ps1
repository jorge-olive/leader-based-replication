﻿docker network create clusterNetwork;

docker run -d -m 512m --cpus=0.5    --network clusterNetwork -v primary_data:/var/lib/postgresql/data --name pg_primary -e POSTGRES_PASSWORD=secret -p 5432:5432 docker.io/postgres:14;

docker exec pg_primary sh -c 'echo host replication replicator 0.0.0.0/0 md5 >> $PGDATA/pg_hba.conf';

docker exec pg_primary psql -U postgres -c "CREATE USER replicator WITH REPLICATION ENCRYPTED PASSWORD 'secret'";

docker exec pg_primary psql -U postgres -c 'select pg_reload_conf()';

docker run -it --rm --network clusterNetwork -v replica_data:/var/lib/postgresql/data --name pg_replica_init docker.io/postgres:14 sh -c 'pg_basebackup -h pg_primary -U replicator -p 5432 -D $PGDATA -Fp -Xs -P -R -W';

docker run -m 512m --cpus=0.5 -d --network clusterNetwork -v replica_data:/var/lib/postgresql/data -e POSTGRES_PASSWORD=secret -p 5480:5432 --name pg_replica docker.io/postgres:14;


docker run -it --rm --network clusterNetwork -v replica_data2:/var/lib/postgresql/data --name pg_replica_init docker.io/postgres:14 sh -c 'pg_basebackup -h pg_primary -U replicator -p 5432 -D $PGDATA -Fp -Xs -P -R -W';

docker run -m 512m --cpus=0.5 -d --network clusterNetwork -v replica_data2:/var/lib/postgresql/data -e POSTGRES_PASSWORD=secret -p 5490:5432 --name pg_replica2 docker.io/postgres:14;

docker pull dpage/pgadmin4
docker run -p 5050:80 \
    -e "PGADMIN_DEFAULT_EMAIL=user@domain.com" \
    -e "PGADMIN_DEFAULT_PASSWORD=SuperSecret" \
    -d dpage/pgadmin4