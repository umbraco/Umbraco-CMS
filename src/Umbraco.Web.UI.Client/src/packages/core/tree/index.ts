export * from './context-menu';
export * from './entity-tree-item';
export * from './tree-item';
export * from './tree-item-base';
export * from './tree-menu-item';
export * from './tree.context';
export * from './tree.element';

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
