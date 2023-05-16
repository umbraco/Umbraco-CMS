/** Tried to find a common base of our entities — used by Entity Workspace Context */
export type UmbEntityBase = {
	id?: string;
	name?: string;
};

export interface UmbSwatchDetails {
	label: string;
	value: string;
}
