import { UmbStylesheetCollectionRepository } from './stylesheet-collection.repository.js';
import { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Collection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STYLESHEET_COLLECTION_REPOSITORY_ALIAS,
	name: 'Stylesheet Collection Repository',
	api: UmbStylesheetCollectionRepository,
};

export const manifests = [repository];
