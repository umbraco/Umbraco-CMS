import { UmbDocumentTypeCompositionRepository } from './document-type-composition.repository.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Composition';

const queryRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS,
	name: 'Document Type Composition Repository',
	api: UmbDocumentTypeCompositionRepository,
};

export const manifests: Array<ManifestTypes> = [queryRepository];
