import type { UmbBlockTypeBaseModel, UmbBlockTypeWithGroupKey } from '../block-type/index.js';
import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '../index.js';

export const UMB_BLOCK_GRID_TYPE = 'block-grid-type';

// Configuration models:
export interface UmbBlockGridTypeModel extends UmbBlockTypeBaseModel {
	columnSpanOptions: Array<number>;
	allowAtRoot: boolean;
	allowInAreas: boolean;
	rowMinSpan: number;
	rowMaxSpan: number;
	thumbnail?: string;
	areaGridColumns?: number;
	areas: Array<any>;
}

export interface UmbBlockGridTypeGroupType {
	name: string;
	key: string;
}

export interface UmbBlockGridGroupTypeConfiguration extends Partial<UmbBlockGridTypeGroupType> {
	blocks: Array<UmbBlockTypeWithGroupKey>;
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
