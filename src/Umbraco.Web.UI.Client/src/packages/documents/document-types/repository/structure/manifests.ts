import { UmbDocumentTypeStructureRepository } from './document-type-structure.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Structure';

const structureRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS,
	name: 'Document Type Structure Repository',
	api: UmbDocumentTypeStructureRepository,
};

export const manifests = [structureRepository];
