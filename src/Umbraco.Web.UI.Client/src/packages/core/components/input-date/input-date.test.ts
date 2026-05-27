import { UmbInputDateElement } from './input-date.element.js';
import { expect, fixture, html, oneEvent } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDateElement', () => {
	let element: UmbInputDateElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-date></umb-input-date> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputDateElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('transient invalid input', () => {
		it('does not reflect an empty native value back while the input is being edited', async () => {
			element.value = '2026-05-27';
			await element.updateComplete;

			const native = element.shadowRoot!.querySelector('input')!;
			native.value = '';
			native.dispatchEvent(new Event('input', { bubbles: true, composed: true }));

			expect(element.value).to.equal('2026-05-27');
		});

		it('reflects a non-empty native value back via the input event', async () => {
			element.value = '2026-05-27';
			await element.updateComplete;

			const native = element.shadowRoot!.querySelector('input')!;
			native.value = '2026-04-27';
			native.dispatchEvent(new Event('input', { bubbles: true, composed: true }));

			expect(element.value).to.equal('2026-04-27');
		});

		it('commits an emptied native value on focusout and dispatches a change event', async () => {
			element.value = '2026-05-27';
			await element.updateComplete;

			const native = element.shadowRoot!.querySelector('input')!;
			native.value = '';

			const changeEvent = oneEvent(element, 'change');
			native.dispatchEvent(new FocusEvent('focusout', { bubbles: true, composed: true }));
			await changeEvent;

			expect(element.value).to.equal('');
		});
	});
});
