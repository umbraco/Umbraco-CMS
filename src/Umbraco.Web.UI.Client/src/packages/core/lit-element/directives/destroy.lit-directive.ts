import { AsyncDirective, directive, nothing, type ElementPart } from '@umbraco-cms/backoffice/external/lit';

/**
 * The `focus` directive sets focus on the given element once its connected to the DOM.
 */
class UmbDestroyDirective extends AsyncDirective {
	#el?: HTMLElement & { destroy: () => void };
	#disconnectAC?: AbortController;

	override render() {
		return nothing;
	}

	override update(part: ElementPart) {
		this.#el = part.element as any;
		return nothing;
	}

	override disconnected() {
		this.#disconnectAC?.abort();
		const abortController = (this.#disconnectAC = new AbortController());
		queueMicrotask(() => {
			if (!abortController.signal.aborted) {
				this.#disconnectAC = undefined;
				if (this.#el) {
					this.#el.destroy();
					this.#el = undefined;
				}
			}
		});
	}

	override reconnected() {
		if (this.#disconnectAC) {
			this.#disconnectAC.abort();
			this.#disconnectAC = undefined;
		}
	}
}

/**
 * @description
 * A Lit directive, which destroys the element once its disconnected from the DOM.
 * @example:
 * ```js
 * html`<input ${umbDestroyOnDisconnect()}>`;
 * ```
 */
export const umbDestroyOnDisconnect = directive(UmbDestroyDirective);

//export type { UmbDestroyDirective };
