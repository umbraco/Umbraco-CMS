export * from './context-menu/index.js';
export * from './entity-tree-item/index.js';
export * from './tree-item/index.js';
export * from './tree-item-base/index.js';
export * from './tree-menu-item/index.js';
export * from './tree.context.js';
export * from './tree.element.js';

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
