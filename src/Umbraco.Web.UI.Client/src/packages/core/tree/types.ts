export interface UmbTreeItemModelBase {
	name: string;
	entityType: string;
	hasChildren: boolean;
	isFolder: boolean;
	icon?: string | null;
}

export interface UmbTreeItemModel extends UmbTreeItemModelBase {
	unique: string;
	parent: {
		unique: string | null;
		entityType: string;
	};
}

export interface UmbTreeRootModel extends UmbTreeItemModelBase {
	unique: null;
}

export type UmbTreeSelectionConfiguration = {
	multiple?: boolean;
	selectable?: boolean;
	selection?: Array<string | null>;
};
