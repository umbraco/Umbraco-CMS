import type { ManifestBase } from './models';

export interface ManifestWorkspaceViewCollection extends ManifestBase {
	type: 'workspaceViewCollection';
	meta: MetaEditorViewCollection;
}

export interface MetaEditorViewCollection {
	workspaces: string[];
	pathname: string;
	label: string;
	icon: string;
	entityType: string;
	storeAlias: string;
}
