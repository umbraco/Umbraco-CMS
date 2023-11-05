import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { DirectionModel, LogLevelModel, SavedLogSearchPresenationBaseModel } from '@umbraco-cms/backoffice/backend-api';

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbWebhookRepository {
	#host: UmbControllerHostElement;
	#notificationService?: UmbNotificationContext;
	#init;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#init = new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
		}).asPromise();
	}

	async requestWebhooks({ skip, take } = { skip: 0, take: 1000 }) {
		await this.#init;
		
	}
}
