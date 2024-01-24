import type { UmbBlockTypeBaseModel } from '../block-type/index.js';

export interface UmbBlockGridType extends UmbBlockTypeBaseModel {
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
