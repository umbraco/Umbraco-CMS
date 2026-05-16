import { UmbWebhookEventCollectionRepository } from '../collection/repository/webhook-event-collection.repository.js';
import type { UmbWebhookEventCollectionFilterModel, UmbWebhookEventCollectionItemModel } from '../collection/types.js';
import { UmbWebhookEventItemRepository } from '../item/repository/webhook-event-item.repository.js';
import type {
	UmbPickerCollectionDataSource,
	UmbPickerCollectionDataSourceTextFilterFeature,
} from '@umbraco-cms/backoffice/picker-data-source';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbWebhookEventPickerDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<UmbWebhookEventCollectionItemModel>
{
	#collectionRepository = new UmbWebhookEventCollectionRepository(this);
	#itemRepository = new UmbWebhookEventItemRepository(this);

	#supportsTextFilter = new UmbObjectState<UmbPickerCollectionDataSourceTextFilterFeature>({ enabled: false });

	public readonly features = {
		supportsTextFilter: this.#supportsTextFilter.asObservable(),
	};

	async requestCollection(args: UmbWebhookEventCollectionFilterModel) {
		return this.#collectionRepository.requestCollection(args);
	}

	async requestItems(uniques: Array<string>) {
		return this.#itemRepository.requestItems(uniques);
	}
}

export { UmbWebhookEventPickerDataSource as api };
