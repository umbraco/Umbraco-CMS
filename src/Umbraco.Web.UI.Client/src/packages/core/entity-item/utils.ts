import type { UmbItemModel } from './types.js';

/**
 * Returns a fallback name for an item
 * @param {UmbItemModel} item The item to get the fallback name for
 * @returns {string} A fallback name
 */
export function getItemFallbackName(item: UmbItemModel): string {
	return `${item.entityType}:${item.unique}`;
}

/**
 * Returns a fallback icon for an item
 * @returns {string} A fallback icon
 */
export function getItemFallbackIcon(): string {
	return 'icon-circle-dotted';
}
