import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionContentRollbackKind extends ManifestEntityAction<MetaEntityActionContentRollbackKind> {
	type: 'entityAction';
	kind: 'contentRollback';
}

export interface MetaEntityActionContentRollbackKind extends MetaEntityActionDefaultKind {
	rollbackNotificationMessage?: string;
	rollbackRepositoryAlias: string;
	detailRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionContentRollbackKind: ManifestEntityActionContentRollbackKind;
	}
}
