import type { UmbContextRequestEvent } from '../consume/context-request.event.js';
import type { UmbContextToken } from '../token/index.js';
import { UMB_CONTEXT_REQUEST_EVENT_TYPE, UMB_DEBUG_CONTEXT_EVENT_TYPE } from '../consume/context-request.event.js';
import { UmbContextProvideEventImplementation } from './context-provide.event.js';

/**
 * @class UmbContextProvider
 */
export class UmbContextProvider<BaseType = unknown, ResultType extends BaseType = BaseType> {
	#eventTarget: EventTarget;

	#contextAlias: string;
	#apiAlias: string;
	#instance: unknown;

	/**
	 * Method to enable comparing the context providers by the instance they provide.
	 * Note this method should have a unique name for the provider controller, for it not to be confused with a consumer.
	 * @returns {*}
	 */
	public providerInstance(): unknown {
		return this.#instance;
	}

	/**
	 * Creates an instance of UmbContextProvider.
	 * @param {EventTarget} eventTarget - the host element for this context provider
	 * @param {string | UmbContextToken} contextIdentifier - a string or token to identify the context
	 * @param {*} instance - the instance to provide
	 * @memberof UmbContextProvider
	 */
	constructor(
		eventTarget: EventTarget,
		contextIdentifier: string | UmbContextToken<BaseType, ResultType>,
		instance: ResultType,
	) {
		this.#eventTarget = eventTarget;

		const idSplit = contextIdentifier.toString().split('#');
		this.#contextAlias = idSplit[0];
		this.#apiAlias = idSplit[1] ?? 'default';
		this.#instance = instance;
	}

	/**
	 * @memberof UmbContextProvider
	 */
	public hostConnected(): void {
		this.#eventTarget.addEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, this.#handleContextRequest);
		this.#eventTarget.addEventListener(UMB_DEBUG_CONTEXT_EVENT_TYPE, this.#handleDebugContextRequest);

		this.#eventTarget.dispatchEvent(new UmbContextProvideEventImplementation(this.#contextAlias));
	}

	/**
	 * @memberof UmbContextProvider
	 */
	public hostDisconnected(): void {
		this.#eventTarget.removeEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, this.#handleContextRequest);
		this.#eventTarget.removeEventListener(UMB_DEBUG_CONTEXT_EVENT_TYPE, this.#handleDebugContextRequest);
		// Out-commented for now, but kept if we like to reintroduce this:
		//window.dispatchEvent(new UmbContextUnprovidedEventImplementation(this._contextAlias, this.#instance));
	}

	/**
	 * @private
	 * @param {UmbContextRequestEvent} event - the event to handle
	 * @memberof UmbContextProvider
	 */
	#handleContextRequest = ((event: UmbContextRequestEvent): void => {
		if (event.contextAlias !== this.#contextAlias) return;

		if (event.stopAtContextMatch) {
			// Since the alias matches, we will stop it from bubbling further up. But we still allow it to ask the other Contexts of the element. Hence not calling `event.stopImmediatePropagation();`
			event.stopPropagation();
		}

		// First and importantly, check that the apiAlias matches and then call the callback. If that returns true then we can stop the event completely.
		if (this.#apiAlias === event.apiAlias && event.callback(this.#instance)) {
			// Make sure the event not hits any more Contexts as we have found a match.
			event.stopImmediatePropagation();
		}
	}) as EventListener;

	/**
	 * @private
	 * @param {UmbContextRequestEvent} event - the event to append awareness to
	 * @memberof UmbContextProvider
	 */
	#handleDebugContextRequest = (event: any): void => {
		// If the event doesn't have an instances property, create it.
		if (!event.instances) {
			event.instances = new Map();
		}

		// If the event doesn't have an instance for this context, add it.
		// Nearest to the DOM element of <umb-debug> will be added first
		// as contexts can change/override deeper in the DOM
		if (!event.instances.has(this.#contextAlias)) {
			event.instances.set(this.#contextAlias, this.#instance);
		}
	};

	destroy(): void {
		// Note we are not removing the event listener in the hostDisconnected, therefor we do it here [NL].
		this.#eventTarget?.removeEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, this.#handleContextRequest);
		this.#eventTarget?.removeEventListener(UMB_DEBUG_CONTEXT_EVENT_TYPE, this.#handleDebugContextRequest);
		// We want to call a destroy method on the instance, if it has one.
		(this.#instance as any)?.destroy?.();
		this.#instance = undefined;
		(this.#eventTarget as unknown) = undefined;
	}
}
