export interface UmbPickerModalData<ItemType> {
	multiple?: boolean;
	filter?: (item: ItemType) => boolean;
	pickableFilter?: (item: ItemType) => boolean;
}
export interface UmbPickerModalValue {
	selection: Array<string | null>;
}
