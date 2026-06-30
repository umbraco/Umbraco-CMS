import type {
	ManifestEntityAction,
	MetaEntityActionDefaultKind,
	UmbDuplicateRequestArgs,
} from '@umbraco-cms/backoffice/entity-action';

export type * from './duplicate-to-data-source.interface.js';
export type * from './duplicate-to-repository.interface.js';

export interface UmbDuplicateToRequestArgs extends UmbDuplicateRequestArgs {
	destination: {
		unique: string | null;
	};
}

export interface ManifestEntityActionDuplicateToKind extends ManifestEntityAction<MetaEntityActionDuplicateToKind> {
	type: 'entityAction';
	kind: 'duplicateTo';
}

export interface MetaEntityActionDuplicateToKind extends MetaEntityActionDefaultKind {
	duplicateRepositoryAlias: string;
	treeRepositoryAlias: string;
	treeAlias: string;
	foldersOnly?: boolean;
	/**
	 * The alias of a search provider used to enable search in the destination picker.
	 */
	searchProviderAlias?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDuplicateToEntityActionKind: ManifestEntityActionDuplicateToKind;
	}
}
