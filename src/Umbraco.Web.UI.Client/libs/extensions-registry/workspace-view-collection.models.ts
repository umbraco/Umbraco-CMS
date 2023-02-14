import type { ManifestBase } from './models';

export interface ManifestWorkspaceViewCollection extends ManifestBase {
	type: 'workspaceViewCollection';
	meta: MetaEditorViewCollection;
}

// TODO: Get rid of store alias, when we are done migrating to repositories(remember to enforce repositoryAlias):
export interface MetaEditorViewCollection {
	workspaces: string[];
	pathname: string;
	label: string;
	icon: string;
	entityType: string;
	storeAlias?: string;
	repositoryAlias?: string;
}
