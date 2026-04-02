/**
 * Utility functions for managing property editor UI state updates
 * when values are set programmatically.
 */

/**
 * Interface for items that can be selected/checked
 */
export interface UmbSelectableItem {
	value: string;
	selected?: boolean;
	checked?: boolean;
}

/**
 * Updates the selected state of items based on current selection.
 * This function is for internal use only within the property-editors package and should not be exposed
 * to external consumers to avoid unwanted external dependencies.
 * @internal
 * @template T
 * @param {T[]} items - Array of items to update
 * @param {string[]} selection - Array of selected values
 * @param {'selected' | 'checked'} stateProperty - Property name to update ('selected' or 'checked')
 * @returns {T[]} New array with updated state, or original array if no changes needed
 */
export function updateItemsSelectedState<T extends UmbSelectableItem>(
	items: T[],
	selection: string[],
	stateProperty: 'selected' | 'checked' = 'selected',
): T[] {
	// Convert to Set for O(1) lookups instead of O(n) includes
	const selectionSet = new Set(selection);

	// Check if any state changes are needed to avoid unnecessary array allocations
	let hasChanges = false;
	for (const item of items) {
		const shouldBeSelected = selectionSet.has(item.value);
		const currentState = item[stateProperty] ?? false;
		if (currentState !== shouldBeSelected) {
			hasChanges = true;
			break;
		}
	}

	// Return original array if no changes needed
	if (!hasChanges) {
		return items;
	}

	// Only create new array if changes are needed
	return items.map((item) => ({
		...item,
		[stateProperty]: selectionSet.has(item.value),
	}));
}

/**
 * Helper function to ensure a value is an array
 * @param {string | string[] | null | undefined} value - Value to convert to array
 * @returns {string[]} Array representation of the value
 */
export function ensureArray(value: string | string[] | null | undefined): string[] {
	return Array.isArray(value) ? value : value ? [value] : [];
}
