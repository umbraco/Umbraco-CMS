import { UmbInputDimensionsElement } from './input-dimensions.element.js';
import { expect, fixture, html, oneEvent } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbInputDimensionsElement', () => {
	let element: UmbInputDimensionsElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-input-dimensions></umb-input-dimensions>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputDimensionsElement);
	});

	it('renders with initial width/height values', async () => {
		element = await fixture(html`<umb-input-dimensions .width=${800} .height=${600}></umb-input-dimensions>`);
		expect(element.width).to.equal(800);
		expect(element.height).to.equal(600);

		const widthInput = element.shadowRoot!.querySelector<HTMLInputElement>('#width');
		const heightInput = element.shadowRoot!.querySelector<HTMLInputElement>('#height');
		expect(widthInput?.value).to.equal('800');
		expect(heightInput?.value).to.equal('600');
	});

	it('dispatches change event on width input', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${100} .height=${100} .locked=${false}></umb-input-dimensions>`,
		);

		const widthInput = element.shadowRoot!.querySelector<HTMLInputElement>('#width');
		widthInput!.value = '200';
		const listener = oneEvent(element, 'change');
		widthInput!.dispatchEvent(new Event('input'));
		await listener;

		expect(element.width).to.equal(200);
	});

	it('dispatches change event on height input', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${100} .height=${100} .locked=${false}></umb-input-dimensions>`,
		);

		const heightInput = element.shadowRoot!.querySelector<HTMLInputElement>('#height');
		heightInput!.value = '300';
		const listener = oneEvent(element, 'change');
		heightInput!.dispatchEvent(new Event('input'));
		await listener;

		expect(element.height).to.equal(300);
	});

	it('auto-computes height when locked and width changes', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${800} .height=${400} .locked=${true}></umb-input-dimensions>`,
		);
		// Ratio is 800/400 = 2

		const widthInput = element.shadowRoot!.querySelector<HTMLInputElement>('#width');
		widthInput!.value = '400';
		const listener = oneEvent(element, 'change');
		widthInput!.dispatchEvent(new Event('input'));
		await listener;

		expect(element.width).to.equal(400);
		expect(element.height).to.equal(200); // 400 / 2
	});

	it('auto-computes width when locked and height changes', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${800} .height=${400} .locked=${true}></umb-input-dimensions>`,
		);
		// Ratio is 800/400 = 2

		const heightInput = element.shadowRoot!.querySelector<HTMLInputElement>('#height');
		heightInput!.value = '200';
		const listener = oneEvent(element, 'change');
		heightInput!.dispatchEvent(new Event('input'));
		await listener;

		expect(element.height).to.equal(200);
		expect(element.width).to.equal(400); // 200 * 2
	});

	it('does not affect height when unlocked and width changes', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${800} .height=${400} .locked=${false}></umb-input-dimensions>`,
		);

		const widthInput = element.shadowRoot!.querySelector<HTMLInputElement>('#width');
		widthInput!.value = '600';
		const listener = oneEvent(element, 'change');
		widthInput!.dispatchEvent(new Event('input'));
		await listener;

		expect(element.width).to.equal(600);
		expect(element.height).to.equal(400); // unchanged
	});

	it('recalculates ratio when locking', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${800} .height=${400} .locked=${false}></umb-input-dimensions>`,
		);

		// Change width without lock — height stays
		const widthInput = element.shadowRoot!.querySelector<HTMLInputElement>('#width');
		widthInput!.value = '600';
		widthInput!.dispatchEvent(new Event('input'));
		await element.updateComplete;

		// Now lock — new ratio should be 600/400 = 1.5
		const lockButton = element.shadowRoot!.querySelector<HTMLElement>('uui-button[label]');
		lockButton!.click();
		await element.updateComplete;

		expect(element.locked).to.be.true;

		// Change width — height should follow new ratio
		widthInput!.value = '300';
		const listener = oneEvent(element, 'change');
		widthInput!.dispatchEvent(new Event('input'));
		await listener;

		expect(element.width).to.equal(300);
		expect(element.height).to.equal(200); // 300 / 1.5
	});

	it('rejects invalid input values', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${100} .height=${100} .locked=${false}></umb-input-dimensions>`,
		);

		const widthInput = element.shadowRoot!.querySelector<HTMLInputElement>('#width');

		// Zero
		widthInput!.value = '0';
		widthInput!.dispatchEvent(new Event('input'));
		await element.updateComplete;
		expect(element.width).to.equal(100); // unchanged

		// Negative
		widthInput!.value = '-5';
		widthInput!.dispatchEvent(new Event('input'));
		await element.updateComplete;
		expect(element.width).to.equal(100); // unchanged

		// NaN
		widthInput!.value = 'abc';
		widthInput!.dispatchEvent(new Event('input'));
		await element.updateComplete;
		expect(element.width).to.equal(100); // unchanged
	});

	it('disables inputs and buttons when disabled', async () => {
		element = await fixture(
			html`<umb-input-dimensions .width=${100} .height=${100} .disabled=${true}></umb-input-dimensions>`,
		);

		const widthInput = element.shadowRoot!.querySelector<HTMLElement>('#width');
		const heightInput = element.shadowRoot!.querySelector<HTMLElement>('#height');
		const lockButton = element.shadowRoot!.querySelector<HTMLElement>('uui-button');

		expect(widthInput?.hasAttribute('disabled')).to.be.true;
		expect(heightInput?.hasAttribute('disabled')).to.be.true;
		expect(lockButton?.hasAttribute('disabled')).to.be.true;
	});

	it('does not show reset button when no natural dimensions', async () => {
		element = await fixture(html`<umb-input-dimensions .width=${100} .height=${100}></umb-input-dimensions>`);

		const buttons = element.shadowRoot!.querySelectorAll('uui-button');
		// Only the lock button
		expect(buttons.length).to.equal(1);
	});

	it('shows reset button when natural dimensions differ from current', async () => {
		element = await fixture(html`
			<umb-input-dimensions
				.width=${400}
				.height=${300}
				.naturalWidth=${800}
				.naturalHeight=${600}>
			</umb-input-dimensions>
		`);

		const buttons = element.shadowRoot!.querySelectorAll('uui-button');
		// Lock button + reset button
		expect(buttons.length).to.equal(2);
	});

	it('hides reset button when dimensions match natural', async () => {
		element = await fixture(html`
			<umb-input-dimensions
				.width=${800}
				.height=${600}
				.naturalWidth=${800}
				.naturalHeight=${600}>
			</umb-input-dimensions>
		`);

		const buttons = element.shadowRoot!.querySelectorAll('uui-button');
		// Only lock button, no reset
		expect(buttons.length).to.equal(1);
	});

	it('resets to natural dimensions when reset button clicked', async () => {
		element = await fixture(html`
			<umb-input-dimensions
				.width=${400}
				.height=${300}
				.naturalWidth=${800}
				.naturalHeight=${600}
				.locked=${false}>
			</umb-input-dimensions>
		`);

		const buttons = element.shadowRoot!.querySelectorAll('uui-button');
		const resetButton = buttons[1]; // second button is reset

		const listener = oneEvent(element, 'change');
		resetButton.click();
		await listener;

		expect(element.width).to.equal(800);
		expect(element.height).to.equal(600);
		expect(element.locked).to.be.true;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
