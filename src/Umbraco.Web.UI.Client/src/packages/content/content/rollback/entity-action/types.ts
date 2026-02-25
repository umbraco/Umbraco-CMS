import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionRollbackKind extends ManifestEntityAction<MetaEntityActionRollbackKind> {
	type: 'entityAction';
	kind: 'rollback';
}

export interface MetaEntityActionRollbackKind extends MetaEntityActionDefaultKind {
	rollbackModalAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionRollbackKind: ManifestEntityActionRollbackKind;
	}
}
