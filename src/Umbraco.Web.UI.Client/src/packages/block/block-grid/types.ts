import type { UmbBlockTypeBase, UmbBlockTypeWithGroupKey } from '../block-type/index.js';

export interface UmbBlockGridType extends UmbBlockTypeBase {
	columnSpanOptions: Array<number>;
	allowAtRoot: boolean;
	allowInAreas: boolean;
	rowMinSpan: number;
	rowMaxSpan: number;
	thumbnail?: string;
	areaGridColumns?: number;
	areas: Array<any>;
	groupKey: null | string;
}

export interface UmbBlockGridGroupType {
	name: string;
	key: string;
}

export interface UmbBlockGridGroupTypeConfiguration extends Partial<UmbBlockGridGroupType> {
	blocks: Array<UmbBlockTypeWithGroupKey>;
}
