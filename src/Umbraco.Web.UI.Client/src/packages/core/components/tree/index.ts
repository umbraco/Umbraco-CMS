export * from './tree.context';
export * from './tree-item-base/tree-item-base.context';
export * from './tree-item/tree-item.context.interface';

export interface UmbTreeRootModel {
	type: string;
	name: string;
	hasChildren: boolean;
	icon?: string;
}

export interface UmbTreeRootEntityModel extends UmbTreeRootModel {
	id: string | null;
}

export interface UmbTreeRootFileSystemModel extends UmbTreeRootModel {
	path: string | null;
}
