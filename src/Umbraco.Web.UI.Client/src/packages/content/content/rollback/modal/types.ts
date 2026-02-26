import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export interface ManifestModalContentRollbackKind extends ManifestModal {
	type: 'modal';
	kind: 'contentRollback';
	meta: MetaModalContentRollbackKind;
}

export interface MetaModalContentRollbackKind {
	rollbackRepositoryAlias: string;
	detailRepositoryAlias: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentRollbackModalData {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentRollbackModalValue {}

declare global {
	interface UmbExtensionManifestMap {
		umbModalContentRollbackKind: ManifestModalContentRollbackKind;
	}
}
