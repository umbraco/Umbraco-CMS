import { UmbDocumentTrackedReferenceRepository } from './document-tracked-reference.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TRACKED_REFERENCE_REPOSITORY_ALIAS = 'Umb.Repository.Document.TrackedReference';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TRACKED_REFERENCE_REPOSITORY_ALIAS,
	name: 'Document Tracked Reference Repository',
	api: UmbDocumentTrackedReferenceRepository,
};

export const manifests = [repository];
