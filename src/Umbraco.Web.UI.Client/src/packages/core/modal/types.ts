export * from './extensions/index.js';

export interface UmbPickerModalData<ItemType> {
	multiple?: boolean;
	filter?: (item: ItemType) => boolean;
	pickableFilter?: (item: ItemType) => boolean;
	search?: UmbPickerModalSearchConfig;
}

export interface UmbPickerModalSearchConfig {
	providerAlias: string;
	queryParams?: object;
}

export interface UmbPickerModalValue {
	selection: Array<string | null>;
}
