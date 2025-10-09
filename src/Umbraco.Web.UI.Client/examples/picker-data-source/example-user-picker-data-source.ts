import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbPickerPropertyEditorCollectionDataSource } from '@umbraco-cms/backoffice/picker-property-editor';
import { UmbUserCollectionRepository, UmbUserItemRepository } from '@umbraco-cms/backoffice/user';

export class ExampleUserPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	#collection = new UmbUserCollectionRepository(this);
	#item = new UmbUserItemRepository(this);

	requestCollection(args: UmbCollectionFilterModel) {
		return this.#collection.requestCollection(args);
	}

	requestItems(uniques: Array<string>) {
		return this.#item.requestItems(uniques);
	}
}

export { ExampleUserPickerPropertyEditorDataSource as api };
