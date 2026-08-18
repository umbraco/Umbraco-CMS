import type { UmbTreeItemModel, UmbTreePickerModalData, UmbTreePickerModalValue } from '@umbraco-cms/backoffice/tree';

export interface UmbContentPickerModalCollectionConfig {
	/**
	 * The alias of the collection to render at a node whose content type has a collection configured.
	 */
	alias: string;
}

export interface UmbContentPickerModalData<TreeItemType = UmbTreeItemModel> extends Omit<
	UmbTreePickerModalData<TreeItemType>,
	'createAction'
> {
	collection: UmbContentPickerModalCollectionConfig;

	/**
	 * Decides which items can be picked.
	 *
	 * Items are presented as tree items at a level that renders the tree, and as collection items at a level that
	 * renders a collection, so a filter may only rely on what both carry:
	 *
	 * | Field | Available |
	 * |---|---|
	 * | `unique`, `entityType` | both levels |
	 * | `contentType.unique`, `contentType.collection` | both levels |
	 * | `hasChildren` | tree only; a collection item reports `false` until the server supplies it |
	 * | `name`, `isFolder`, `variants` | tree only |
	 *
	 * A `unique` of `null` means the root, which never occurs at a collection level.
	 */
	pickableFilter?: (item: TreeItemType) => boolean;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentPickerModalValue extends UmbTreePickerModalValue {}
