import { AsyncDirective, directive, nothing, type ElementPart } from '@umbraco-cms/backoffice/external/lit';

class UmbFocusDirective extends AsyncDirective {
	private _el?: HTMLElement;

	render() {
		return nothing;
	}

	override update(part: ElementPart) {
		if (this._el !== part.element) {
			(this._el = part.element as HTMLElement).focus();
		}
		return nothing;
	}

	override disconnected() {
		this._el = undefined;
	}

	//override reconnected() {}
}

/**
 * Sets focus on the given element once its connected to the DOM.
 *
 * ```js
 * render(html`<input ${focus}>`, container);
 * ```
 */
export const focus = directive(UmbFocusDirective);

export type { UmbFocusDirective };
