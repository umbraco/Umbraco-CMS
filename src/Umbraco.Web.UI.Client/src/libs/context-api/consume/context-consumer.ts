import { UmbContextToken } from '../token/context-token.js';
import {
	isUmbContextProvideEventType,
	isUmbContextUnprovidedEventType,
	umbContextProvideEventType,
	umbContextUnprovidedEventType,
} from '../provide/context-provide.event.js';
import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event.js';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer<T = unknown> {
	#callback?: UmbContextCallback<T | undefined>;
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
		callback?: UmbContextCallback<T | undefined>
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
		if (instance !== undefined) {
			this.#promiseResolver?.(instance);
		}
	};

	public asPromise() {
		return new Promise<T>((resolve) => {
			this.#instance ? resolve(this.#instance) : (this.#promiseResolver = resolve);
		});
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
		window.addEventListener(umbContextUnprovidedEventType, this._handleRemovedProvider);
		this.request();
	}

	public hostDisconnected() {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.removeEventListener(umbContextProvideEventType, this._handleNewProvider);
		window.removeEventListener(umbContextUnprovidedEventType, this._handleRemovedProvider);
	}

	private _handleNewProvider = (event: Event) => {
		// Does seem a bit unnecessary, we could just assume the type via type casting...
		if (!isUmbContextProvideEventType(event)) return;

		if (this.#contextAlias === event.contextAlias) {
			this.request();
		}
	};

	private _handleRemovedProvider = (event: Event) => {
		// Does seem a bit unnecessary, we could just assume the type via type casting...
		if (!isUmbContextUnprovidedEventType(event)) return;

		if (
			this.#instance !== undefined &&
			this.#contextAlias === event.contextAlias &&
			event.instance === this.#instance
		) {
			this.#instance = undefined;
			this.#callback?.(undefined);
		}
	};

	// TODO: Test destroy scenarios:
	public destroy() {
		this.hostDisconnected();
		this.#callback = undefined;
		this.#promiseResolver = undefined;
		this.#instance = undefined;
	}
}
