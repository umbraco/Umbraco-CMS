import { UmbDocumentPermissionRepository } from './document-permission.repository.js';
import { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_PERMISSION_REPOSITORY_ALIAS = 'Umb.Repository.Document.Permission';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_PERMISSION_REPOSITORY_ALIAS,
	name: 'Document Permission Repository',
	class: UmbDocumentPermissionRepository,
};

export const manifests = [repository];
