import { UMB_SERVER_EVENT_CONTEXT } from './server-event.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import type { HubConnection } from '@microsoft/signalr';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityEvent } from '@umbraco-cms/backoffice/entity-action';

export interface UmbServerEventModel {
	eventSource: string;
	eventType: string;
	key: string;
}

export class UmbServerEventContext extends UmbContextBase {
	public readonly hub = new Subject<UmbServerEventModel>();

	#connection?: HubConnection;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_SERVER_EVENT_CONTEXT);

		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;
			this.#observeIsAuthorized();
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;
		});
	}

	#observeIsAuthorized() {
		this.observe(this.#authContext?.isAuthorized, async (isAuthorized) => {
			if (isAuthorized) {
				const token = await this.#authContext?.getLatestToken();
				if (token) {
					this.#initHubConnection(token);
				} else {
					throw new Error('No auth token found');
				}
			} else {
				this.#connection?.stop();
				this.#connection = undefined;
			}
		});
	}

	#initHubConnection(token: string) {
		this.#connection = new HubConnectionBuilder()
			.withUrl('https://localhost:44339/umbraco/serverEventHub', {
				accessTokenFactory: () => token,
			})
			.build();

		this.#connection.on('notify', (payload: UmbServerEventModel) => {
			console.log('payloadReceived', payload);

			// TODO: Only for POC purposes, replace with a proper mapping
			const eventSourceToEntity: Record<any, any> = {
				'Umbraco:CMS:Document': 'document',
				'Umbraco:CMS:DocumentType': 'document-type',
				'Umbraco:CMS:Media': 'media',
				'Umbraco:CMS:MediaType': 'media-type',
				'Umbraco:CMS:Member': 'member',
				'Umbraco:CMS:MemberType': 'member-type',
				'Umbraco:CMS:MemberGroup': 'member-group',
				'Umbraco:CMS:DataType': 'data-type',
			};

			// TODO: Only for POC purposes, replace with a proper mapping
			const eventTypeToAction: Record<any, any> = {
				Created: 'entity-created',
				Updated: 'entity-updated',
				Deleted: 'entity-deleted',
				Trashed: 'entity-trashed',
			};

			const event = new UmbEntityEvent({
				type: eventTypeToAction[payload.eventType],
				entityType: eventSourceToEntity[payload.eventSource],
				unique: payload.key,
			});

			this.#actionEventContext?.dispatchEvent(event);
		});

		this.#connection
			.start()
			.then(function () {
				console.log('Connected!');
			})
			.catch(function (err) {
				console.log(err);
			});

		this.#connection.onclose((err?: Error) => {
			if (err) {
				this.hub.error(err);
			} else {
				this.hub.complete();
			}
		});
	}
}

export { UmbServerEventContext as api };
