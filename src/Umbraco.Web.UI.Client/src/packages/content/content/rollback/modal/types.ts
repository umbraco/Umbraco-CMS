import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export interface ManifestModalRollbackKind extends ManifestModal {
	type: 'modal';
	kind: 'rollback';
	meta: MetaModalRollbackKind;
}

export interface MetaModalRollbackKind {
	rollbackRepositoryAlias: string;
	detailRepositoryAlias: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRollbackModalData {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRollbackModalValue {}

declare global {
	interface UmbExtensionManifestMap {
		umbModalRollbackKind: ManifestModalRollbackKind;
	}
}
