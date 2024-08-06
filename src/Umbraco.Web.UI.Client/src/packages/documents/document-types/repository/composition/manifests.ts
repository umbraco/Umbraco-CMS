import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Composition';

const compositionRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS,
	name: 'Document Type Composition Repository',
	api: () => import('./document-type-composition.repository.js'),
};

export const manifests: Array<ManifestTypes> = [compositionRepository];
