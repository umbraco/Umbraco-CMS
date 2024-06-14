import { UmbImagingRepository } from './imaging.repository.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_IMAGING_REPOSITORY_ALIAS = 'Umb.Repository.Imaging';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_IMAGING_REPOSITORY_ALIAS,
	name: 'Imaging Repository',
	api: UmbImagingRepository,
};

export const manifests: Array<ManifestTypes> = [repository];
