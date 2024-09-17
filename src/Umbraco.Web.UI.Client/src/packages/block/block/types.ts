import type { UmbContentValueModel } from '@umbraco-cms/backoffice/content';

export interface UmbBlockLayoutBaseModel {
	contentKey: string;
	settingsKey?: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockDataValueModel<ValueType = unknown> extends UmbContentValueModel<ValueType> {}

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

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel> {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
}
