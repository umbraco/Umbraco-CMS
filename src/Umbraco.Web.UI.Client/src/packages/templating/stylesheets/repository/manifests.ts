import { STYLESHEET_REPOSITORY_ALIAS } from '../config.js';
import { UmbStylesheetRepository } from './stylesheet.repository.js';
import { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: STYLESHEET_REPOSITORY_ALIAS,
	name: 'Stylesheet Repository',
	api: UmbStylesheetRepository,
};

export const manifests = [repository];
