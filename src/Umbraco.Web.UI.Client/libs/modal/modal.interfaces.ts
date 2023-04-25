export interface UmbPickerModalData<T> {
	multiple: boolean;
	selection: Array<string>;
	filter?: (item: T) => boolean;
}

export interface UmbPickerModalResult {
	selection: Array<string>;
}
