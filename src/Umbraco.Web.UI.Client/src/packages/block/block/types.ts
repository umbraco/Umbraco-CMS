// Shared with the Property Editor
export interface UmbBlockLayoutBaseModel {
	contentUdi: string;
	settingsUdi?: string | null;
}

// Shared with the Property Editor
export interface UmbBlockDataType {
	udi: string;
	contentTypeKey: string;
	[key: string]: unknown;
}

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel> {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
	contentData: Array<UmbBlockDataType>;
	settingsData: Array<UmbBlockDataType>;
}
