export interface UmbPickerModalData<TreeItemType> {
	multiple?: boolean;
	hideTreeRoot?: boolean;
	filter?: (item: TreeItemType) => boolean;
	pickableFilter?: (item: TreeItemType) => boolean;
}
export interface UmbPickerModalValue {
	selection: Array<string | null>;
}

export interface UmbTreePickerModalData<TreeItemType> extends UmbPickerModalData<TreeItemType> {
	treeAlias?: string;
}
