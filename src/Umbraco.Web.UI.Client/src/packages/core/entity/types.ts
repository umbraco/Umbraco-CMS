export type UmbEntityUnique = string | null;

export interface UmbEntityModel {
	unique: UmbEntityUnique;
	entityType: string;
}

export interface UmbNamedEntityModel extends UmbEntityModel {
	name: string;
}

export interface UmbDefaultItemModel extends UmbNamedEntityModel {
	icon?: string;
}
