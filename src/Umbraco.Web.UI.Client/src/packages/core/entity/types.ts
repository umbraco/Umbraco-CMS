import type { UmbEntityUnique } from './entity-unique/types.js';

export interface UmbEntityModel {
	unique: UmbEntityUnique;
	entityType: string;
}

export interface UmbNamedEntityModel extends UmbEntityModel {
	name: string;
}

// TODO: Should this be in its own package?
export interface UmbEntityWithFlags extends UmbEntityModel {
	flags: Array<UmbEntityFlag>;
}
export interface UmbEntityWithOptionalFlags extends UmbEntityModel {
	flags?: UmbEntityWithFlags['flags'];
}

export interface UmbEntityFlag {
	alias: string;
}

export type * from './entity-type/types.js';
export type * from './entity-unique/types.js';
