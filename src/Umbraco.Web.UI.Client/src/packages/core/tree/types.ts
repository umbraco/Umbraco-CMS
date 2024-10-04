import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
export type * from './extensions/index.js';

export interface UmbTreeItemModelBase extends UmbEntityModel {
	name: string;
	hasChildren: boolean;
	isFolder: boolean;
	icon?: string | null;
}

export interface UmbTreeItemModel extends UmbTreeItemModelBase {
	unique: string;
	parent: UmbEntityModel;
}

export interface UmbTreeRootModel extends UmbTreeItemModelBase {
	unique: null;
}

export type UmbTreeSelectionConfiguration = {
	multiple?: boolean;
	selectable?: boolean;
	selection?: Array<string | null>;
};

export interface UmbTreeStartNode {
	unique: string;
	entityType: string;
}
