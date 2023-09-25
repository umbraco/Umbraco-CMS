export interface UmbPickerModalData<ItemType> {
	multiple?: boolean;
	selection?: Array<string | null>;
	filter?: (item: ItemType) => boolean;
	pickableFilter?: (item: ItemType) => boolean;
}
export interface UmbPickerModalValue {
	selection: Array<string | null>;
}

export interface UmbTreePickerModalData<TreeItemType> extends UmbPickerModalData<TreeItemType> {
	treeAlias?: string;
}
