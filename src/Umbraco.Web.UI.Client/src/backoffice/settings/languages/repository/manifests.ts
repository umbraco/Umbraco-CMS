import { UmbLanguageRepository } from '../repository/language.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const LANGUAGE_REPOSITORY_ALIAS = 'Umb.Repository.Languages';

const repository: ManifestRepository = {
	type: 'repository',
	alias: LANGUAGE_REPOSITORY_ALIAS,
	name: 'Languages Repository',
	class: UmbLanguageRepository,
};

export const manifests = [repository];
