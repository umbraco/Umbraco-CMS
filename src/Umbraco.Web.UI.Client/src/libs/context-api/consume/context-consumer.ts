import type { UmbContextDiscriminator, UmbContextToken } from '../token/context-token.js';
import { isUmbContextProvideEventType, UMB_CONTEXT_PROVIDE_EVENT_TYPE } from '../provide/context-provide.event.js';
import type { UmbContextCallback } from './context-request.event.js';
import { UmbContextRequestEventImplementation } from './context-request.event.js';

type HostElementMethod = () => Element | undefined;

export type UmbContextConsumerAsPromiseOptionsType = {
	preventTimeout?: boolean;
};

/**
 * @class UmbContextConsumer
 * @description The context consumer class, used to consume a context from a host element.
 * Notice it is not recommended to use this class directly, but rather use the `consumeContext` method from a `UmbElement` or `UmbElementMixin` or `UmbControllerBase` or `UmbClassMixin`.
 * Alternatively, you can use the `UmbContextConsumerController` to consume a context from a host element. But this does require that you can implement one of the Class Mixins mentioned above.
 */
export class UmbContextConsumer<BaseType = unknown, ResultType extends BaseType = BaseType> {
	protected _retrieveHost: HostElementMethod;

	#raf?: number;
	#skipHost?: boolean;
	#stopAtContextMatch = true;
	#callback?: UmbContextCallback<ResultType>;
	#promise?: Promise<ResultType | undefined>;
	#promiseOptions?: UmbContextConsumerAsPromiseOptionsType;
	#promiseResolver?: (instance: ResultType) => void;
	#promiseRejecter?: (reason: string) => void;

	#instance?: ResultType;
	get instance() {
		return this.#instance;
	}

	#contextAlias: string;
	#apiAlias: string;

	#discriminator?: UmbContextDiscriminator<BaseType, ResultType>;

	/**
	 * Creates an instance of UmbContextConsumer.
	 * @param {Element} host - The host element.
	 * @param {string} contextIdentifier - The context identifier, an alias or a Context Token.
	 * @param {UmbContextCallback} callback - The callback.
	 * @memberof UmbContextConsumer
	 */
	constructor(
		host: Element | HostElementMethod,
		contextIdentifier: string | UmbContextToken<BaseType, ResultType>,
		callback?: UmbContextCallback<ResultType>,
	) {
		if (typeof host === 'function') {
			this._retrieveHost = host;
		} else {
			this._retrieveHost = () => host;
		}
		const idSplit = contextIdentifier.toString().split('#');
		this.#contextAlias = idSplit[0];
		this.#apiAlias = idSplit[1] ?? 'default';
		this.#callback = callback;
		this.#discriminator = (contextIdentifier as UmbContextToken<BaseType, ResultType>).getDiscriminator?.();
	}

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @description Make the consumption skip the contexts provided by the Host element.
	 * @returns {UmbContextConsumer} - The current instance of the UmbContextConsumer.
	 */
	public skipHost() {
		this.#skipHost = true;
		return this;
	}

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @description Pass beyond any context aliases that matches this.
	 * The default behavior is to stop at first Context Alias match, this is to avoid receiving unforeseen descending contexts.
	 * @returns {UmbContextConsumer} - The current instance of the UmbContextConsumer.
	 */
	public passContextAliasMatches() {
		this.#stopAtContextMatch = false;
		return this;
	}

	protected _onResponse = (instance: BaseType): boolean => {
		if (this.#instance === instance) {
			this.#resolvePromise();
			return true;
		}

		if (instance === undefined) {
			throw new Error('Not allowed to set context api instance to undefined.');
		}
		if (this.#discriminator) {
			// Notice if discriminator returns false, we do not want to setInstance. [NL]
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

	protected async setInstance(instance: ResultType): Promise<void> {
		this.#instance = instance;
		await this.#callback?.(instance); // Resolve callback first as it might perform something you like completed before resolving the promise, as the promise might be used to determine when things are ready/initiated [NL]
		this.#resolvePromise();
	}

	#resolvePromise() {
		if (this.#promiseResolver && this.#instance !== undefined) {
			this.#promiseResolver(this.#instance);
			this.#promise = undefined;
			this.#promiseOptions = undefined;
			this.#promiseResolver = undefined;
			this.#promiseRejecter = undefined;
		}
		if (!this.#callback) {
			this.destroy();
		}
	}

	#rejectPromise() {
		if (this.#promiseRejecter) {
			const hostElement = this._retrieveHost();
			// If we still have the rejecter, it means that the context was not found immediately, so lets reject the promise. [NL]
			this.#promiseRejecter?.(
				`Context could not be found. (Context Alias: ${this.#contextAlias} with API Alias: ${this.#apiAlias}). Controller is hosted on ${hostElement?.parentNode?.nodeName ?? 'Not attached node'} > ${hostElement?.nodeName}`,
			);
			this.#promise = undefined;
			this.#promiseOptions = undefined;
			this.#promiseResolver = undefined;
			this.#promiseRejecter = undefined;
		}
		if (!this.#callback) {
			this.destroy();
		}
	}

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @param {UmbContextConsumerAsPromiseOptionsType} options - Prevent the promise from timing out.
	 * @description Get the context as a promise.
	 * @returns {UmbContextConsumer} - A promise that resolves when the context is consumed.
	 */
	public asPromise(options?: UmbContextConsumerAsPromiseOptionsType): Promise<ResultType | undefined> {
		return (
			this.#promise ??
			(this.#promise = new Promise<ResultType | undefined>((resolve, reject) => {
				if (this.#instance) {
					this.#promiseOptions = undefined;
					this.#promiseResolver = undefined;
					this.#promiseRejecter = undefined;
					resolve(this.#instance);
				} else {
					this.#promiseOptions = options;
					this.#promiseResolver = resolve;
					this.#promiseRejecter = reject;
				}
			}))
		);
	}

	/**
	 * @public
	 * @memberof UmbContextConsumer
	 * @description Request the context from the host element.
	 */
	public request(): void {
		if (this.#raf !== undefined) {
			cancelAnimationFrame(this.#raf);
		}

		const event = new UmbContextRequestEventImplementation(
			this.#contextAlias,
			this.#apiAlias,
			this._onResponse,
			this.#stopAtContextMatch,
		);
		(this.#skipHost ? this._retrieveHost()?.parentNode : this._retrieveHost())?.dispatchEvent(event);

		/*
		let i: number = this.#timeoutFrames ?? 1;
		while (i-- > 0 && this.#promiseRejecter) {
			await new Promise((resolve) => requestAnimationFrame(resolve));
		}
		*/

		if (this.#promiseResolver && this.#promiseOptions?.preventTimeout !== true) {
			this.#raf = requestAnimationFrame(() => {
				// For unproviding, then setInstance to undefined here. [NL]
				this.#rejectPromise();
				this.#raf = undefined;
			});
		}
	}

	public hostConnected(): void {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.addEventListener(UMB_CONTEXT_PROVIDE_EVENT_TYPE, this.#handleNewProvider);
		//window.addEventListener(umbContextUnprovidedEventType, this.#handleRemovedProvider);
		this.request();
	}

	public hostDisconnected(): void {
		if (this.#raf !== undefined) {
			cancelAnimationFrame(this.#raf);
			this.#raf = undefined;
		}

		if (this.#promiseRejecter) {
			const hostElement = this._retrieveHost();
			this.#promiseRejecter(
				`Context request was cancelled, host was disconnected. (Context Alias: ${this.#contextAlias} with API Alias: ${this.#apiAlias}). Controller is hosted on ${hostElement?.parentNode?.nodeName ?? 'Not attached node'} > ${hostElement?.nodeName}`,
			);
		}
		this.#promise = undefined;
		this.#promiseOptions = undefined;
		this.#promiseResolver = undefined;
		this.#promiseRejecter = undefined;

		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.removeEventListener(UMB_CONTEXT_PROVIDE_EVENT_TYPE, this.#handleNewProvider);
		//window.removeEventListener(umbContextUnprovidedEventType, this.#handleRemovedProvider);
	}

	#handleNewProvider = (event: Event): void => {
		// Does seem a bit unnecessary, we could just assume the type via type casting...
		if (!isUmbContextProvideEventType(event)) return;

		if (this.#contextAlias === event.contextAlias) {
			this.request();
		}
	};

	//Niels: I'm keeping this code around as it might be relevant, but I wanted to try to see if leaving this feature out for now could work for us. [NL]
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

	public destroy(): void {
		this.hostDisconnected();
		this._retrieveHost = undefined as any;
		this.#callback = undefined;
		this.#promise = undefined;
		this.#promiseOptions = undefined;
		this.#promiseResolver = undefined;
		this.#promiseRejecter = undefined;
		this.#instance = undefined;
		this.#discriminator = undefined;
	}
}
