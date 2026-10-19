import type { UmbConfigCollectionEntryModel } from './types.js';

/**
 * Get a value from a config collection by its alias.
 * @template {UmbConfigCollectionEntryModel} T
 * @template {T['alias']} K
 * @param {T[] | undefined} config - The config collection to get the value from.
 * @param {K} alias - The alias of the config entry to get the value for.
 * @returns {Extract<T, { alias: K }>['value'] | undefined} The value of the config entry with the specified alias, or undefined if not found.
 */
export function getConfigValue<T extends UmbConfigCollectionEntryModel, K extends T['alias']>(
	config: T[] | undefined,
	alias: K,
) {
	return config?.find((entry) => entry.alias === alias)?.value as Extract<T, { alias: K }>['value'] | undefined;
}
