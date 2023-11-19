import {
	UmbContextRequestEvent,
	UMB_ContextRequestEventType,
	UMB_umbDebugContextEventType,
} from '../consume/context-request.event.js';
import { UmbContextToken } from '../token/context-token.js';
import {
	UmbContextProvideEventImplementation,
	//UmbContextUnprovidedEventImplementation,
} from './context-provide.event.js';

/**
 * @export
 * @class UmbContextProvider
 */
export class UmbContextProvider<BaseType = unknown, ResultType extends BaseType = BaseType> {
	protected hostElement: EventTarget;

	protected _contextAlias: string;
	#instance: unknown;

	/**
	 * Method to enable comparing the context providers by the instance they provide.
	 * Note this method should have a unique name for the provider controller, for it not to be confused with a consumer.
	 * @returns {*}
	 */
	public providerInstance() {
		return this.#instance;
	}

	/**
	 * Creates an instance of UmbContextProvider.
	 * @param {EventTarget} host
	 * @param {string} contextAlias
	 * @param {*} instance
	 * @memberof UmbContextProvider
	 */
	constructor(
		hostElement: EventTarget,
		contextAlias: string | UmbContextToken<BaseType, ResultType>,
		instance: ResultType,
	) {
		this.hostElement = hostElement;
		this._contextAlias = contextAlias.toString();
		this.#instance = instance;
	}

	/**
	 * @private
	 * @param {UmbContextRequestEvent} event
	 * @memberof UmbContextProvider
	 */
	#handleContextRequest = ((event: UmbContextRequestEvent) => {
		if (event.contextAlias !== this._contextAlias) return;

		// Since the alias matches, we will stop it from bubbling further up. But we still allow it to ask the other Contexts of the element. Hence not calling `event.stopImmediatePropagation();`
		event.stopPropagation();

		if (event.callback(this.#instance)) {
			// Make sure the event not hits any more Contexts as we have found a match.
			event.stopImmediatePropagation();
		}
	}) as EventListener;

	/**
	 * @memberof UmbContextProvider
	 */
	public hostConnected() {
		this.hostElement.addEventListener(UMB_ContextRequestEventType, this.#handleContextRequest);
		this.hostElement.dispatchEvent(new UmbContextProvideEventImplementation(this._contextAlias));

		// Listen to our debug event 'umb:debug-contexts'
		this.hostElement.addEventListener(UMB_umbDebugContextEventType, this._handleDebugContextRequest);
	}

	/**
	 * @memberof UmbContextProvider
	 */
	public hostDisconnected() {
		this.hostElement.removeEventListener(UMB_ContextRequestEventType, this.#handleContextRequest);
		// Out-commented for now, but kept if we like to reintroduce this:
		//window.dispatchEvent(new UmbContextUnprovidedEventImplementation(this._contextAlias, this.#instance));

		// Stop listen to our debug event 'umb:debug-contexts'
		this.hostElement.removeEventListener(UMB_umbDebugContextEventType, this._handleDebugContextRequest);
	}

	private _handleDebugContextRequest = (event: any) => {
		// If the event doesn't have an instances property, create it.
		if (!event.instances) {
			event.instances = new Map();
		}

		// If the event doesn't have an instance for this context, add it.
		// Nearest to the DOM element of <umb-debug> will be added first
		// as contexts can change/override deeper in the DOM
		if (!event.instances.has(this._contextAlias)) {
			event.instances.set(this._contextAlias, this.#instance);
		}
	};

	destroy(): void {
		// We want to call a destroy method on the instance, if it has one.
		(this.#instance as any)?.destroy?.();
		this.#instance = undefined;
	}
}
