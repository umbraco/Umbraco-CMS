FROM mcr.microsoft.com/azure-sql-edge:latest

ENV ACCEPT_EULA=Y

USER root

RUN mkdir /var/opt/sqlserver

RUN chown mssql /var/opt/sqlserver

ENV MSSQL_BACKUP_DIR="/var/opt/mssql"
ENV MSSQL_DATA_DIR="/var/opt/mssql/data"
ENV MSSQL_LOG_DIR="/var/opt/mssql/log"

EXPOSE 1433/tcp
COPY setup.sql /
COPY startup.sh /
COPY healthcheck.sh /

ENTRYPOINT [ "/bin/bash", "startup.sh" ]
CMD [ "/opt/mssql/bin/sqlservr" ]
