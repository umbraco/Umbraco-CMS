import { UmbDocumentValidationRepository } from './document-validation.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_VALIDATION_REPOSITORY_ALIAS = 'Umb.Repository.Document.Validation';

const validationRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_VALIDATION_REPOSITORY_ALIAS,
	name: 'Document Validation Repository',
	api: UmbDocumentValidationRepository,
};

export const manifests = [validationRepository];
