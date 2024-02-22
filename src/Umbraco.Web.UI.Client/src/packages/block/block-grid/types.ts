import type { UmbBlockTypeWithGroupKey } from '../block-type/index.js';
import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '../index.js';

export const UMB_BLOCK_GRID_TYPE = 'block-grid-type';

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

export interface UmbBlockGridValueModel extends UmbBlockValueType<UmbBlockGridLayoutModel> {}

export interface UmbBlockGridLayoutModel extends UmbBlockLayoutBaseModel {
	columnSpan: number;
	rowSpan: number;
	areas: Array<UmbBlockGridLayoutAreaItemModel>;
}

export interface UmbBlockGridLayoutAreaItemModel {
	key: string;
	items: Array<UmbBlockGridLayoutModel>;
}
