import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbMoveSelectableFilterProvider } from './move-selectable-filter-provider.interface.js';

export type { UmbMoveDataSource } from './move-data-source.interface.js';
export type { UmbMoveRepository } from './move-repository.interface.js';
export type { UmbMoveSelectableFilterProvider } from './move-selectable-filter-provider.interface.js';
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
	selectableFilterProviderAlias?: string;
}

export interface ManifestMoveSelectableFilterProvider extends ManifestApi<UmbMoveSelectableFilterProvider> {
	type: 'moveSelectableFilterProvider';
}

declare global {
	interface UmbExtensionManifestMap {
		umbMoveToEntityActionKind: ManifestEntityActionMoveToKind;
		umbMoveSelectableFilterProvider: ManifestMoveSelectableFilterProvider;
	}
}
