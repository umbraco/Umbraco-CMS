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

	/**
	 * An optional list of culture codes that the segment applies to.
	 * If null, the segment applies to the invariant culture.
	 * If undefined, the segment is considered generic and applies to all cultures.
	 */
	cultures?: Array<string> | null;
}

export interface UmbSegmentResponseModel {
	items: Array<UmbSegmentModel>;
	total: number;
}
