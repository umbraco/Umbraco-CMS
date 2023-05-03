export interface UmbPickerModalData<T> {
	multiple: boolean;
	selection: Array<string | null>;
	filter?: (item: T) => boolean;
}

export interface UmbPickerModalResult {
	selection: Array<string | null>;
}
