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
	 * An optional list of culture codes that the segment is limited to.
	 * If null or undefined, the segment is generic and applies to all cultures, as well as to content that does not vary by culture.
	 * A segment limited to cultures does not apply to content that does not vary by culture.
	 * @deprecated Deprecated since v19. This property is temporary, as on the server. Scheduled for removal in Umbraco 20.
	 */
	cultures?: Array<string> | null;
}

export interface UmbSegmentResponseModel {
	items: Array<UmbSegmentModel>;
	total: number;
}
