import type { UmbApiResponse } from '../types.js';
import { tryExecute } from './tryExecute.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Batches promises and returns a promise that resolves to an array of results
 * @template BatchEntryType
 * @template PromiseResult
 * @param {UmbControllerHost} host - The host to use for the request and where notifications will be shown
 * @param {Array<Array<BatchEntryType>>} chunks - The array of chunks to process
 * @param {(chunk: Array<BatchEntryType>) => Promise<PromiseResult>} callback - The function to call for each chunk
 * @returns {Promise<PromiseSettledResult<UmbApiResponse<PromiseResult>>[]>} - A promise that resolves to an array of results
 * @remarks Each chunk is run through tryExecute, which resolves with an error instead of rejecting. A chunk that
 * failed therefore arrives as a fulfilled result carrying an error, and callers have to read it to notice.
 */
export function batchTryExecute<BatchEntryType, PromiseResult>(
	host: UmbControllerHost,
	chunks: Array<Array<BatchEntryType>>,
	callback: (chunk: Array<BatchEntryType>) => Promise<PromiseResult>,
): Promise<PromiseSettledResult<UmbApiResponse<PromiseResult>>[]> {
	return Promise.allSettled(chunks.map((chunk) => tryExecute(host, callback(chunk), { disableNotifications: true })));
}
