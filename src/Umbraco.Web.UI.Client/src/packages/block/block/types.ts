import type { UmbEntryValueModel } from '@umbraco-cms/backoffice/content';

export type * from './conditions/types.js';
export type * from './clipboard/types.js';

export interface UmbBlockLayoutBaseModel {
	key: string;
	contentKey: string;
	settingsKey?: string | null;
	isExternalContent?: boolean;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockDataValueModel<ValueType = unknown> extends UmbEntryValueModel<ValueType> {}

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

/**
 * Why the external (library element) content of a Block is unavailable: it no longer exists, or the current user may not access it.
 */
export type UmbBlockExternalContentFailureReason = 'deleted' | 'noAccess';

export interface UmbBlockExposeModel {
	contentKey: string;
	culture: string | null;
}

export interface UmbBlockValueDataPropertiesBaseType {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	expose: Array<UmbBlockExposeModel>;
}

export interface UmbBlockValueType<
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbBlockValueDataPropertiesBaseType {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
}

export type UmbBlockLabelUfmValueType = Record<string, unknown> & {
	$settings?: Record<string, unknown>;
	$index?: number;
};
