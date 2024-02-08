import { UmbDocumentPublicAccessRepository } from '../repository/index.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS = 'Umb.Repository.Document.PublicAccess';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS,
	name: 'Document Public Access Repository',
	api: UmbDocumentPublicAccessRepository,
};

export const manifests = [repository];
