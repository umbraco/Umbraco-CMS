import { UmbCultureRepository } from '../repository/culture.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const CULTURE_REPOSITORY_ALIAS = 'Umb.Repository.Culture';

const repository: ManifestRepository = {
	type: 'repository',
	alias: CULTURE_REPOSITORY_ALIAS,
	name: 'Cultures Repository',
	class: UmbCultureRepository,
};

export const manifests = [repository];
