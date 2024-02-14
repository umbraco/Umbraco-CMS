export interface UmbTreeItemModelBase {
	name: string;
	entityType: string;
	hasChildren: boolean;
	isFolder: boolean;
	icon?: string | null;
}

export interface UmbUniqueTreeItemModel extends UmbTreeItemModelBase {
	unique: string;
	parentUnique: string | null;
}

// Root
export interface UmbUniqueTreeRootModel extends UmbTreeItemModelBase {
	unique: null;
}

// ------------------------------------

// TODO: remove this when we have unique for everything
export interface UmbEntityTreeItemModel extends UmbTreeItemModelBase {
	id: string;
	parentId: string | null;
}

// TODO: remove this when we have unique for everything
export interface UmbEntityTreeRootModel extends UmbTreeItemModelBase {
	id: null;
}
