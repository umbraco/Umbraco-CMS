// @ts-check

const config = {
    endpoint: "https://xyz.documents.azure.com:443/",
    key: "abcd",
    databaseId: "UmbracoLoadTesting",
    containerId: "LoadTestResults",
    partitionKey: { kind: "Hash", paths: ["/testingSource"] }
  };

  module.exports = config;
