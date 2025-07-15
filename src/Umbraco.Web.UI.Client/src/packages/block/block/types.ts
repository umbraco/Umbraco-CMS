import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

export type * from './conditions/types.js';
export type * from './clipboard/types.js';

export interface UmbBlockLayoutBaseModel {
	contentKey: string;
	settingsKey?: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockDataValueModel<ValueType = unknown> extends UmbElementValueModel<ValueType> {}

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

export interface UmbBlockExposeModel {
	contentKey: string;
	culture: string | null;
	segment: string | null;
}

export interface UmbBlockValueDataPropertiesBaseType {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	expose: Array<UmbBlockExposeModel>;
}

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel>
	extends UmbBlockValueDataPropertiesBaseType {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
}
