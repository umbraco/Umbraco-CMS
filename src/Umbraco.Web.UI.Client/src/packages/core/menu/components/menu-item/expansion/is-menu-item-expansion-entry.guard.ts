import type { UmbMenuItemExpansionEntryModel } from '../../types.js';

/**
 * Checks if the provided object is a Menu Item Expansion Entry.
 * @param {object } object - The object to check.
 * @returns {boolean } True if the object is a Menu Item Expansion Entry, false otherwise.
 */
export function isMenuItemExpansionEntry(object: unknown): object is UmbMenuItemExpansionEntryModel {
	return (
		typeof object === 'object' &&
		object !== null &&
		'entityType' in object &&
		'unique' in object &&
		'menuItemAlias' in object
	);
}
