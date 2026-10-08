/**
 * Fetches the Management API OpenAPI document from a running Umbraco backend and writes it
 * byte-for-byte to src/Umbraco.Cms.Api.Management/OpenApi.json.
 *
 * The backend must already be running — this script does not start one. Pass --wait to keep
 * retrying while nothing is listening yet, for when the backend was only just started.
 * @example node devops/openapi/index.js
 * @example node devops/openapi/index.js --wait
 * @example node devops/openapi/index.js https://localhost:5001/umbraco/openapi/management.json
 * @author Umbraco HQ
 */

import { get } from 'node:https';
import { writeFileSync } from 'node:fs';
import { join } from 'node:path';

const DEFAULT_URL = 'https://localhost:44339/umbraco/openapi/management.json';
const TARGET = join(import.meta.dirname, '../../../Umbraco.Cms.Api.Management/OpenApi.json');
const REQUEST_TIMEOUT_MS = 30_000;
const WAIT_TIMEOUT_MS = 300_000;
const RETRY_DELAY_MS = 3_000;
const ERROR_COLOR = '\x1b[31m%s\x1b[0m';
const SUCCESS_COLOR = '\x1b[32m%s\x1b[0m';

const args = process.argv.slice(2);
const shouldWait = args.includes('--wait');
const url = args.find((arg) => !arg.startsWith('--')) ?? DEFAULT_URL;

// The backend serves this over HTTPS with the local dev certificate.
function fetchDocument() {
	return new Promise((resolve, reject) => {
		const request = get(url, { rejectUnauthorized: false }, (response) => {
			if (response.statusCode !== 200) {
				response.resume();
				reject(new Error(`Responded with HTTP ${response.statusCode}`));
				return;
			}

			let body = '';
			response.setEncoding('utf8');
			response.on('data', (chunk) => (body += chunk));
			response.on('end', () => resolve(body));
			response.on('error', reject);
		});

		request.on('error', reject);
		request.setTimeout(REQUEST_TIMEOUT_MS, () => request.destroy(new Error('Timed out')));
	});
}

const isConnectionError = (error) => error.code === 'ECONNREFUSED' || error.code === 'ECONNRESET';
const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

console.log(`Fetching the OpenAPI document from ${url}`);

const deadline = Date.now() + (shouldWait ? WAIT_TIMEOUT_MS : 0);
let announcedWait = false;
let document;

for (; ;) {
	try {
		document = await fetchDocument();
		break;
	} catch (error) {
		if (!isConnectionError(error) || Date.now() >= deadline) {
			console.error(ERROR_COLOR, `Could not fetch ${url}: ${error.message}`);
			console.error(
				isConnectionError(error)
					? 'Nothing is listening on that address. Start a backend first:\n  dotnet run --project src/Umbraco.Web.UI --no-launch-profile -- --environment Development --urls https://localhost:44339'
					: 'Something is listening on that address but is not serving the Management API OpenAPI document.\OpenAPI is only mapped when the environment is not Production, so a backend started without\n--environment Development will return 404 here.',
			);
			process.exit(1);
		}

		if (!announcedWait) {
			console.log('Nothing listening yet — waiting for the backend to come up...');
			announcedWait = true;
		}

		await delay(RETRY_DELAY_MS);
	}
}

try {
	JSON.parse(document);
} catch (error) {
	console.error(ERROR_COLOR, `The response from ${url} is not valid JSON: ${error.message}`);
	console.error('OpenApi.json has been left untouched.');
	process.exit(1);
}

writeFileSync(TARGET, document);

console.log(SUCCESS_COLOR, `Wrote ${TARGET}`);
