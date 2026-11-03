import { UmbBlockGridEntryElement } from './block-grid-entry.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';

describe('UmbBlockGridEntryElement', () => {
	let container: HTMLElement;
	let element: UmbBlockGridEntryElement;

	beforeEach(async () => {
		container = await fixture(html`<div style="width: 0"><umb-block-grid-entry></umb-block-grid-entry></div>`);
		element = container.querySelector('umb-block-grid-entry')!;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbBlockGridEntryElement);
	});

	describe('when disconnected', () => {
		const originalSetTimeout = window.setTimeout;
		let scheduledCallbacks: Array<TimerHandler>;

		beforeEach(() => {
			scheduledCallbacks = [];
		});

		afterEach(() => {
			window.setTimeout = originalSetTimeout;
		});

		it('stops scheduling layout measurements', async () => {
			// Let the measurement loop start while the layout container has no width.
			await aTimeout(250);

			// Detach the surrounding subtree, so the entry keeps its (now detached) parent element.
			container.remove();

			window.setTimeout = ((handler: TimerHandler, timeout?: number, ...args: Array<unknown>) => {
				if (timeout === 100) scheduledCallbacks.push(handler);
				return originalSetTimeout(handler, timeout, ...args);
			}) as typeof window.setTimeout;

			await aTimeout(350);

			expect(scheduledCallbacks).to.have.lengthOf(0);
		});
	});
});
