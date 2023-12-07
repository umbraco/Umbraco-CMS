export interface UmbTreeItemModelBase {
	type: string;
	name: string;
	hasChildren: boolean;
	icon: string | null;
}

export interface UmbTreeItemModel extends UmbTreeItemModelBase {
	parentId: string | null;
}

export interface UmbUniqueTreeItemModel extends UmbTreeItemModelBase {
	unique: string | null;
}

// TODO: remove this when we have unique for everything
export interface UmbEntityTreeItemModel extends UmbTreeItemModel {
	id: string;
}

// TODO: remove this when we have unique for everything
export interface UmbFileSystemTreeItemModel extends UmbTreeItemModel {
	path: string;
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
