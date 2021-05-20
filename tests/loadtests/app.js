const yargs = require('yargs/yargs')
const { hideBin } = require('yargs/helpers')
const CosmosClient = require("@azure/cosmos").CosmosClient;
const dbContext = require("./databaseContext");
const csvReports = require("./csvReports");
const throughputReports = require("./throughputReports");

async function pushReports(container, umbVersion, specName, perfReportType) {
    await pushPerformanceReports(container, umbVersion, specName, perfReportType);
    await pushThroughputReports(container, umbVersion, specName);
}

async function pushThroughputReports(container, umbVersion, specName) {
    const reports = throughputReports.createReports();

    for (let i = 0; i < reports.length; i++) {
        const report = reports[i];

        const newItem = {
            id: report.fileId,
            testingSource: specName,
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

async function pushPerformanceReports(container, umbVersion, specName, perfReportType) {

    const reports = await csvReports.createReports(perfReportType);

    for (let i = 0; i < reports.length; i++) {
        const report = reports[i];

        const newItem = {
            id: report.fileId,
            testingSource: specName,
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

async function showReport(container, specName) {

    const queries = [
        `SELECT
            c.umbVersion,
            c.file,
            AVG(c.report['# bytes in all heaps']) BytesInAllHeaps,
            AVG(c.report['large object heap size']) LOH,
            AVG(c.report['gen 2 heap size']) Gen2,
            AVG(c.report['gen 1 heap size']) Gen1,
            AVG(c.report['gen 0 heap size']) Gen0,
            AVG(c.report['working set']) WorkingSet,
            AVG(c.report['# total committed bytes']) TotalCommittedBytes,
            AVG(c.report['% processor time']) ProcessorTime
        FROM c
        WHERE c.testingSource = '${specName}' AND c.testType = 'perf'
        GROUP BY c.umbVersion, c.file`,

        `SELECT
            c.umbVersion,
            c.file,
            AVG(c.report.rps.mean) RPS,
            AVG(c.report.latency.p99) P99,
            AVG(c.report.latency.p95) P95
        FROM c
        WHERE c.testingSource = '${specName}' AND c.testType = 'throughput'
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

    const argv = yargs(hideBin(process.argv)).argv

    if (argv.cosmosEndpoint && argv.cosmosKey && argv.cosmosDatabaseId && argv.cosmosContainerId && argv.specName) {

        const client = new CosmosClient({
            "endpoint": argv.cosmosEndpoint,
            "key": argv.cosmosKey
        });
        const database = client.database(argv.cosmosDatabaseId);
        const container = database.container(argv.cosmosContainerId);

        if (argv.sendReport) {
            if (argv.umbVersion && argv.perfReportType) {
                // Make sure Tasks database is already setup. If not, create it.
                const partitionKey = { kind: "Hash", paths: ["/testingSource"] };
                await dbContext.create(client, argv.cosmosDatabaseId, argv.cosmosContainerId, partitionKey);

                await pushReports(container, argv.umbVersion, argv.specName, argv.perfReportType);
            }
            else {
                throw "Missing required reporting arguments";
            }
        }

        if (argv.showReport) {
            await showReport(container, argv.specName);
        }

    }
    else {
        throw "Missing required cosmos arguments";
    }
}

main();
