import { UmbContextDiscriminator, UmbContextToken } from '../token/context-token.js';
import {
	isUmbContextProvideEventType,
	//isUmbContextUnprovidedEventType,
	UMB_CONTEXT_PROVIDE_EVENT_TYPE,
	//umbContextUnprovidedEventType,
} from '../provide/context-provide.event.js';
import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event.js';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer<BaseType = unknown, ResultType extends BaseType = BaseType> {
	#callback?: UmbContextCallback<ResultType>;
	#promise?: Promise<ResultType>;
	#promiseResolver?: (instance: ResultType) => void;

	#instance?: ResultType;
	get instance() {
		return this.#instance;
	}

	#contextAlias: string;

	#discriminator?: UmbContextDiscriminator<BaseType, ResultType>;

	/**
	 * Creates an instance of UmbContextConsumer.
	 * @param {EventTarget} hostElement
	 * @param {string} contextAlias
	 * @param {UmbContextCallback} callback
	 * @memberof UmbContextConsumer
	 */
	constructor(
		protected hostElement: EventTarget,
		contextAlias: string | UmbContextToken<BaseType, ResultType>,
		callback?: UmbContextCallback<ResultType>,
	) {
		this.#contextAlias = contextAlias.toString();
		this.#callback = callback;
		this.#discriminator = (contextAlias as UmbContextToken<BaseType, ResultType>).getDiscriminator?.();
	}

	protected _onResponse = (instance: BaseType): boolean => {
		if (this.#instance === instance) {
			return false;
		}
		if (this.#discriminator) {
			// Notice if discriminator returns false, we do not want to setInstance.
			if (this.#discriminator(instance)) {
				this.setInstance(instance as unknown as ResultType);
				return true;
			}
		} else {
			this.setInstance(instance as ResultType);
			return true;
		}
		return false;
	};

	protected setInstance(instance: ResultType) {
		this.#instance = instance;
		this.#callback?.(instance);
		if (instance !== undefined) {
			this.#promiseResolver?.(instance);
			this.#promise = undefined;
		}
	}

	public asPromise() {
		return (
			this.#promise ??
			(this.#promise = new Promise<ResultType>((resolve) => {
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
		window.addEventListener(UMB_CONTEXT_PROVIDE_EVENT_TYPE, this.#handleNewProvider);
		//window.addEventListener(umbContextUnprovidedEventType, this.#handleRemovedProvider);
		this.request();
	}

	public hostDisconnected() {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.removeEventListener(UMB_CONTEXT_PROVIDE_EVENT_TYPE, this.#handleNewProvider);
		//window.removeEventListener(umbContextUnprovidedEventType, this.#handleRemovedProvider);
	}

	#handleNewProvider = (event: Event) => {
		// Does seem a bit unnecessary, we could just assume the type via type casting...
		if (!isUmbContextProvideEventType(event)) return;

		if (this.#contextAlias === event.contextAlias) {
			this.request();
		}
	};

	//Niels: I'm keeping this code around as it might be relevant, but I wanted to try to see if leaving this feature out for now could work for us.
	/*
	#handleRemovedProvider = (event: Event) => {
		// Does seem a bit unnecessary, we could just assume the type via type casting...
		if (!isUmbContextUnprovidedEventType(event)) return;

		if (this.#contextAlias === event.contextAlias && event.instance === this.#instance) {
			this.#unProvide();
		}
	};

	#unProvide() {
		if (this.#instance !== undefined) {
			this.#instance = undefined;
			this.#callback?.(undefined);
		}
	}
	*/

	// TODO: Test destroy scenarios:
	public destroy() {
		this.hostDisconnected();
		this.#callback = undefined;
		this.#promise = undefined;
		this.#promiseResolver = undefined;
		this.#instance = undefined;
	}
}
