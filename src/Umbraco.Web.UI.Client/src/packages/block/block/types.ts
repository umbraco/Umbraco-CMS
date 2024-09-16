export interface UmbBlockLayoutBaseModel {
	contentKey: string;
	settingsKey?: string | null;
}

export interface UmbBlockDataValueModel<ValueType = unknown> {
	culture: string | null;
	segment: string | null;
	alias: string;
	value: ValueType;
}
export interface UmbBlockDataModel {
	key: string;
	contentTypeKey: string;
	values: Array<UmbBlockDataValueModel>;
}

export interface UmbBlockDataType {
	//udi: string; // I want to try to leave these out for now [NL]
	//contentTypeKey: string; // I want to try to leave these out for now [NL]
	[key: string]: unknown;
}

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel> {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
}
