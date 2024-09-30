import type { UmbWebhookItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { WebhookItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => WebhookService.getItemWebhook({ id: uniques });

const mapper = (item: WebhookItemResponseModel): UmbWebhookItemModel => {
	return {
		unique: item.name,
		name: item.name,
	};
};
