import { tryExecute } from './tryExecute.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Batches promises and returns a promise that resolves to an array of results
 * @param {UmbControllerHost} host - The host to use for the request and where notifications will be shown
 * @param {Array<Array<BatchEntryType>>} chunks - The array of chunks to process
 * @param {(chunk: Array<BatchEntryType>) => Promise<PromiseResult>} callback - The function to call for each chunk
 * @returns {Promise<PromiseSettledResult<PromiseResult>[]>} - A promise that resolves to an array of results
 */
export function batchTryExecute<BatchEntryType, PromiseResult>(
	host: UmbControllerHost,
	chunks: Array<Array<BatchEntryType>>,
	callback: (chunk: Array<BatchEntryType>) => Promise<PromiseResult>,
): Promise<PromiseSettledResult<PromiseResult>[]> {
	return Promise.allSettled(chunks.map((chunk) => tryExecute(host, callback(chunk), { disableNotifications: true })));
}
