import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_CREATE_BLUEPRINT_REPOSITORY_ALIAS = 'Umb.Repository.Document.CreateBlueprint';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_CREATE_BLUEPRINT_REPOSITORY_ALIAS,
	name: 'Document Create Blueprint Repository',
	api: () => import('./document-create-blueprint.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
