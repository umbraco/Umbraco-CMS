import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { HubConnectionBuilder } from '@umbraco-cms/backoffice/external/signalr';
import type { HubConnection } from '@umbraco-cms/backoffice/external/signalr';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Manages the SignalR PreviewHub connection for the visual editor.
 * Invokes the supplied callback with the refreshed document key whenever the
 * server signals that preview content has changed.
 */
export class UmbVisualEditorSignalRController extends UmbControllerBase {
	#connection?: HubConnection;
	#onRefreshed: (documentKey: string) => void;
	#suppressUntil = 0;

	constructor(host: UmbControllerHost, onRefreshed: (documentKey: string) => void) {
		super(host);
		this.#onRefreshed = (documentKey) => {
			if (Date.now() < this.#suppressUntil) return;
			onRefreshed(documentKey);
		};
	}

	/** Ignore `refreshed` events for the given window — used right after a local save so the editor doesn't full-reload its own change. */
	suppressSelfReload(durationMs = 4000) {
		this.#suppressUntil = Date.now() + durationMs;
	}

	async connect(serverUrl: string) {
		if (!serverUrl) return;
		await this.disconnect();

		const hubUrl = `${serverUrl}/umbraco/PreviewHub`;
		this.#connection = new HubConnectionBuilder().withUrl(hubUrl).build();
		this.#connection.on('refreshed', this.#onRefreshed);

		try {
			await this.#connection.start();
		} catch (e) {
			console.error('[VisualEditor] SignalR connection failed', e);
		}
	}

	async disconnect() {
		if (this.#connection) {
			await this.#connection.stop();
			this.#connection = undefined;
		}
	}

	override destroy() {
		this.disconnect();
		super.destroy();
	}
}
