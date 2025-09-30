import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CONTEXT_PROVIDE_EVENT_TYPE, UMB_CONTEXT_REQUEST_EVENT_TYPE } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbContextRequestEvent, UmbContextProvideEvent } from '@umbraco-cms/backoffice/context-api';

const CtrlAlias = Symbol();

/**
 * @internal
 */
export class UmbContextProxyController extends UmbControllerBase {
	#target?: EventTarget;
	#getDestination: () => EventTarget | undefined;
	#ignorer: Array<string> = [];

	constructor(host: UmbControllerHost, target: EventTarget | undefined, getDestination: () => EventTarget | undefined) {
		super(host, CtrlAlias);
		this.#target = target;
		this.#getDestination = getDestination;

		// Only handle something if there is a target. This could seem stupid, but since we support construction this controller despite a missing element, we enable the controller to destroy an already existing controller. aka replace it. [NL]
		if (target) {
			target.addEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, this.#onContextRequest as EventListener);
			target.addEventListener(UMB_CONTEXT_PROVIDE_EVENT_TYPE, this.#onContextProvide as EventListener);
		}
	}

	/* We do not currently have a good enough control to ensure that the proxy is last, meaning if another context is provided at this element, it might respond after the proxy event has been dispatched.
	To avoid such you can declare context aliases to be ignorer by the proxy.
	*/
	setIgnoreContextAliases(aliases: Array<string>) {
		this.#ignorer = aliases;
		return this;
	}

	#onContextRequest = (event: UmbContextRequestEvent) => {
		const destination = this.#getDestination();
		if (destination && !this.#ignorer.includes(event.contextAlias)) {
			event.stopImmediatePropagation();
			destination.dispatchEvent(event.clone());
		}
	};
	#onContextProvide = (event: UmbContextProvideEvent) => {
		const destination = this.#getDestination();
		if (destination) {
			event.stopPropagation();
			destination.dispatchEvent(event.clone());
		}
	};

	override destroy() {
		super.destroy();
		if (this.#target) {
			this.#target.removeEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, this.#onContextRequest as EventListener);
			this.#target.removeEventListener(UMB_CONTEXT_PROVIDE_EVENT_TYPE, this.#onContextProvide as EventListener);
		}
	}
}
