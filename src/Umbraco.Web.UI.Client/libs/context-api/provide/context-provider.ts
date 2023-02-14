import { umbContextRequestEventType, isUmbContextRequestEvent } from '../consume/context-request.event';
import { UmbContextToken } from '../context-token';
import { UmbContextProvideEventImplementation } from './context-provide.event';

/**
 * @export
 * @class UmbContextProvider
 */
export class UmbContextProvider<HostType extends EventTarget = EventTarget> {
	protected host: HostType;

	protected _contextAlias: string;
	#instance: unknown;

	/**
	 * Creates an instance of UmbContextProvider.
	 * @param {EventTarget} host
	 * @param {string} contextAlias
	 * @param {*} instance
	 * @memberof UmbContextProvider
	 */
	constructor(host: HostType, contextAlias: string | UmbContextToken, instance: unknown) {
		this.host = host;
		this._contextAlias = contextAlias.toString();
		this.#instance = instance;
	}

	/**
	 * @memberof UmbContextProvider
	 */
	public hostConnected() {
		this.host.addEventListener(umbContextRequestEventType, this._handleContextRequest);
		this.host.dispatchEvent(new UmbContextProvideEventImplementation(this._contextAlias));

		// Listen to our debug event 'umb:debug-contexts'
		this.host.addEventListener('umb:debug-contexts', this._handleDebugContextRequest);
	}

	/**
	 * @memberof UmbContextProvider
	 */
	public hostDisconnected() {
		this.host.removeEventListener(umbContextRequestEventType, this._handleContextRequest);
		// TODO: fire unprovided event.
	}

	/**
	 * @private
	 * @param {UmbContextRequestEvent} event
	 * @memberof UmbContextProvider
	 */
	private _handleContextRequest = (event: Event) => {
		if (!isUmbContextRequestEvent(event)) return;
		if (event.contextAlias !== this._contextAlias) return;

		event.stopPropagation();
		event.callback(this.#instance);
	};

	private _handleDebugContextRequest = (event: Event) => {

		
		console.log('Context Alias:', this._contextAlias);
		console.log('Context Instance:', this.#instance);

		// Do I update an array on the event which
		// The Debug element can then render in UI?!
		console.log('Event:', event);
	};


	destroy(): void {
		// I want to make sure to call this, but for now it was too overwhelming to require the destroy method on context instances.
		(this.#instance as any).destroy?.();
	};
}
