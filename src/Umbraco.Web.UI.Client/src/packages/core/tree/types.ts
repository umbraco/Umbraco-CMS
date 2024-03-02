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

export interface UmbUniqueTreeRootModel extends UmbTreeItemModelBase {
	unique: null;
}

export type UmbTreeSelectionConfiguration = {
	multiple?: boolean;
	selectable?: boolean;
	selection?: Array<string | null>;
};
