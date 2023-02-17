import { UmbDictionaryRepository } from '../repository/dictionary.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const DICTIONARY_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DICTIONARY_REPOSITORY_ALIAS,
	name: 'Dictionary Repository',
	class: UmbDictionaryRepository,
};

export const manifests = [repository];
