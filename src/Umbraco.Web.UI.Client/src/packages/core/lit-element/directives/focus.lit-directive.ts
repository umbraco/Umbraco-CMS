import { AsyncDirective, directive, nothing, type ElementPart } from '@umbraco-cms/backoffice/external/lit';
/**
 *
 * test if a element has focus
 * this also returns true if the focused element is a child of the target.
 * @param current
 * @param target
 * @returns bool
 */
function hasFocus(current: any, target: HTMLElement): boolean {
	if (current === target) {
		return true;
	}
	if (current.shadowRoot) {
		const node = current.shadowRoot.activeElement;
		if (node) {
			return hasFocus(node, target);
		}
	}
	return false;
}

/**
 * The `focus` directive sets focus on the given element once its connected to the DOM.
 */
class UmbFocusDirective extends AsyncDirective {
	static #next?: HTMLElement;
	#el?: HTMLElement;
	#timeout?: number;

	override render() {
		return nothing;
	}

	override update(part: ElementPart) {
		if (this.#el !== part.element) {
			UmbFocusDirective.#next = this.#el = part.element as HTMLElement;
			this.#setFocus();
		}
		return nothing;
	}

	/**
	 * This method tries to set focus, if it did not succeed, it will try again.
	 * It always tests against the latest element, because the directive can be used multiple times in the same render.
	 * This is NOT needed because the elements focus method isn't ready to be called, but due to something with rendering of the DOM.
	 * But I'm not completely sure at this movement why the browser does not accept the focus call.
	 * But I have tested that everything is in place for it to be good, so something else must have an effect,
	 * setting the focus somewhere else, maybe a re-appending of some sort?
	 * cause Lit does not re-render the element but also notice reconnect callback on the directive is not triggered either. [NL]
	 */
	#setFocus = () => {
		// Make sure we clear the timeout, so we don't get multiple timeouts running.
		if (this.#timeout) {
			clearTimeout(this.#timeout);
			this.#timeout = undefined;
		}
		// If this is the next element to focus, then try to focus it.
		if (this.#el && this.#el === UmbFocusDirective.#next) {
			this.#el.focus();
			if (hasFocus(document.activeElement, this.#el) === false) {
				this.#timeout = setTimeout(this.#setFocus, 100) as unknown as number;
			} else {
				UmbFocusDirective.#next = undefined;
			}
		}
	};

	override disconnected() {
		if (this.#el === UmbFocusDirective.#next) {
			UmbFocusDirective.#next = undefined;
		}
		this.#el = undefined;
	}

	//override reconnected() {}
}

/**
 * @description
 * A Lit directive, which sets focus on the element of scope once its connected to the DOM.
 * @example:
 * ```js
 * html`<input ${umbFocus()}>`;
 * ```
 */
export const umbFocus = directive(UmbFocusDirective);

//export type { UmbFocusDirective };
