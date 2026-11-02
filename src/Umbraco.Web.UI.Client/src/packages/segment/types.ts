export type * from './entity.js';
export type * from './collection/types.js';

export interface UmbSegmentModel {
	/**
	 * The unique alias of the segment.
	 */
	alias: string;

	/**
	 * The name of the segment used for display purposes.
	 */
	name: string;
}

export interface UmbSegmentResponseModel {
	items: Array<UmbSegmentModel>;
	total: number;
}
