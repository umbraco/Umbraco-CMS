#!/bin/bash
set -echo
echo "WARREN WARREN WARREN WARREN Building Umbraco"

echo "Changing to Umbraco.Web.UI.Client folder"
cd ../src/Umbraco.Web.UI.Client

echo "npm install of Client files"
npm install --verbose

echo "npm run build to copy assets to site"
npm run build --verbose

echo "Changing to src folder"
cd ..

echo "dotnet build SLN"
dotnet build umbraco-netcore-only.sln


echo "DONE DONE DONE DONE DONE DONE"