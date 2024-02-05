import type { UmbBlockTypeBaseModel, UmbBlockTypeWithGroupKey } from '../block-type/index.js';
import type { UmbBlockLayoutBaseModel } from '../index.js';

export const UMB_BLOCK_GRID_TYPE = 'block-grid-type';

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

export interface UmbBlockGridGroupType {
	name: string;
	key: string;
}

export interface UmbBlockGridGroupTypeConfiguration extends Partial<UmbBlockGridGroupType> {
	blocks: Array<UmbBlockTypeWithGroupKey>;
}

export interface UmbBlockGridLayoutModel extends UmbBlockLayoutBaseModel {
	columnSpan: number;
	rowSpan: number;
	areas: Array<UmbBlockGridLayoutAreaItemModel>;
}

export interface UmbBlockGridLayoutAreaItemModel {
	key: string;
	items: Array<UmbBlockGridLayoutModel>;
}
