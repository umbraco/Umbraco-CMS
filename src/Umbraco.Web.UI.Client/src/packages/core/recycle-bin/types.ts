export interface UmbRecycleBinRestoreRequestArgs {
	unique: string;
	destination: {
		unique: string | null;
	};
}

export interface UmbRecycleBinTrashRequestArgs {
	unique: string;
}

export interface UmbRecycleBinOriginalParentRequestArgs {
	unique: string;
}
