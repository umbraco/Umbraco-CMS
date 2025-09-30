import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLanguageCollectionRepository, UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';

export class UmbLanguagePickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	collection = new UmbLanguageCollectionRepository(this);
	item = new UmbLanguageItemRepository(this);
}

export { UmbLanguagePickerPropertyEditorDataSource as api };
