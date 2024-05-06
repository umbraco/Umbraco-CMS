export interface UmbMediaCardItemModel {
	name: string;
	unique: string;
	url?: string;
	extension?: string;
	isFolder: boolean;
	isImageRenderable: boolean;
}

export interface UmbMediaPathModel {
	name: string;
	unique: string | null;
}
