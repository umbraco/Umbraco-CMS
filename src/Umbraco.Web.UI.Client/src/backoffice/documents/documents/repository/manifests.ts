import { UmbDocumentRepository } from '../repository/document.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const DOCUMENT_REPOSITORY_ALIAS = 'Umb.Repository.Documents';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_REPOSITORY_ALIAS,
	name: 'Documents Repository',
	class: UmbDocumentRepository,
};

export const manifests = [repository];
