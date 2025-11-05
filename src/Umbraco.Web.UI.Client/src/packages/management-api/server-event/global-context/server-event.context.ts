import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from './server-event.context-token.js';
import type { UmbManagementApiServerEventModel } from './types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { HubConnectionBuilder, type HubConnection } from '@umbraco-cms/backoffice/external/signalr';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { filter, Subject } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbManagementApiServerEventContext extends UmbContextBase {
	#connection?: HubConnection;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#serverContext?: typeof UMB_SERVER_CONTEXT.TYPE;

	#events = new Subject<UmbManagementApiServerEventModel>();
	public readonly events = this.#events.asObservable();

	#isConnected = new UmbBooleanState(undefined);
	public readonly isConnected = this.#isConnected.asObservable();

	/**
	 * Filters events by the given event types
	 * @param {string} eventTypes - The event types to filter by
	 * @returns {Observable<UmbManagementApiServerEventModel>} - The filtered events
	 * @memberof UmbManagementApiServerEventContext
	 */
	byEventSource(eventTypes: string): Observable<UmbManagementApiServerEventModel> {
		return this.#events.asObservable().pipe(filter((event) => event.eventType === eventTypes));
	}

	/**
	 * Filters events by the given event sources and event types
	 * @param {Array<string>} eventSources - The event sources to filter by
	 * @param {Array<string>} eventTypes - The event types to filter by
	 * @returns {Observable<UmbManagementApiServerEventModel>} - The filtered events
	 * @memberof UmbManagementApiServerEventContext
	 */
	byEventSourcesAndEventTypes(
		eventSources: Array<string>,
		eventTypes: Array<string>,
	): Observable<UmbManagementApiServerEventModel> {
		return this.#events
			.asObservable()
			.pipe(filter((event) => eventSources.includes(event.eventSource) && eventTypes.includes(event.eventType)));
	}

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
				this.#isConnected.setValue(false);
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

		this.#connection.on('notify', (payload) => {
			const event: UmbManagementApiServerEventModel = {
				...payload,
				clientTimestamp: new Date().toISOString(),
			};

			this.#events.next(event);
		});

		this.#connection
			.start()
			.then(() => this.#isConnected.setValue(true))
			.catch(() => this.#isConnected.setValue(false));

		this.#connection.onclose(() => this.#isConnected.setValue(false));
	}
}

export { UmbManagementApiServerEventContext as api };
