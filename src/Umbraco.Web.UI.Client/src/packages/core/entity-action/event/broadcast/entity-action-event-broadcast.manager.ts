import { UmbEntityActionEvent } from '../../entity-action.event.js';
import { UmbEntityEvent } from '../../entity.event.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbEntityActionEventBroadcastManager extends UmbControllerBase {
	#broadcastChannel = new BroadcastChannel('umbraco-backoffice-entity-action-event');
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;

			// Broadcast all events from the action event context to the broadcast channel
			this.observe(this.#actionEventContext?.events, (event) => {
				if (event instanceof UmbEntityActionEvent) {
					this.#broadcastEvent(event);
				}
			});
		});

		this.#broadcastChannel.onmessage = (event: MessageEvent) => {
			/* We know when the event is from this channel the data will include
			all the args for a UmbEntityActionEvent */
			const entityEvent = new UmbEntityEvent(event.data);
			this.#actionEventContext?.localDispatchEvent(entityEvent);
		};
	}

	#broadcastEvent(event: UmbEntityActionEvent) {
		const eventObject = event.toObject();
		this.#broadcastChannel.postMessage(eventObject);
	}
}
