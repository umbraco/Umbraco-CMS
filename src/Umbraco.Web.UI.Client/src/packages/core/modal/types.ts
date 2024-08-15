export interface UmbPickerModalData<ItemType> {
	multiple?: boolean;
	filter?: (item: ItemType) => boolean;
	pickableFilter?: (item: ItemType) => boolean;
	search?: UmbPickerModalSearchData;
}

export interface UmbPickerModalSearchData {
	providerAlias: string;
}

export interface UmbPickerModalValue {
	selection: Array<string | null>;
}
