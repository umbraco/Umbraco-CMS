import { UmbDocumentRecycleBinRepository } from './document-recycle-bin.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS = 'Umb.Repository.DocumentRecycleBin';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	name: 'Document Recycle Bin Repository',
	class: UmbDocumentRecycleBinRepository,
};

export const manifests = [repository];
