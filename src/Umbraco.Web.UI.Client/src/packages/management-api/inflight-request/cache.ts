import type { UmbApiResponse } from '@umbraco-cms/backoffice/resources';

// Keep internal
type RequestResolvedType<ResponseModelType> = UmbApiResponse<{ data?: ResponseModelType }>;

/**
 * A cache for inflight requests to the Management Api. Use this class to cache requests and avoid duplicate calls.
 * @class UmbManagementApiInflightRequestCache
 * @template ResponseModelType
 */
export class UmbManagementApiInflightRequestCache<ResponseModelType> {
	#entries = new Map<string, Promise<RequestResolvedType<ResponseModelType>>>();

	/**
	 * Checks if an entry exists in the cache
	 * @param {string} key - The ID of the entry to check
	 * @returns {boolean} - True if the entry exists, false otherwise
	 * @memberof UmbManagementApiInflightRequestCache
	 */
	has(key: string): boolean {
		return this.#entries.has(key);
	}

	/**
	 * Adds an entry to the cache
	 * @param {string} key - A unique key representing the promise
	 * @param {Promise<UmbApiResponse<RequestResolvedType<ResponseModelType>>>} promise - The promise to cache
	 * @memberof UmbManagementApiInflightRequestCache
	 */
	set(key: string, promise: Promise<RequestResolvedType<ResponseModelType>>): void {
		this.#entries.set(key, promise);
	}

	/**
	 * Retrieves an entry from the cache
	 * @param {string} key - The ID of the entry to retrieve
	 * @returns {Promise<RequestResolvedType<ResponseModelType>> | undefined} - The cached promise or undefined if not found
	 * @memberof UmbManagementApiInflightRequestCache
	 */
	get(key: string): Promise<RequestResolvedType<ResponseModelType>> | undefined {
		return this.#entries.get(key);
	}

	/**
	 * Deletes an entry from the cache
	 * @param {string} key - The ID of the entry to delete
	 * @memberof UmbManagementApiInflightRequestCache
	 */
	delete(key: string): void {
		this.#entries.delete(key);
	}

	/**
	 * Clears all entries from the cache
	 * @memberof UmbManagementApiInflightRequestCache
	 */
	clear(): void {
		this.#entries.clear();
	}
}
