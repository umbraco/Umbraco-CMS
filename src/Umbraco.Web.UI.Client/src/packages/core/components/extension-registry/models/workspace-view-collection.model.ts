import type { ManifestWithConditions, ManifestWithView, MetaManifestWithView } from 'src/libs/extension-api';

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
