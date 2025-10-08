import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityWithOptionalFlags } from '@umbraco-cms/backoffice/entity-flag';

export type * from './entity-actions/types.js';
export type * from './extensions/types.js';
export type * from './folder/types.js';
export type * from './tree-menu-item/types.js';

export interface UmbTreeItemModelBase extends UmbEntityWithOptionalFlags {
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
