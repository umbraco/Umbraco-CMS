import { UmbTemplateRepository } from '../repository/template.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const TEMPLATE_REPOSITORY_ALIAS = 'Umb.Repository.Templates';

const repository: ManifestRepository = {
	type: 'repository',
	alias: TEMPLATE_REPOSITORY_ALIAS,
	name: 'Documents Repository',
	class: UmbTemplateRepository,
};

export const manifests = [repository];
