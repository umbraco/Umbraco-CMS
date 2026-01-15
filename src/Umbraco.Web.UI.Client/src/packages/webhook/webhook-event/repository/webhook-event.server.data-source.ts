import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Webhook that fetches data from the server
 * @class UmbWebhookEventServerDataSource
 * @implements {RepositoryEventDataSource}
 */
export class UmbWebhookEventServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbWebhookEventServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookEventServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getAll() {
		const { data, error } = await tryExecute(this.#host, WebhookService.getWebhookEvents());

		if (error || !data) {
			return { error };
		}

		const items = data.items.map((item) => {
			return {
				eventName: item.eventName,
				eventType: item.eventType,
				alias: item.alias,
			};
		});

		return {
			data: {
				items,
				total: items.length,
			},
		};
	}
}
