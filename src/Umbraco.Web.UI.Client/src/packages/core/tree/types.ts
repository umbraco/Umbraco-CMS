export interface UmbTreeItemModelBase {
	entityType: string;
	name: string;
	hasChildren: boolean;
	isFolder: boolean;
	icon?: string | null;
}

export interface UmbUniqueTreeItemModel extends UmbTreeItemModelBase {
	unique: string;
	parentUnique: string | null;
}

// TODO: remove this when we have unique for everything
export interface UmbEntityTreeItemModel extends UmbTreeItemModelBase {
	id: string;
	parentId: string | null;
}

// TODO: remove this when we have unique for everything
export interface UmbFileSystemTreeItemModel extends UmbTreeItemModelBase {
	path: string;
	isFolder: boolean;
}

// Root
export interface UmbUniqueTreeRootModel extends UmbTreeItemModelBase {
	unique: null;
}

// TODO: remove this when we have unique for everything
export interface UmbEntityTreeRootModel extends UmbTreeItemModelBase {
	id: null;
}

// TODO: remove this when we have unique for everything
export interface UmbFileSystemTreeRootModel extends UmbTreeItemModelBase {
	path: null;
}
