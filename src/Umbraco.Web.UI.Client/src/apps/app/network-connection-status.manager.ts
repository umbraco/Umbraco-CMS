import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import {
	UMB_NOTIFICATION_CONTEXT,
	type UmbNotificationContext,
	type UmbNotificationHandler,
} from '@umbraco-cms/backoffice/notification';

export class UmbNetworkConnectionStatusManager extends UmbControllerBase {
	#notificationContext?: UmbNotificationContext;
	#offlineNotification?: UmbNotificationHandler;

	#localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
			this.#notificationContext = notificationContext;
		});

		window.addEventListener('online', () => this.#onOnline());
		window.addEventListener('offline', () => this.#onOffline());
	}

	#onOnline() {
		this.#offlineNotification?.close();
		this.#notificationContext?.peek('positive', {
			data: {
				headline: this.#localize.term('speechBubbles_onlineHeadline'),
				message: this.#localize.term('speechBubbles_onlineMessage'),
			},
		});
	}

	#onOffline() {
		this.#offlineNotification = this.#notificationContext?.stay('danger', {
			data: {
				headline: this.#localize.term('speechBubbles_offlineHeadline'),
				message: this.#localize.term('speechBubbles_offlineMessage'),
			},
		});
	}

	override destroy() {
		this.#offlineNotification?.close();
		this.removeEventListener('online', () => this.#onOnline());
		this.removeEventListener('offline', () => this.#onOffline());
		super.destroy();
	}
}
