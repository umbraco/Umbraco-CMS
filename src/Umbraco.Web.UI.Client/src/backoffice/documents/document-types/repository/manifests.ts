import { UmbDocumentTypeRepository } from './document-type.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const DOCUMENT_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentTypes';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Document Types Repository',
	class: UmbDocumentTypeRepository,
};

export const manifests = [repository];
