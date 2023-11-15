export interface UmbFolderModel {
	name: string;
	unique: string;
	parentUnique: string | null;
}

export interface UmbCreateFolderModel {
	unique?: string;
	parentUnique: string | null;
	name: string;
}

export interface UmbUpdateFolderModel {
	unique: string;
	name: string;
}
