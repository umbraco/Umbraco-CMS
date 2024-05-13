export type UmbEntityUnique = string | null;

export interface UmbEntityModel {
	unique: UmbEntityUnique;
	entityType: string;
}
