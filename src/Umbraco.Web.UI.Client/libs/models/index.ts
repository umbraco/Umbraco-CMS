// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type ClassConstructor<T> = new (...args: any[]) => T;

/** Tried to find a common base of our entities — used by Entity Workspace Context */
export type UmbEntityBase = {
	id?: string;
	name?: string;
};

export interface UmbSwatchDetails {
	label: string;
	value: string;
}
