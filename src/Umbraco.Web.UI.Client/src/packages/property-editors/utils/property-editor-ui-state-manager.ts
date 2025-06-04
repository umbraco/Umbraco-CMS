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
 * Updates the selected state of items based on current selection
 * @param items - Array of items to update
 * @param selection - Array of selected values
 * @param stateProperty - Property name to update ('selected' or 'checked')
 * @returns New array with updated state
 */
export function updateItemsState<T extends UmbSelectableItem>(
	items: T[],
	selection: string[],
	stateProperty: 'selected' | 'checked' = 'selected'
): T[] {
	return items.map(item => ({
		...item,
		[stateProperty]: selection.includes(item.value)
	}));
}

/**
 * Mixin for property editor elements that need to update UI state
 * when values are set programmatically
 */
export function UmbPropertyEditorUIStateMixin<T extends new (...args: any[]) => any>(Base: T) {
	return class extends Base {
		/**
		 * Updates the UI state and triggers a re-render
		 * @param updateFn - Function that updates the internal state
		 */
		protected updateUIState(updateFn: () => void): void {
			updateFn();
			this.requestUpdate();
		}
	};
}

/**
 * Helper function to ensure a value is an array
 * @param value - Value to convert to array
 * @returns Array representation of the value
 */
export function ensureArray(value: string | string[] | null | undefined): string[] {
	return Array.isArray(value) ? value : value ? [value] : [];
} 