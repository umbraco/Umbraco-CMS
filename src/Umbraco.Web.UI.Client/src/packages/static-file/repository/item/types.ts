/* TODO: This interface has a parenUnique. It looks more like a Tree Item than an Item? Investigate how this is used,
rename and extend The correct base model (tree or item) */
export interface UmbStaticFileItemModel {
	entityType: string;
	isFolder: boolean;
	name: string;
	parentUnique: string | null;
	unique: string;
}
