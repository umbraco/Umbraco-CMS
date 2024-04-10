export interface UmbPickerModalData<ItemType> {
	multiple?: boolean;
	hideTreeRoot?: boolean; // TODO: this should be moved to a tree picker modal data interface
	filter?: (item: ItemType) => boolean;
	pickableFilter?: (item: ItemType) => boolean;
}
export interface UmbPickerModalValue {
	selection: Array<string | null>;
}

export interface UmbTreePickerModalData<TreeItemType> extends UmbPickerModalData<TreeItemType> {
	treeAlias?: string;
}

export interface UmbTreePickerModalValue extends UmbPickerModalValue {}
