const config = require("./config");
const CosmosClient = require("@azure/cosmos").CosmosClient;

/*
// This script ensures that the database is setup and populated correctly
*/
async function create(client, databaseId, containerId) {
    const partitionKey = config.partitionKey;

    /**
     * Create the database if it does not exist
     */
    const { database } = await client.databases.createIfNotExists({
        id: databaseId
    });
    console.log(`Ensured database: ${database.id}`);

    /**
     * Create the container if it does not exist
     */
    const { container } = await client
        .database(databaseId)
        .containers.createIfNotExists(
            { id: containerId, partitionKey }
            // { offerThroughput: 400 }
        );

    console.log(`Ensured container: ${container.id}`);
}

module.exports = { create };
