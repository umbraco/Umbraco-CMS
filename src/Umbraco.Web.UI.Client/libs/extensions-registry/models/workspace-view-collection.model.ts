import type { ManifestWithConditions, ManifestWithView, MetaManifestWithView } from '.';

export interface ManifestWorkspaceViewCollection
	extends ManifestWithView,
		ManifestWithConditions<ConditionsEditorViewCollection> {
	type: 'workspaceViewCollection';
	meta: MetaEditorViewCollection;
}
export interface MetaEditorViewCollection extends MetaManifestWithView {
	entityType: string;
	repositoryAlias: string;
}

export interface ConditionsEditorViewCollection {
	workspaces: string[];
}
