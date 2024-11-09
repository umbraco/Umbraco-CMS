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

export interface UmbNumberRangeValueType {
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
