import { AsyncDirective, directive, nothing, type ElementPart } from '@umbraco-cms/backoffice/external/lit';

function isDescendant(parent: any, child: any) {
	let node = child.parentNode;
	while (node != null) {
		if (node == parent) {
			return true;
		}
		node = node.host ? node.host : node.parentNode;
	}
	return false;
}

function hasFocus(current: any) {
	if (current.shadowRoot) {
		const node = current.shadowRoot.activeElement;
		if (node) {
			return hasFocus(node);
		}
	}
	return current;
}

/**
 * The `focus` directive sets focus on the given element once its connected to the DOM.
 */
class UmbFocusDirective extends AsyncDirective {
	static _latestElement?: HTMLElement;
	private _el?: HTMLElement;

	override render() {
		return nothing;
	}

	override update(part: ElementPart) {
		if (this._el !== part.element) {
			UmbFocusDirective._latestElement = this._el = part.element as HTMLElement;
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
		if (this._el && this._el === UmbFocusDirective._latestElement) {
			this._el.focus();
			if (hasFocus(document.activeElement)) {
				setTimeout(this.#setFocus, 100);
			}
		}
	};

	override disconnected() {
		if (this._el === UmbFocusDirective._latestElement) {
			UmbFocusDirective._latestElement = undefined;
		}
		this._el = undefined;
	}

	//override reconnected() {}
}

/**
 * @description
 * A Lit directive, which sets focus on the element of scope once its connected to the DOM.
 *
 * @example:
 * ```js
 * html`<input ${umbFocus()}>`;
 * ```
 */
export const umbFocus = directive(UmbFocusDirective);

//export type { UmbFocusDirective };
