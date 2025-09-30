import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbMediaItemRepository, UmbMediaTreeRepository } from '@umbraco-cms/backoffice/media';

export class UmbDocumentPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	tree = new UmbMediaTreeRepository(this);
	item = new UmbMediaItemRepository(this);
}

export { UmbDocumentPickerPropertyEditorDataSource as api };
