import { UmbMediaTypeRepository } from './media-type.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const MEDIA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.MediaTypes';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEDIA_TYPE_REPOSITORY_ALIAS,
	name: 'Media Types Repository',
	class: UmbMediaTypeRepository,
};

export const manifests = [repository];
