import type {
	ManifestEntityCreateOptionAction,
	MetaEntityCreateOptionAction,
} from '@umbraco-cms/backoffice/entity-create-option-action';

export interface ManifestEntityCreateOptionActionFolderKind
	extends ManifestEntityCreateOptionAction<MetaEntityCreateOptionActionFolderKind> {
	type: 'entityCreateOptionAction';
	kind: 'folder';
}

export interface MetaEntityCreateOptionActionFolderKind extends MetaEntityCreateOptionAction {
	folderRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityCreateOptionActionFolderKind: ManifestEntityCreateOptionActionFolderKind;
	}
}
