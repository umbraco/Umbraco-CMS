import type { UmbPickerPropertyEditorCollectionDataSource } from '../../src/packages/core/property-editor/property-editor-data-source/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbLanguageCollectionRepository, UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';

export class ExampleLanguagePickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
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
