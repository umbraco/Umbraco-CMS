import type { UmbWebhookItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { WebhookItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

/**
 * A server data source for Webhook items
 * @class UmbWebhookItemServerDataSource
 * @implements {UmbItemServerDataSourceBase}
 */
export class UmbWebhookItemServerDataSource extends UmbItemServerDataSourceBase<
	WebhookItemResponseModel,
	UmbWebhookItemModel
> {
	/**
	 * Creates an instance of UmbWebhookItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => WebhookService.getItemWebhook({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: WebhookItemResponseModel): UmbWebhookItemModel => {
	return {
		unique: item.name,
		name: item.name,
	};
};
