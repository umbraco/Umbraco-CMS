import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbLanguageCollectionRepository, UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';

export class ExampleLanguagePickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource
{
	#collection = new UmbLanguageCollectionRepository(this);
	#item = new UmbLanguageItemRepository(this);

	requestCollection(args: UmbCollectionFilterModel) {
		return this.#collection.requestCollection(args);
	}

	requestItems(uniques: Array<string>) {
		return this.#item.requestItems(uniques);
	}
}

export { ExampleLanguagePickerPropertyEditorDataSource as api };
