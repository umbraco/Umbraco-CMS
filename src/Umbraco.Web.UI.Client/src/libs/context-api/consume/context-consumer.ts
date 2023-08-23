import { UmbContextDiscriminator, UmbContextToken } from '../token/context-token.js';
import {
	isUmbContextProvideEventType,
	//isUmbContextUnprovidedEventType,
	umbContextProvideEventType,
	//umbContextUnprovidedEventType,
} from '../provide/context-provide.event.js';
import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event.js';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer<BaseType = unknown, DiscriminatedType extends BaseType = BaseType> {
	#callback?: UmbContextCallback<DiscriminatedType>;
	#promise?: Promise<DiscriminatedType>;
	#promiseResolver?: (instance: DiscriminatedType) => void;

	#instance?: DiscriminatedType;
	get instance() {
		return this.#instance;
	}

	#contextAlias: string;

	#discriminator?: UmbContextDiscriminator<BaseType, DiscriminatedType>;

	/**
	 * Creates an instance of UmbContextConsumer.
	 * @param {EventTarget} hostElement
	 * @param {string} contextAlias
	 * @param {UmbContextCallback} callback
	 * @memberof UmbContextConsumer
	 */
	constructor(
		protected hostElement: EventTarget,
		contextAlias: string | UmbContextToken<BaseType, DiscriminatedType>,
		callback?: UmbContextCallback<DiscriminatedType>
	) {
		this.#contextAlias = contextAlias.toString();
		this.#callback = callback;
		this.#discriminator = (contextAlias as any).getDiscriminator?.();
	}


	/* Idea: Niels: If we need to filter for specific contexts, we could make the response method return true/false. If false, the event should then then not be stopped. Alternatively parse the event it self on to the response-callback.
	This will enable the event to continue to bubble up finding a context that matches.
	The reason for such would be to have some who are more specific than others. For example, some might just need the current workspace-context, others might need the closest handling a certain entityType.
	As I'm writing this is not relevant, but I wanted to keep the idea as we have had some circumstance that might be solved with this approach.
	*/
	protected _onResponse = (instance: BaseType) => {
		if (this.#instance === instance) {
			return;
		}
		if(this.#discriminator) {
			// Notice if discriminator returns false, we do not want to setInstance.
			if(this.#discriminator(instance)) {
				this.setInstance(instance);
			}
		} else {
			this.setInstance(instance as DiscriminatedType);
		}
	};

	protected setInstance(instance: DiscriminatedType) {
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
			(this.#promise = new Promise<DiscriminatedType>((resolve) => {
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
		window.addEventListener(umbContextProvideEventType, this.#handleNewProvider);
		//window.addEventListener(umbContextUnprovidedEventType, this.#handleRemovedProvider);
		this.request();
	}

	public hostDisconnected() {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.removeEventListener(umbContextProvideEventType, this.#handleNewProvider);
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
