import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbDocumentItemRepository,
	UmbDocumentSearchRepository,
	UmbDocumentTreeRepository,
} from '@umbraco-cms/backoffice/document';

export class UmbDocumentPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	tree = new UmbDocumentTreeRepository(this);
	item = new UmbDocumentItemRepository(this);
	search = new UmbDocumentSearchRepository(this);
}

export { UmbDocumentPickerPropertyEditorDataSource as api };
