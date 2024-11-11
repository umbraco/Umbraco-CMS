import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface UmbMoveToRequestArgs {
	unique: string;
	destination: {
		unique: string | null;
	};
}

export interface ManifestEntityActionMoveToKind extends ManifestEntityAction<MetaEntityActionMoveToKind> {
	type: 'entityAction';
	kind: 'moveTo';
}

export interface MetaEntityActionMoveToKind extends MetaEntityActionDefaultKind {
	moveRepositoryAlias: string;
	treeRepositoryAlias: string;
	treeAlias: string;
	foldersOnly?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbMoveToEntityActionKind: ManifestEntityActionMoveToKind;
	}
}
