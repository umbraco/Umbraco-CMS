const CosmosClient = require("@azure/cosmos").CosmosClient;
const dbContext = require("./databaseContext");
const csvReports = require("./csvReports");
const throughputReports = require("./throughputReports");

async function pushReports(container, umbVersion, machineSpec) {
    await pushPerformanceReports(container, umbVersion, machineSpec);
    await pushThroughputReports(container, umbVersion, machineSpec);
}

async function pushThroughputReports(container, umbVersion, machineSpec) {
    const reports = throughputReports.createReports();

    for (let i = 0; i < reports.length; i++) {
        const report = reports[i];

        const newItem = {
            id: report.fileId,
            testingSource: machineSpec,
            umbVersion: umbVersion,
            file: report.file,
            fileDate: report.fileDate,
            testType: "throughput",
            report: report.report
        };

        const { resource: createdItem } = await container.items.create(newItem);

        console.log(`Pushed report: ${createdItem.id} - ${createdItem.file}`);
    }
}

async function pushPerformanceReports(container, umbVersion, machineSpec) {

    const reports = await csvReports.createReports();

    for (let i = 0; i < reports.length; i++) {
        const report = reports[i];

        const newItem = {
            id: report.fileId,
            testingSource: machineSpec,
            umbVersion: umbVersion,
            file: report.file,
            fileDate: report.fileDate,
            testType: "perf",
            report: report.report
        };

        const { resource: createdItem } = await container.items.create(newItem);

        console.log(`Pushed report: ${createdItem.id} - ${createdItem.file}`);
    }
}

async function showReport(container, machineSpec) {

    const queries = [
        `SELECT
            c.umbVersion,
            c.file,
            AVG(c.report['# bytes in all heaps']) BytesInAllHeaps,
            AVG(c.report['large object heap size']) LOH,
            AVG(c.report['gen 2 heap size']) Gen2,
            AVG(c.report['gen 1 heap size']) Gen1,
            AVG(c.report['gen 0 heap size']) Gen0,
            AVG(c.report['working set']) WorkingSet
        FROM c
        WHERE c.testingSource = '${machineSpec}' AND c.testType = 'perf'
        GROUP BY c.umbVersion, c.file`,

        `SELECT
            c.umbVersion,
            c.file,
            AVG(c.report.rps.mean) RPS,
            AVG(c.report.latency.p99) P99,
            AVG(c.report.latency.p95) P95
        FROM c
        WHERE c.testingSource = '${machineSpec}' AND c.testType = 'throughput'
        GROUP BY c.umbVersion, c.file`
    ];

    for (let qry = 0; qry < queries.length; qry++) {
        const query = queries[qry];

        // query to return all items
        const querySpec = {
            query: query
        };

        // read all items in the LoadTestResults container
        const { resources: items } = await container.items
            .query(querySpec)
            .fetchAll();

        items.forEach(item => {
            console.log(item);
        });
    }
}

async function main() {
    const myArgs = process.argv.slice(2);
    if (myArgs.length < 1) {
        throw "Missing argument for the Umbraco version";
    }
    if (myArgs.length < 2) {
        throw "Missing argument for the test runner machine spec";
    }
    if (myArgs.length < 3) {
        throw "Missing argument for cosmosdb endpoint";
    }
    if (myArgs.length < 4) {
        throw "Missing argument for cosmosdb key";
    }
    if (myArgs.length < 5) {
        throw "Missing argument for cosmosdb databaseId";
    }
    if (myArgs.length < 6) {
        throw "Missing argument for cosmosdb containerId";
    }

    const umbVersion = myArgs[0];
    const machineSpec = myArgs[1];
    const endpoint = myArgs[2];
    const key = myArgs[3];
    const databaseId = myArgs[4];
    const containerId = myArgs[5];

    const client = new CosmosClient({ endpoint, key });

    const database = client.database(databaseId);
    const container = database.container(containerId);

    // Make sure Tasks database is already setup. If not, create it.
    const partitionKey = { kind: "Hash", paths: ["/testingSource"] };
    await dbContext.create(client, databaseId, containerId, partitionKey);

    await pushReports(container, umbVersion, machineSpec);
    await showReport(container, machineSpec);
}

main();
