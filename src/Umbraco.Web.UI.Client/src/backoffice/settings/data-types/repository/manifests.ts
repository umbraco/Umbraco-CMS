import { UmbDataTypeRepository } from './data-type.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const DATA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DataTypes';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Data Types Repository',
	class: UmbDataTypeRepository,
};

export const manifests = [repository];
