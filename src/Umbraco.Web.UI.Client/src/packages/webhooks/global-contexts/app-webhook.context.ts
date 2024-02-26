import { UmbWebhookCollectionRepository } from '../collection/index.js';
import type { UmbWebhookDetailModel } from '../types.js';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbAppWebhookContext extends UmbBaseController implements UmbApi {
	#webhookCollectionRepository: UmbWebhookCollectionRepository;
	#webhooks: Array<UmbWebhookDetailModel> = [];
	#appWebhook = new UmbObjectState<UmbWebhookDetailModel | undefined>(undefined);
	appWebhook = this.#appWebhook.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_APP_WEBHOOK_CONTEXT, this);
		this.#webhookCollectionRepository = new UmbWebhookCollectionRepository(this);
		this.#observeWebhooks();
	}

	setLanguage(unique: string) {
		const webhook = this.#webhooks.find((x) => x.unique === unique);
		this.#appWebhook.update(webhook);
	}

	async #observeWebhooks() {
		const { data } = await this.#webhookCollectionRepository.requestCollection({ skip: 0, take: 100 });

		// TODO: make this observable / update when webhooks are added/removed/updated
		if (data) {
			this.#webhooks = data.items;
		}
	}
}

// Default export to enable this as a globalContext extension js:
export default UmbAppWebhookContext;

export const UMB_APP_WEBHOOK_CONTEXT = new UmbContextToken<UmbAppWebhookContext>('UmbAppWebhookContext');
