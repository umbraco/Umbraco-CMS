import { UmbCancelablePromise } from '../cancelable-promise.js';
import { UmbCancelError } from '../umb-error.js';
import type { UmbApiResponse } from '../types.js';
import { tryExecute } from './tryExecute.function.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

@customElement('test-try-execute-host')
class UmbTestTryExecuteHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const PENDING = Symbol('pending');

/**
 * Resolves with the settled value, or with PENDING if the promise has not settled within the given time.
 * @param {Promise<T>} promise The promise to wait for.
 * @param {number} ms How long to wait.
 * @returns {Promise<Awaited<T> | typeof PENDING>} The settled value, or PENDING.
 */
async function settleWithin<T>(promise: Promise<T>, ms = 100): Promise<Awaited<T> | typeof PENDING> {
	return Promise.race([promise, aTimeout(ms).then((): typeof PENDING => PENDING)]);
}

/**
 * A promise that never settles, like a request that is still in flight.
 * @returns {Promise<unknown>} A pending promise.
 */
function pendingRequest(): Promise<unknown> {
	return new Promise(() => {});
}

describe('tryExecute', () => {
	let hostElement: UmbTestTryExecuteHostElement;
	let notificationCount: number;
	let originalConsoleError: typeof console.error;

	/**
	 * Gives a failed request time to raise its notification, which happens asynchronously.
	 * @returns {Promise<number>} The number of notifications shown so far.
	 */
	async function notificationsShown(): Promise<number> {
		await aTimeout(50);
		return notificationCount;
	}

	beforeEach(() => {
		hostElement = new UmbTestTryExecuteHostElement();
		document.body.appendChild(hostElement);
		notificationCount = 0;
		new UmbNotificationContext(hostElement).notifications.subscribe((notifications) => {
			notificationCount = notifications.length;
		});
		// A failed request logs the error, which is expected noise in these tests
		originalConsoleError = console.error;
		console.error = () => {};
	});

	afterEach(() => {
		console.error = originalConsoleError;
		hostElement.remove();
	});

	it('shows a notification when the request fails', async () => {
		const { error } = await tryExecute(hostElement, Promise.reject(new Error('Boom')));

		expect(error).to.exist;
		expect(await notificationsShown()).to.equal(1);
	});

	describe('abortSignal', () => {
		it('resolves with the response when the signal does not abort', async () => {
			const abortController = new AbortController();

			const result = await tryExecute(hostElement, Promise.resolve({ data: 'value' }), {
				abortSignal: abortController.signal,
			});

			expect(result.data).to.equal('value');
			expect(result.error).to.be.undefined;
		});

		it('resolves with an UmbCancelError when the signal aborts while the request is pending', async () => {
			const abortController = new AbortController();
			const execution = tryExecute(hostElement, pendingRequest(), { abortSignal: abortController.signal });

			abortController.abort();
			const result = await settleWithin(execution);

			expect(result).to.not.equal(PENDING);
			expect(UmbCancelError.isUmbCancelError((result as UmbApiResponse).error)).to.be.true;
		});

		it('resolves with an UmbCancelError when the signal has already aborted', async () => {
			const result = await settleWithin(
				tryExecute(hostElement, pendingRequest(), { abortSignal: AbortSignal.abort() }),
			);

			expect(result).to.not.equal(PENDING);
			expect(UmbCancelError.isUmbCancelError((result as UmbApiResponse).error)).to.be.true;
		});

		it('does not show a notification when the request fails after the signal aborted', async () => {
			const abortController = new AbortController();
			let rejectRequest!: (reason: unknown) => void;
			const request = new Promise<unknown>((_, reject) => (rejectRequest = reject));
			const execution = tryExecute(hostElement, request, { abortSignal: abortController.signal });

			abortController.abort();
			rejectRequest(new Error('The request failed after it was aborted'));
			const { error } = await execution;

			expect(UmbCancelError.isUmbCancelError(error)).to.be.true;
			expect(await notificationsShown()).to.equal(0);
		});

		it('cancels a cancelable request as soon as the signal aborts', async () => {
			const abortController = new AbortController();
			let cancelled = 0;
			const request = new UmbCancelablePromise<never>((_resolve, _reject, onCancel) => onCancel(() => cancelled++));
			const execution = tryExecute(hostElement, request, { abortSignal: abortController.signal });

			abortController.abort();

			expect(cancelled).to.equal(1);
			await execution;
		});

		it('cancels a cancelable request straight away when the signal has already aborted', async () => {
			let cancelled = 0;
			const request = new UmbCancelablePromise<never>((_resolve, _reject, onCancel) => onCancel(() => cancelled++));

			const execution = tryExecute(hostElement, request, { abortSignal: AbortSignal.abort() });

			expect(cancelled).to.equal(1);
			await settleWithin(execution);
		});

		it('stops listening to the signal once the request has settled', async () => {
			const abortController = new AbortController();
			let cancelled = 0;
			const request = Object.assign(Promise.resolve({ data: 'value' }), { cancel: () => cancelled++ });
			await tryExecute(hostElement, request, { abortSignal: abortController.signal });
			const cancelledBeforeAbort = cancelled;

			abortController.abort();

			expect(cancelled).to.equal(cancelledBeforeAbort);
		});
	});

	describe('a request given its own signal', () => {
		it('cancels the umbHttpClient request without a notification', async () => {
			const abortController = new AbortController();
			let requestSignal: AbortSignal | undefined;
			const fetchUntilAborted: typeof fetch = (input) => {
				const signal = (input as Request).signal;
				requestSignal = signal;
				return new Promise((_, reject) => {
					if (signal.aborted) reject(signal.reason);
					signal.addEventListener('abort', () => reject(signal.reason), { once: true });
				});
			};
			const execution = tryExecute(
				hostElement,
				umbHttpClient.get({
					url: '/umbraco/management/api/v1/server/status',
					signal: abortController.signal,
					fetch: fetchUntilAborted,
				}),
			);
			await aTimeout(0);

			abortController.abort();
			const { error } = await execution;

			expect(requestSignal?.aborted).to.be.true;
			expect(UmbCancelError.isUmbCancelError(error)).to.be.true;
			expect(await notificationsShown()).to.equal(0);
		});
	});
});
