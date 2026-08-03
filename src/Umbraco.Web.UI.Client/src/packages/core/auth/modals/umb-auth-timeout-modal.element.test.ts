import type { UmbAuthTimeoutModalElement } from './umb-auth-timeout-modal.element.js';
import './umb-auth-timeout-modal.element.js';
import { aTimeout, expect } from '@open-wc/testing';

describe('UmbAuthTimeoutModalElement', () => {
	let element: UmbAuthTimeoutModalElement;
	let expiredCalls: number;
	const realDateNow = Date.now;

	beforeEach(() => {
		expiredCalls = 0;
		element = document.createElement('umb-auth-timeout-modal');
		element.data = {
			remainingTimeInSeconds: 30,
			onLogout: () => {},
			onContinue: () => {},
			onExpired: () => {
				expiredCalls++;
			},
		};
	});

	afterEach(() => {
		Date.now = realDateNow;
		element.remove();
	});

	it('counts down once per second', async () => {
		document.body.appendChild(element);
		await aTimeout(1200);

		expect(expiredCalls).to.equal(0);
		// Allow for a slow event loop having delivered an extra tick
		expect(element.shadowRoot?.textContent).to.match(/2[89]/);
	});

	it('expires based on the wall clock, not the tick count (e.g. after system sleep)', async () => {
		document.body.appendChild(element);

		// Simulate system sleep / background-tab throttling: the wall clock jumps past the
		// deadline while the countdown interval has not been firing.
		Date.now = () => realDateNow() + 60_000;

		// Wait for a single interval tick — it should detect the deadline has passed.
		await aTimeout(1200);

		expect(expiredCalls).to.equal(1);
	});
});
