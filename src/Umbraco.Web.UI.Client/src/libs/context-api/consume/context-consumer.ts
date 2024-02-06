import type { UmbContextDiscriminator, UmbContextToken } from '../token/context-token.js';
import {
	isUmbContextProvideEventType,
	//isUmbContextUnprovidedEventType,
	UMB_CONTEXT_PROVIDE_EVENT_TYPE,
	//umbContextUnprovidedEventType,
} from '../provide/context-provide.event.js';
import type { UmbContextCallback } from './context-request.event.js';
import { UmbContextRequestEventImplementation } from './context-request.event.js';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer<BaseType = unknown, ResultType extends BaseType = BaseType> {
	#skipOrigin?: boolean;
	#stopAtContextMatch = true;
	#callback?: UmbContextCallback<ResultType>;
	#promise?: Promise<ResultType>;
	#promiseResolver?: (instance: ResultType) => void;

	#instance?: ResultType;
	get instance() {
		return this.#instance;
	}

	#contextAlias: string;
	#apiAlias: string;

	#discriminator?: UmbContextDiscriminator<BaseType, ResultType>;

	/**
	 * Creates an instance of UmbContextConsumer.
	 * @param {Element} element
	 * @param {string} contextIdentifier
	 * @param {UmbContextCallback} callback
	 * @memberof UmbContextConsumer
	 */
	constructor(
		protected element: Element,
		contextIdentifier: string | UmbContextToken<BaseType, ResultType>,
		callback?: UmbContextCallback<ResultType>,
	) {
		const idSplit = contextIdentifier.toString().split('#');
		this.#contextAlias = idSplit[0];
		this.#apiAlias = idSplit[1] ?? 'default';
		this.#callback = callback;
		this.#discriminator = (contextIdentifier as UmbContextToken<BaseType, ResultType>).getDiscriminator?.();
	}

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @description Skip the contexts provided by the requesting element.
	 */
	public skipOrigin() {
		this.#skipOrigin = true;
	}

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @description Pass beyond any context aliases that matches this.
	 * The default behavior is to stop at first Context Alias match, this is to avoid receiving unforeseen descending contexts.
	 */
	public passContextAliasMatches() {
		this.#stopAtContextMatch = false;
	}

	protected _onResponse = (instance: BaseType): boolean => {
		if (this.#instance === instance) {
			return true;
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

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @description Get the context as a promise.
	 */
	public asPromise() {
		return (
			this.#promise ??
			(this.#promise = new Promise<ResultType>((resolve) => {
				this.#instance ? resolve(this.#instance) : (this.#promiseResolver = resolve);
			}))
		);
	}

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @description Request the context from the host element.
	 */
	public request() {
		const event = new UmbContextRequestEventImplementation(
			this.#contextAlias,
			this.#apiAlias,
			this._onResponse,
			this.#stopAtContextMatch,
		);
		(this.#skipOrigin ? this.element.parentNode : this.element)?.dispatchEvent(event);
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

	public destroy() {
		this.hostDisconnected();
		this.#callback = undefined;
		this.#promise = undefined;
		this.#promiseResolver = undefined;
		this.#instance = undefined;
	}
}
