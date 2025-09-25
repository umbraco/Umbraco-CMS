import type { UmbEntityUnique } from './entity-unique/types.js';

export interface UmbEntityModel {
	unique: UmbEntityUnique;
	entityType: string;
}

export interface UmbNamedEntityModel extends UmbEntityModel {
	// TODO: ⚠️[v17]⚠️ Review this, as I had to make `name` nullable to TS compile! Which works against what a `UmbNamedEntityModel` should be! [LK]
	name?: string;
}

export type * from './entity-type/types.js';
export type * from './entity-unique/types.js';
