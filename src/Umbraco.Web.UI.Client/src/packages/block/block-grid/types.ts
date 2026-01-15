import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeWithGroupKey } from '@umbraco-cms/backoffice/block-type';

export type * from './clipboard/types.js';

// Configuration models:
export interface UmbBlockGridTypeModel extends UmbBlockTypeWithGroupKey {
	columnSpanOptions: Array<UmbBlockGridTypeColumnSpanOption>;
	allowAtRoot: boolean;
	allowInAreas: boolean;
	rowMinSpan: number;
	rowMaxSpan: number;
	thumbnail?: string;
	areaGridColumns?: number;
	areas: Array<UmbBlockGridTypeAreaType>;
	inlineEditing?: boolean;
}

export type UmbBlockGridTypeColumnSpanOption = {
	columnSpan: number;
};

export interface UmbBlockGridTypeAreaType {
	key: string;
	alias: string;
	columnSpan?: number;
	rowSpan?: number;
	minAllowed?: number;
	maxAllowed?: number;
	specifiedAllowance?: Array<UmbBlockGridTypeAreaTypePermission>;
	createLabel?: string;
}

export interface UmbBlockGridTypeAreaTypePermission {
	elementTypeKey?: string;
	groupKey?: string;
	minAllowed?: number;
	maxAllowed?: number;
}

export interface UmbBlockGridTypeGroupType {
	name: string;
	key: string;
}

// Content models:

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockGridValueModel extends UmbBlockValueType<UmbBlockGridLayoutModel> {}

export interface UmbBlockGridLayoutModel extends UmbBlockLayoutBaseModel {
	columnSpan: number;
	rowSpan: number;
	areas?: Array<UmbBlockGridLayoutAreaItemModel>;
}

export interface UmbBlockGridLayoutAreaItemModel {
	key: string;
	items: Array<UmbBlockGridLayoutModel>;
}
