import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import { UmbUserCollectionRepository, UmbUserItemRepository } from '@umbraco-cms/backoffice/user';

export class ExampleUserPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource
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
