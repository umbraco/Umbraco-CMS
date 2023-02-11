# [Choice] .NET version: 6.0, 3.1, 6.0-bullseye, 3.1-bullseye, 6.0-focal, 3.1-focal
ARG VARIANT=6.0-bullseye
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-${VARIANT}

# [Choice] Node.js version: none, lts/*, 18, 16, 14
ARG NODE_VERSION="none"
RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi


# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

# Following added by Warren...
# Sets the global user for npm as 'root' due to some permission errors for gulp-imagemin creating binaries
# https://stackoverflow.com/a/45505787
#
# Needing to set unsafe-perm as root is the user setup
# https://docs.npmjs.com/cli/v6/using-npm/config#unsafe-perm
# Default: false if running as root, true otherwise (we are ROOT)
#RUN npm -g config set user vscode && npm -g config set unsafe-perm

# Generate and trust a local developer certificate for Kestrel
# This is needed for Kestrel to bind on https
RUN dotnet dev-certs https --trust
