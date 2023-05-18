export type * from './data-type-model';

/** Tried to find a common base of our entities — used by Entity Workspace Context */
export type UmbEntityBase = {
	id?: string;
	name?: string;
};

export interface UmbSwatchDetails {
	label: string;
	value: string;
}
export interface ServertimeOffset {
	/**
	 * offset in minutes relative to UTC
	 */
	offset: number;
}
