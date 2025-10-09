import type { UmbConfigCollectionModel } from './types.js';

/**
 * Get a value from a config collection by its alias.
 * @param {UmbConfigCollectionModel | undefined} config - The config collection to get the value from.
 * @param {string} alias - The alias of the value to get.
 * @returns {T | undefined} The value with the specified alias, or undefined if not found or if the config is undefined.
 */
export function getConfigValue<T>(config: UmbConfigCollectionModel | undefined, alias: string): T | undefined {
	const entry = config?.find((entry) => entry.alias === alias);
	return entry?.value as T | undefined;
}
