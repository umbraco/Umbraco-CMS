export type UmbEntityUnique = string | null;

/** Tried to find a common base of our entities — used by Entity Workspace Context */
export type UmbEntityBase = {
	id?: string;
	name?: string;
};

export interface UmbVariantableValueModel<T = unknown> extends UmbInvariantValueModel<T> {
	culture?: string | null;
	segment?: string | null;
}
export interface UmbVariantValueModel<T = unknown> extends UmbInvariantValueModel<T> {
	culture: string | null;
	segment: string | null;
}

export interface UmbInvariantValueModel<T = unknown> {
	alias: string;
	value: T;
}

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

export interface NumberRangeValueType {
	min?: number;
	max?: number;
}

export interface UmbReferenceByUnique {
	unique: string;
}

export interface UmbReferenceByUniqueAndType {
	type: string;
	unique: string;
}

export interface UmbUniqueItemModel {
	unique: string;
	name: string;
	icon?: string;
}

export enum UmbDirectionModel {
	ASCENDING = 'Ascending',
	DESCENDING = 'Descending',
}
