import type { UmbDocumentTreeItemModel } from './types.js';

/**
 * Type guard to check if an item is a document tree item with noAccess property
 * @param {unknown} item - The item to check
 * @returns {boolean} true if the item is a UmbDocumentTreeItemModel
 */
export function isDocumentTreeItem(item: unknown): item is UmbDocumentTreeItemModel {
	return typeof item === 'object' && item !== null && 'noAccess' in item;
}
