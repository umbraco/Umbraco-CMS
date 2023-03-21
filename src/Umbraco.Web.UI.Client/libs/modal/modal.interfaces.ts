export interface UmbPickerModalData<T> {
	multiple: boolean;
	selection: Array<string>;
	filter?: (language: T) => boolean;
}

export interface UmbPickerModalResult<T> {
	selection: Array<string>;
}
