import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDocumentItemRepository, UmbDocumentTreeRepository } from '@umbraco-cms/backoffice/document';
import { UmbDocumentSearchRepository } from 'src/packages/documents/documents/search/document-search.repository.js';

export class UmbDocumentPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	tree = new UmbDocumentTreeRepository(this);
	item = new UmbDocumentItemRepository(this);
}

export { UmbDocumentPickerPropertyEditorDataSource as api };
