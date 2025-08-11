import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from './server-event.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { HubConnectionBuilder, type HubConnection } from '@umbraco-cms/backoffice/external/signalr';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';

interface UmbManagementApiServerEventModel {
	eventSource: string;
	eventType: string;
	key: string;
}

export class UmbManagementApiServerEventContext extends UmbContextBase {
	#connection?: HubConnection;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#serverContext?: typeof UMB_SERVER_CONTEXT.TYPE;

	#events = new Subject<UmbManagementApiServerEventModel>();
	public readonly events = this.#events.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT);

		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;
			this.#observeIsAuthorized();
		});

		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.#serverContext = context;
		});
	}

	#observeIsAuthorized() {
		this.observe(this.#authContext?.isAuthorized, async (isAuthorized) => {
			if (isAuthorized === undefined) return;

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
		const serverURL = this.#serverContext?.getServerUrl();

		if (!serverURL) {
			throw new Error('Server URL is not defined in the server context');
		}

		// TODO: get the url from a server config?
		const serverEventHubUrl = `${serverURL}/umbraco/serverEventHub`;

		this.#connection = new HubConnectionBuilder()
			.withUrl(serverEventHubUrl, {
				accessTokenFactory: () => token,
			})
			.build();

		this.#connection.on('notify', (payload: UmbManagementApiServerEventModel) => {
			this.#events.next(payload);
		});

		this.#connection
			.start()
			.then(function () {
				//console.log('Connected!');
			})
			.catch(function (err) {
				console.log('SignalR', err);
			});

		this.#connection.onclose((err?: Error) => {
			if (err) {
				console.error('Connection closed with error: ', err);
			}
		});
	}
}

export { UmbManagementApiServerEventContext as api };
