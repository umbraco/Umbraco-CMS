import { AsyncDirective, directive, nothing, type ElementPart } from '@umbraco-cms/backoffice/external/lit';

/**
 * The `focus` directive sets focus on the given element once its connected to the DOM.
 */
class UmbFocusDirective extends AsyncDirective {
	private _el?: HTMLElement;

	override render() {
		return nothing;
	}

	override update(part: ElementPart) {
		if (this._el !== part.element) {
			// This does feel wrong that we need to wait one render. [NL]
			// Because even if our elements focus method is implemented so it can be called initially, my research shows that calling the focus method at this point is too early, thought the element is connected to the DOM and the focus method is available. [NL]
			// This smells a bit like the DOMPart of which the directive is in is not connected to the main DOM yet, and therefor cant receive focus. [NL]
			// Which is why we need to await one render: [NL]
			requestAnimationFrame(() => {
				(this._el = part.element as HTMLElement).focus();
			});
		}
		return nothing;
	}

	override disconnected() {
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
