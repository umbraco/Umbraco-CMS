export interface UmbMediaCardItemModel {
	name: string;
	unique: string;
	url?: string;
	extension?: string;
	isImage: boolean;
}

export interface UmbMediaPathModel {
	name: string;
	unique: string | null;
}
