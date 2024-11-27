import { contextData, UMB_DEBUG_CONTEXT_EVENT_TYPE } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

// Temp controller to get the code away from the app.element.ts
export class UmbContextDebugController extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	override hostConnected(): void {
		super.hostConnected();
		// Maybe this could be part of the context-api? When we create a new root, we could attach the debugger to it?
		// Listen for the debug event from the <umb-debug> component
		this.getHostElement().addEventListener(
			UMB_DEBUG_CONTEXT_EVENT_TYPE,
			this.#onContextDebug as unknown as EventListener,
		);
	}

	#onContextDebug = (event: any) => {
		// Once we got to the outter most component <umb-app>
		// we can send the event containing all the contexts
		// we have collected whilst coming up through the DOM
		// and pass it back down to the callback in
		// the <umb-debug> component that originally fired the event
		if (event.callback) {
			event.callback(event.instances);
		}

		// Massage the data into a simplier format
		// Why? Can't send contexts data directly - browser seems to not serialize it and says its null
		// But a simple object works fine for browser extension to consume
		const data = {
			contexts: contextData(event.instances),
		};

		// Emit this new event for the browser extension to listen for
		this.getHostElement().dispatchEvent(new CustomEvent('umb:debug-contexts:data', { detail: data, bubbles: true }));
	};

	override hostDisconnected(): void {
		super.hostDisconnected();
		this.getHostElement().removeEventListener(
			UMB_DEBUG_CONTEXT_EVENT_TYPE,
			this.#onContextDebug as unknown as EventListener,
		);
	}
}
