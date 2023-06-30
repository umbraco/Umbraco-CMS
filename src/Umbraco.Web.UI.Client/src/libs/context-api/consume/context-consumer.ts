import { UmbContextToken } from '../token/context-token.js';
import { isUmbContextProvideEventType, umbContextProvideEventType } from '../provide/context-provide.event.js';
import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event.js';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer<T = unknown> {
	#callback?: UmbContextCallback<T>;
	#promise?: Promise<T>;
	#promiseResolver?: (instance: T) => void;

	#instance?: T;
	get instance() {
		return this.#instance;
	}

	#contextAlias: string;

	/**
	 * Creates an instance of UmbContextConsumer.
	 * @param {EventTarget} hostElement
	 * @param {string} contextAlias
	 * @param {UmbContextCallback} _callback
	 * @memberof UmbContextConsumer
	 */
	constructor(
		protected hostElement: EventTarget,
		contextAlias: string | UmbContextToken<T>,
		callback?: UmbContextCallback<T>
	) {
		this.#contextAlias = contextAlias.toString();
		this.#callback = callback;
	}

	protected _onResponse = (instance: T) => {
		if (this.#instance === instance) {
			return;
		}
		this.#instance = instance;
		this.#callback?.(instance);
		this.#promiseResolver?.(instance);
	};

	public asPromise() {
		return (
			this.#promise ||
			(this.#promise = new Promise<T>((resolve) => {
				this.#instance ? resolve(this.#instance) : (this.#promiseResolver = resolve);
			}))
		);
	}

	/**
	 * @memberof UmbContextConsumer
	 */
	public request() {
		const event = new UmbContextRequestEventImplementation(this.#contextAlias, this._onResponse);
		this.hostElement.dispatchEvent(event);
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

		if (this.#contextAlias === event.contextAlias) {
			this.request();
		}
	};

	// TODO: Test destroy scenarios:
	public destroy() {
		this.hostDisconnected();
		this.#callback = undefined;
		this.#promise = undefined;
		this.#promiseResolver = undefined;
		this.#instance = undefined;
	}
}
