import { UmbLanguageRepository } from '../repository/language.repository';
import { UmbLanguageStore } from './language.store';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';
import { ManifestStore } from '@umbraco-cms/extensions-registry';

export const LANGUAGE_REPOSITORY_ALIAS = 'Umb.Repository.Language';

const repository: ManifestRepository = {
	type: 'repository',
	alias: LANGUAGE_REPOSITORY_ALIAS,
	name: 'Languages Repository',
	class: UmbLanguageRepository,
};

export const LANGUAGE_STORE_ALIAS = 'Umb.Store.Language';

const store: ManifestStore = {
	type: 'store',
	alias: LANGUAGE_STORE_ALIAS,
	name: 'Language Store',
	class: UmbLanguageStore,
};

export const manifests = [repository, store];
