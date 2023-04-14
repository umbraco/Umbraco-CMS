import { UmbContextToken } from '../token/context-token';
import { isUmbContextProvideEventType, umbContextProvideEventType } from '../provide/context-provide.event';
import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer<HostType extends EventTarget = EventTarget, T = unknown> {
	_promise?: Promise<T>;
	_promiseResolver?: (instance: T) => void;

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
		_contextAlias: string | UmbContextToken<T>,
		private _callback?: UmbContextCallback<T>
	) {
		this._contextAlias = _contextAlias.toString();
	}

	protected _onResponse = (instance: T) => {
		// TODO: check that this check is not giving us any problems:
		if (this._instance === instance) {
			return;
		}
		this._instance = instance;
		this._callback?.(instance);
		this._promiseResolver?.(instance);
	};

	public asPromise() {
		return (
			this._promise ||
			(this._promise = new Promise<T>((resolve) => {
				this._instance ? resolve(this._instance) : (this._promiseResolver = resolve);
			}))
		);
	}

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

	// TODO: Test destroy scenarios:
	public destroy() {
		delete this._instance;
		delete this._callback;
		delete this._promise;
		delete this._promiseResolver;
	}
}
