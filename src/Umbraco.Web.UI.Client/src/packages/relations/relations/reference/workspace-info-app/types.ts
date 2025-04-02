import type { ManifestWorkspaceInfoApp } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceInfoAppEntityReferencesKind extends ManifestWorkspaceInfoApp {
	type: 'workspaceInfoApp';
	kind: 'entityReferences';
}

export interface MetaWorkspaceInfoAppEntityReferencesKind {
	referenceRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceInfoAppEntityReferencesKind: ManifestWorkspaceInfoAppEntityReferencesKind;
	}
}
