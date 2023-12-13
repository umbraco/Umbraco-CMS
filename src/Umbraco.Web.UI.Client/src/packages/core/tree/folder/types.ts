export interface UmbFolderModel {
	unique: string;
	parentUnique: string | null;
	name: string;
}

export interface UmbCreateFolderModel {
	unique: string;
	parentUnique: string | null;
	name: string;
}

export interface UmbUpdateFolderModel {
	unique: string;
	name: string;
}
