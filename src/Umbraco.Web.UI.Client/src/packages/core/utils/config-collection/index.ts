import type { UmbConfigCollectionEntryModel } from './types.js';

/**
 * Get a value from a config collection by its alias.
 * @param config - The config collection to get the value from.
 * @param alias - The alias of the config entry to get the value for.
 * @returns The value of the config entry with the specified alias, or undefined if not found.
 */
export function getConfigValue<T extends UmbConfigCollectionEntryModel, K extends T['alias']>(
	config: T[] | undefined,
	alias: K,
) {
	return config?.find((entry) => entry.alias === alias)?.value as Extract<T, { alias: K }>['value'] | undefined;
}
