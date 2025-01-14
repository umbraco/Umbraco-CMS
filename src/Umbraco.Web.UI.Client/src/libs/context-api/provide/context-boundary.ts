import type { UmbContextRequestEvent } from '../consume/context-request.event.js';
import type { UmbContextToken } from '../token/index.js';
import { UMB_CONTEXT_REQUEST_EVENT_TYPE } from '../consume/context-request.event.js';
import { UmbContextProvideEventImplementation } from './context-provide.event.js';

/**
 * @class UmbContextBoundary
 */
export class UmbContextBoundary {
	#eventTarget: EventTarget;

	#contextAlias: string;

	/**
	 * Creates an instance of UmbContextBoundary.
	 * @param {EventTarget} eventTarget - the host element for this context provider
	 * @param {string | UmbContextToken} contextIdentifier - a string or token to identify the context
	 * @param {*} instance - the instance to provide
	 * @memberof UmbContextBoundary
	 */
	constructor(eventTarget: EventTarget, contextIdentifier: string | UmbContextToken<any>) {
		this.#eventTarget = eventTarget;

		const idSplit = contextIdentifier.toString().split('#');
		this.#contextAlias = idSplit[0];

		this.#eventTarget.addEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, this.#handleContextRequest);
	}

	/**
	 * @private
	 * @param {UmbContextRequestEvent} event - the event to handle
	 * @memberof UmbContextBoundary
	 */
	#handleContextRequest = ((event: UmbContextRequestEvent): void => {
		if (event.contextAlias !== this.#contextAlias) return;

		if (event.stopAtContextMatch) {
			// Since the alias matches, we will stop it from bubbling further up. But we still allow it to ask the other Contexts of the element. Hence not calling `event.stopImmediatePropagation();`
			event.stopPropagation();
		}
	}) as EventListener;

	/**
	 * @memberof UmbContextBoundary
	 */
	public hostConnected(): void {
		//this.hostElement.addEventListener(UMB_CONTENT_REQUEST_EVENT_TYPE, this.#handleContextRequest);
		this.#eventTarget.dispatchEvent(new UmbContextProvideEventImplementation(this.#contextAlias));
	}

	/**
	 * @memberof UmbContextBoundary
	 */
	public hostDisconnected(): void {}

	destroy(): void {
		(this.#eventTarget as unknown) = undefined;
	}
}
