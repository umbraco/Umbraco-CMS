import { umbFocus } from './focus.lit-directive.js';
import { expect, fixture, html } from '@open-wc/testing';
import { nothing } from '@umbraco-cms/backoffice/external/lit';

/**
 * An element whose focus() only works once `ready` is set, the way uui-input's
 * focus() cannot land before the component has finished updating (UUI 1.8.0).
 */
class UmbTestLateReadyFocusElement extends HTMLElement {
	ready = false;

	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		this.shadowRoot!.innerHTML = '<input />';
	}

	override focus() {
		if (this.ready) {
			this.shadowRoot!.querySelector('input')!.focus();
		}
	}
}
customElements.define('umb-test-late-ready-focus', UmbTestLateReadyFocusElement);

describe('umbFocus directive', () => {
	it('focuses the element once it is ready to take focus', async () => {
		const el = (await fixture(html`<umb-test-late-ready-focus ${umbFocus()}></umb-test-late-ready-focus>`)) as UmbTestLateReadyFocusElement;
		await new Promise((resolve) => setTimeout(resolve, 50));

		// Not ready on the first attempt, so the directive should have retried.
		el.ready = true;
		await new Promise((resolve) => setTimeout(resolve, 250));
		expect(document.activeElement).to.equal(el);
	});

	it('does not steal focus back once the user has moved it elsewhere', async () => {
		const other = await fixture(html`<input id="already-here" />`);

		// The directive attempts to focus while the element cannot take focus yet,
		// so a retry is scheduled.
		const el = (await fixture(html`<umb-test-late-ready-focus ${umbFocus()}></umb-test-late-ready-focus>`)) as UmbTestLateReadyFocusElement;

		// Before the retry fires, the user focuses another field, and the element
		// becomes ready — a retry now would steal the focus.
		other.focus();
		el.ready = true;

		// Wait past the 100ms retry window.
		await new Promise((resolve) => setTimeout(resolve, 250));
		expect(document.activeElement).to.equal(other);
	});
});
