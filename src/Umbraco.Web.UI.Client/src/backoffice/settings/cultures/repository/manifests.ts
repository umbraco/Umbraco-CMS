import { UmbCultureRepository } from '../repository/culture.repository';
import { ManifestRepository } from '@umbraco-cms/backoffice/extensions-registry';

export const CULTURE_REPOSITORY_ALIAS = 'Umb.Repository.Culture';

const repository: ManifestRepository = {
	type: 'repository',
	alias: CULTURE_REPOSITORY_ALIAS,
	name: 'Cultures Repository',
	class: UmbCultureRepository,
};

export const manifests = [repository];
