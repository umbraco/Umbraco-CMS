export interface UmbBlockLayoutBaseModel {
	contentUdi: string;
	settingsUdi?: string | null;
}

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

export interface UmbBlockViewUrlsPropType {
	editContent?: string;
	editSettings?: string;
}

export interface UmbBlockViewPropsType<BlockLayoutType extends UmbBlockLayoutBaseModel> {
	label?: string;
	icon?: string;
	contentUdi: string;
	layout?: BlockLayoutType;
	content?: UmbBlockDataType;
	settings?: UmbBlockDataType;
	urls: UmbBlockViewUrlsPropType;
}
