import { UmbMediaRepository } from './media.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const DOCUMENT_REPOSITORY_ALIAS = 'Umb.Repository.Media';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_REPOSITORY_ALIAS,
	name: 'Media Repository',
	class: UmbMediaRepository,
};

export const manifests = [repository];
