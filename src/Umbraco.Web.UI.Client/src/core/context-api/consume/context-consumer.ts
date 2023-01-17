import { UmbContextAlias } from '../context-alias';
import { isUmbContextProvideEventType, umbContextProvideEventType } from '../provide/context-provide.event';
import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer<HostType extends EventTarget = EventTarget, T = unknown> {
	private _instance?: T;
	get instance() {
		return this._instance;
	}

	private _contextAlias: string;
	get consumerAlias(): string {
		return this._contextAlias;
	}

	/**
	 * Creates an instance of UmbContextConsumer.
	 * @param {EventTarget} host
	 * @param {string} _contextAlias
	 * @param {UmbContextCallback} _callback
	 * @memberof UmbContextConsumer
	 */
	constructor(
		protected host: HostType,
		_contextAlias: string | UmbContextAlias<T>,
		private _callback: UmbContextCallback<T>
	) {
		this._contextAlias = _contextAlias.toString();
	}

	private _onResponse = (instance: T) => {
		this._instance = instance;
		this._callback(instance);
	};

	/**
	 * @memberof UmbContextConsumer
	 */
	public request() {
		const event = new UmbContextRequestEventImplementation(this._contextAlias, this._onResponse);
		this.host.dispatchEvent(event);
	}

	public hostConnected() {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.addEventListener(umbContextProvideEventType, this._handleNewProvider);
		this.request();
	}

	public hostDisconnected() {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.removeEventListener(umbContextProvideEventType, this._handleNewProvider);
	}

	private _handleNewProvider = (event: Event) => {
		if (!isUmbContextProvideEventType(event)) return;

		if (this._contextAlias === event.contextAlias) {
			this.request();
		}
	};
}
