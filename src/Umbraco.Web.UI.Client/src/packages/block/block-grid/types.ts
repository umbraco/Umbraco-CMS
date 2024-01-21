import type { UmbBlockTypeWithGroupKey } from '../block-type/index.js';

export interface UmbBlockGridType extends UmbBlockTypeWithGroupKey {
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
