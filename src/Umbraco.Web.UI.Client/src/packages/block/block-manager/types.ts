export interface UmbBlockLayoutBaseModel {
	contentUdi: string;
	settingsUdi?: string;
}

export interface UmbBlockDataType {
	udi: string;
	contentTypeKey: string;
}

export interface UmbBlockValueType {
	layout: { [key: string]: Array<UmbBlockLayoutBaseModel> };
	contentData: Array<UmbBlockDataType>;
	settingsData: Array<UmbBlockDataType>;
}
