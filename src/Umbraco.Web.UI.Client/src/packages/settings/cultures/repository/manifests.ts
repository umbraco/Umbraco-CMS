import { UmbCultureRepository } from '../repository/culture.repository.js';
import { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const CULTURE_REPOSITORY_ALIAS = 'Umb.Repository.Culture';

const repository: ManifestRepository = {
	type: 'repository',
	alias: CULTURE_REPOSITORY_ALIAS,
	name: 'Cultures Repository',
	class: UmbCultureRepository,
};

export const manifests = [repository];
