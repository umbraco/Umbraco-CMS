export interface UmbTreeItemModelBase {
	type?: string; // TODO: remove option to be undefined when we have our own types
	name?: string;
	hasChildren?: boolean;
	icon?: string | null;
}

export interface UmbTreeItemModel extends UmbTreeItemModelBase {
	parentId?: string | null; // TODO: remove option to be undefined when server returns the same or when we get our own types
}

export interface UmbEntityTreeItemModel extends UmbTreeItemModel {
	id?: string; // TODO: remove option to be undefined when server returns the same or when we get our own types
}

export interface UmbFileSystemTreeItemModel extends UmbTreeItemModel {
	path?: string; // TODO: remove option to be undefined when server returns the same or when we get our own types
}

// Root
export interface UmbEntityTreeRootModel extends UmbTreeItemModelBase {
	id: null;
}

export interface UmbFileSystemTreeRootModel extends UmbTreeItemModelBase {
	path: null;
}
