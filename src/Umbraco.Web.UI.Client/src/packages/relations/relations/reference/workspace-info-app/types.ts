import type { ManifestWorkspaceInfoApp, MetaWorkspaceInfoApp } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceInfoAppEntityReferencesKind
	extends ManifestWorkspaceInfoApp<MetaWorkspaceInfoAppEntityReferencesKind> {
	type: 'workspaceInfoApp';
	kind: 'entityReferences';
}

export interface MetaWorkspaceInfoAppEntityReferencesKind extends MetaWorkspaceInfoApp {
	referenceRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceInfoAppEntityReferencesKind: ManifestWorkspaceInfoAppEntityReferencesKind;
	}
}
