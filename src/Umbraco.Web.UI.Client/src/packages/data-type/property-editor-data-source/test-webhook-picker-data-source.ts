import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbWebhookCollectionRepository, UmbWebhookItemRepository } from '@umbraco-cms/backoffice/webhook';

export class UmbWebhookPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	#collection = new UmbWebhookCollectionRepository(this);
	#item = new UmbWebhookItemRepository(this);
	#config: any;

	setConfig(config: any): void {
		this.#config = config;
	}

	getConfig(): any {
		return this.#config;
	}

	requestCollection(args: UmbCollectionFilterModel) {
		return this.#collection.requestCollection(args);
	}

	requestItems(uniques: Array<string>) {
		return this.#item.requestItems(uniques);
	}
}

export { UmbWebhookPickerPropertyEditorDataSource as api };
