import { UmbSignalRReconnectPolicy } from './signalr-reconnect-policy.js';
import type { RetryContext } from '@umbraco-cms/backoffice/external/signalr';
import { expect } from '@open-wc/testing';

const retryContext = (previousRetryCount: number): RetryContext => ({
	previousRetryCount,
	elapsedMilliseconds: 0,
	retryReason: new Error('test'),
});

describe('UmbSignalRReconnectPolicy', () => {
	let policy: UmbSignalRReconnectPolicy;

	beforeEach(() => {
		policy = new UmbSignalRReconnectPolicy();
	});

	it('reconnects immediately on the first attempt', () => {
		expect(policy.nextRetryDelayInMilliseconds(retryContext(0))).to.equal(0);
	});

	it('backs off over the next attempts', () => {
		expect(policy.nextRetryDelayInMilliseconds(retryContext(1))).to.equal(2000);
		expect(policy.nextRetryDelayInMilliseconds(retryContext(2))).to.equal(5000);
		expect(policy.nextRetryDelayInMilliseconds(retryContext(3))).to.equal(10000);
	});

	it('caps the delay and keeps retrying indefinitely', () => {
		// Beyond the explicit schedule the delay is capped, and a number (never null) is always
		// returned so the connection never gives up.
		for (const count of [4, 10, 100, 10000]) {
			expect(policy.nextRetryDelayInMilliseconds(retryContext(count))).to.equal(30000);
		}
	});
});
