import type { UmbMediaTreeItemModel } from './types.js';

/**
 * Type guard to check if an item is a media tree item with noAccess property
 * @param {unknown} item - The item to check
 * @returns {boolean} true if the item is a UmbMediaTreeItemModel
 */
export function isMediaTreeItem(item: unknown): item is UmbMediaTreeItemModel {
	return typeof item === 'object' && item !== null && 'noAccess' in item;
}
