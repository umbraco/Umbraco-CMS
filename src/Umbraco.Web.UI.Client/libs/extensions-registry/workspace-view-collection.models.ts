import type { ManifestBase } from './models';

export interface ManifestWorkspaceViewCollection extends ManifestBase {
	type: 'workspaceViewCollection';
	meta: MetaEditorViewCollection;
	conditions: ConditionsEditorViewCollection;
}

// TODO: Get rid of store alias, when we are done migrating to repositories(remember to enforce repositoryAlias):
export interface MetaEditorViewCollection {
	pathname: string;
	label: string;
	icon: string;
	entityType: string;
	storeAlias?: string;
	repositoryAlias?: string;
}

export interface ConditionsEditorViewCollection {
	workspaces: string[];
}
