export interface UmbTreeItemModel {
	type?: string; // TODO: remove option to be undefined when we have our own types
	name?: string;
	hasChildren?: boolean;
	icon?: string | null;
	parentId?: string | null;
}

export interface UmbEntityTreeItemModel extends UmbTreeItemModel {
	id?: string; // TODO: remove option to be undefined when server returns the same
}

export interface UmbFileSystemTreeItemModel {
	path?: string; // TODO: remove option to be undefined when server returns the same
}
