import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import { UmbWebhookCollectionRepository, UmbWebhookItemRepository } from '@umbraco-cms/backoffice/webhook';

export class ExampleWebhookPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource
{
	#collection = new UmbWebhookCollectionRepository(this);
	#item = new UmbWebhookItemRepository(this);

	requestCollection(args: UmbCollectionFilterModel) {
		return this.#collection.requestCollection(args);
	}

	requestItems(uniques: Array<string>) {
		return this.#item.requestItems(uniques);
	}
}

export { ExampleWebhookPickerPropertyEditorDataSource as api };
