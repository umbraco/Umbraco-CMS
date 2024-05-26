export interface UmbBlockLayoutBaseModel {
	contentUdi: string;
	settingsUdi?: string;
}

export interface UmbBlockDataType {
	udi: string;
	contentTypeKey: string;
	[key: string]: unknown;
}

export interface UmbBlockDataBaseValueType {
	contentData: Array<UmbBlockDataType>;
	settingsData: Array<UmbBlockDataType>;
}

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel> extends UmbBlockDataBaseValueType {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
}

export interface UmbBlockViewUrlsPropType {
	editContent?: string;
	editSettings?: string;
}

export interface UmbBlockViewPropsType<BlockLayoutType extends UmbBlockLayoutBaseModel> {
	label?: string;
	contentUdi: string;
	layout?: BlockLayoutType;
	content?: UmbBlockDataType;
	settings?: UmbBlockDataType;
	urls: UmbBlockViewUrlsPropType;
}
