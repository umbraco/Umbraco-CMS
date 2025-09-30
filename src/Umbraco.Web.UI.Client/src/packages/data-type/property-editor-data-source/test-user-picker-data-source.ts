import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbUserCollectionRepository, UmbUserItemRepository } from '@umbraco-cms/backoffice/user';

export class UmbUserPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	collection = new UmbUserCollectionRepository(this);
	item = new UmbUserItemRepository(this);
}

export { UmbUserPickerPropertyEditorDataSource as api };
