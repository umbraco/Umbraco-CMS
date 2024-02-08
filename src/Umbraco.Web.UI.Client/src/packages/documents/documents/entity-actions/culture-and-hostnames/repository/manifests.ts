import { UmbDocumentCultureAndHostnamesRepository } from './culture-and-hostnames.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_CULTURE_AND_HOSTNAMES_REPOSITORY_ALIAS = 'Umb.Repository.Document.CultureAndHostnames';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_CULTURE_AND_HOSTNAMES_REPOSITORY_ALIAS,
	name: 'Document Culture And Hostnames Repository',
	api: UmbDocumentCultureAndHostnamesRepository,
};

export const manifests = [repository];
