import { UMB_SERVER_EVENT_CONTEXT } from './server-event.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import type { HubConnection } from '@microsoft/signalr';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbServerEventModel<DataType = unknown> {
	type: string;
	user: UmbReferenceByUnique;
	data: DataType;
}

export class UmbServerEventContext extends UmbContextBase {
	//public readonly hub = new Subject<UmbServerEventModel>();

	#connection?: HubConnection;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_SERVER_EVENT_CONTEXT);

		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;
			this.#observeIsAuthorized();
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

		this.#connection.on('notify', (payload: unknown) => {
			console.log('payloadReceived', payload);
			//this.hub.next(payload);
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
