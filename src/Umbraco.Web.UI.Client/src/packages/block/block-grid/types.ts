import type { UmbBlockTypeBase } from '../block-type/index.js';

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

export interface BlockGridGroupConfigration {
	name?: string;
	key: string;
}
