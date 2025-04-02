import type { ManifestEntityAction, MetaEntityActionDeleteKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestWorkspaceInfoAppEntityReferencesKind
	extends ManifestEntityAction<MetaWorkspaceInfoAppEntityReferencesKind> {
	type: 'entityAction';
	kind: 'entityReferences';
}

export interface MetaWorkspaceInfoAppEntityReferencesKind extends MetaEntityActionDeleteKind {
	referenceRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceInfoAppEntityReferencesKind: ManifestWorkspaceInfoAppEntityReferencesKind;
	}
}
