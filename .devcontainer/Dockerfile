# [Choice] .NET Core version: 5.0, 3.1, 2.1
ARG VARIANT=3.1
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:0-${VARIANT}

# [Option] Install Node.js
ARG INSTALL_NODE="true"
ARG NODE_VERSION="lts/*"
RUN if [ "${INSTALL_NODE}" = "true" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# [Option] Install Azure CLI
ARG INSTALL_AZURE_CLI="false"
COPY library-scripts/azcli-debian.sh /tmp/library-scripts/
RUN if [ "$INSTALL_AZURE_CLI" = "true" ]; then bash /tmp/library-scripts/azcli-debian.sh; fi \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/* /tmp/library-scripts

# Install SQL Tools: SQLPackage and sqlcmd
COPY mssql/installSQLtools.sh installSQLtools.sh
RUN bash ./installSQLtools.sh \
     && apt-get clean -y && rm -rf /var/lib/apt/lists/* /tmp/library-scripts

# Update args in docker-compose.yaml to set the UID/GID of the "vscode" user.
ARG USER_UID=1000
ARG USER_GID=$USER_UID
RUN if [ "$USER_GID" != "1000" ] || [ "$USER_UID" != "1000" ]; then groupmod --gid $USER_GID vscode && usermod --uid $USER_UID --gid $USER_GID vscode; fi


# [Optional] Uncomment this section to install additional OS packages.
# Following added by Warren...
# Needed to add as Gifsicle used by gulp-imagemin does not ship a Linux binary and has to be compiled from source
# And this Linux package is needed in order to build it
# https://github.com/imagemin/imagemin-gifsicle/issues/40#issuecomment-616487214
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends dh-autoreconf chromium-browser

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

# Following added by Warren...
# Sets the global user for npm as 'root' due to some permission errors for gulp-imagemin creating binaries
# https://stackoverflow.com/a/45505787
#
# Needing to set unsafe-perm as root is the user setup
# https://docs.npmjs.com/cli/v6/using-npm/config#unsafe-perm
# Default: false if running as root, true otherwise (we are ROOT)
RUN npm -g config set user vscode && npm -g config set unsafe-perm