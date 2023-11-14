export interface UmbTreeItemModel {
	type: string;
	name: string;
	hasChildren: boolean;
	icon?: string;
}

export interface UmbEntityTreeItemModel extends UmbTreeItemModel {
	id: string;
}

export interface UmbFileSystemTreeItemModel {
	path: string;
}
