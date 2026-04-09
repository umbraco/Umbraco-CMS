import { UmbContextProvider } from '../provide/context-provider.js';
import { UmbContextToken } from '../token/context-token.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextConsumer } from './context-consumer.js';
import { UmbContextRequestEventImplementation } from './context-request.event.js';
import { UMB_CONTEXT_REQUEST_EVENT_TYPE } from './context-request.event.js';
import { assert, expect, oneEvent } from '@open-wc/testing';

const testContextAlias = 'my-test-context';
const testContextAliasAndApiAlias = 'my-test-context#testApi';
const testContextAliasAndNotExistingApiAlias = 'my-test-context#notExistingTestApi';

class UmbTestContextConsumerClass {
	public prop: string = 'value from provider';
	getHostElement() {
		return document.body;
	}
}

class UmbTestAlternativeContextConsumerClass {
	public alternativeProp: string = 'value from alternative provider';
	getHostElement() {
		return document.body;
	}
}

describe('UmbContextConsumer', () => {
	describe('Public API', () => {
		let consumer: UmbContextConsumer;

		beforeEach(() => {
			consumer = new UmbContextConsumer(document.body, testContextAlias, () => {});
		});

		describe('properties', () => {
			it('has a instance property', () => {
				expect(consumer).to.have.property('instance').that.is.undefined;
			});
		});
		describe('methods', () => {
			it('has a request method', () => {
				expect(consumer).to.have.property('request').that.is.a('function');
			});
		});

		describe('events', () => {
			it('dispatches context request event when constructed', async () => {
				const listener = oneEvent(window, UMB_CONTEXT_REQUEST_EVENT_TYPE);

				consumer.hostConnected();

				const event = (await listener) as unknown as UmbContextRequestEventImplementation;
				expect(event).to.exist;
				expect(event.type).to.eq(UMB_CONTEXT_REQUEST_EVENT_TYPE);
				expect(event.contextAlias).to.eq(testContextAlias);
				consumer.hostDisconnected();
			});
		});
	});

	describe('Simple implementation', () => {
		let element: HTMLElement;
		beforeEach(() => {
			element = document.createElement('div');
			document.body.appendChild(element);
		});
		afterEach(() => {
			document.body.removeChild(element);
		});

		it('works with UmbContextProvider', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					if (_instance) {
						expect(_instance.prop).to.eq('value from provider');
						localConsumer.hostDisconnected();
						provider.hostDisconnected();
						done();
					}
				},
			);
			localConsumer.hostConnected();
		});

		it('works with asPromise for UmbContextProvider', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());

			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(element, testContextAlias);
			localConsumer.hostConnected();
			localConsumer
				.asPromise()
				.then((instance) => {
					expect(instance?.prop).to.eq('value from provider');
					localConsumer.hostDisconnected();
					provider.hostDisconnected();
					done();
				})
				.catch(() => {
					expect.fail('Promise should not reject');
				});

			provider.hostConnected();
		});

		it('auto destroys when no callback provided', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());

			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(element, testContextAlias);
			expect((localConsumer as any)._retrieveHost).to.not.be.undefined;
			localConsumer.hostConnected();
			provider.hostConnected();
			const instance = await localConsumer.asPromise().catch(() => {
				expect.fail('Promise should not reject');
			});
			expect(instance?.prop).to.eq('value from provider');
			provider.hostDisconnected();

			await Promise.resolve();
			expect((localConsumer as any)._retrieveHost).to.be.undefined;
		});

		it('gets rejected when using asPromise that does not resolve', (done) => {
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(element, testContextAlias);

			localConsumer
				.asPromise()
				.then((instance) => {
					expect.fail('Promise should reject');
				})
				.catch(() => {
					localConsumer.hostDisconnected();
					localConsumer.destroy();
					done();
				});
			localConsumer.hostConnected();
		});

		it('never gets rejected when using asPromise that is set to prevent timeout and never will resolve', (done) => {
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(element, testContextAlias);
			localConsumer.hostConnected();

			let acceptedRejection = false;

			const timeout = setTimeout(() => {
				acceptedRejection = true;
				localConsumer.hostDisconnected();
			}, 100);

			localConsumer
				.asPromise({ preventTimeout: true })
				.then((instance) => {
					clearTimeout(timeout);
					expect.fail('Promise should not resolve');
				})
				.catch((e) => {
					clearTimeout(timeout);
					if (acceptedRejection === true) {
						done();
					} else {
						expect.fail('Promise should not reject');
					}
				});
		});

		it('works with host as a method', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const localConsumer = new UmbContextConsumer(
				() => element,
				testContextAlias,
				(_instance: UmbTestContextConsumerClass | undefined) => {
					if (_instance) {
						expect(_instance.prop).to.eq('value from provider');
						localConsumer.hostDisconnected();
						provider.hostDisconnected();
						done();
					}
				},
			);
			localConsumer.hostConnected();
		});

		it('works with host method returning undefined', async () => {
			const notExistingElement = undefined as unknown as Element;

			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				() => notExistingElement,
				testContextAlias,
				(_instance) => {
					if (_instance) {
						expect.fail('Callback should not be called when never permitted');
					}
				},
			);
			localConsumer.hostConnected();

			await Promise.resolve();

			localConsumer.hostDisconnected();
		});

		it('acts to Context API disconnected', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			let callbackNum = 0;

			const localConsumer = new UmbContextConsumer(
				element,
				testContextAlias,
				(_instance: UmbTestContextConsumerClass | undefined) => {
					callbackNum++;
					if (callbackNum === 1) {
						expect(_instance?.prop).to.eq('value from provider');
						// unregister.
						provider.hostDisconnected();
					} else if (callbackNum === 2) {
						expect(_instance?.prop).to.be.undefined;
						done();
					}
				},
			);
			localConsumer.hostConnected();
		});
	});

	describe('Implementation with Api Alias', () => {
		let element: HTMLElement;
		beforeEach(() => {
			element = document.createElement('div');
			document.body.appendChild(element);
		});
		afterEach(() => {
			document.body.removeChild(element);
		});

		it('responds when api alias matches', (done) => {
			const provider = new UmbContextProvider(
				document.body,
				testContextAliasAndApiAlias,
				new UmbTestContextConsumerClass(),
			);
			provider.hostConnected();

			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAliasAndApiAlias,
				(_instance) => {
					if (_instance) {
						expect(_instance.prop).to.eq('value from provider');
						localConsumer.hostDisconnected();
						provider.hostDisconnected();
						done();
					}
				},
			);
			localConsumer.hostConnected();
		});

		it('does not respond to a non existing api alias', async () => {
			const provider = new UmbContextProvider(
				document.body,
				testContextAliasAndApiAlias,
				new UmbTestContextConsumerClass(),
			);
			provider.hostConnected();

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(element, testContextAliasAndNotExistingApiAlias, (context) => {
				assert.fail('Callback should not be called more than once');
			});
			const requestEvent = oneEvent(localConsumer.getHostElement(), UMB_CONTEXT_REQUEST_EVENT_TYPE);
			localConsumer.hostConnected();

			await requestEvent;
			await Promise.resolve();

			// Delayed check to make sure the callback is not called.

			expect(callbackCount).to.equal(0, 'Callback should never have been called');
			localConsumer.hostDisconnected();
			provider.hostDisconnected();
		});
	});

	describe('Deferred disconnect lifecycle', () => {
		let element: HTMLElement;
		beforeEach(() => {
			element = document.createElement('div');
			document.body.appendChild(element);
		});
		afterEach(() => {
			document.body.removeChild(element);
		});

		it('forgets received instance one JS cycle after hostDisconnected', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let receivedInstance: UmbTestContextConsumerClass | undefined;
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					receivedInstance = _instance;
				},
			);
			localConsumer.hostConnected();

			// Instance should be set immediately after connecting
			expect(receivedInstance).to.not.be.undefined;
			expect(receivedInstance?.prop).to.eq('value from provider');

			// Disconnect the consumer
			localConsumer.hostDisconnected();

			// Instance should still be set synchronously (deferred cleanup hasn't run yet)
			expect(localConsumer.instance).to.not.be.undefined;

			// After one microtask, the instance should be cleared
			await Promise.resolve();
			expect(localConsumer.instance).to.be.undefined;
			expect(receivedInstance).to.be.undefined;

			provider.hostDisconnected();
		});

		it('preserves instance when reconnected before deferred disconnect runs', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let callbackCount = 0;
			let lastInstance: UmbTestContextConsumerClass | undefined;
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					callbackCount++;
					lastInstance = _instance;
				},
			);
			localConsumer.hostConnected();

			expect(callbackCount).to.eq(1);
			expect(lastInstance?.prop).to.eq('value from provider');

			// Disconnect and immediately reconnect (simulating a DOM move)
			localConsumer.hostDisconnected();
			localConsumer.hostConnected();

			// Wait for the deferred disconnect to have run (it should have been cancelled)
			await Promise.resolve();

			// Instance should still be set — the deferred disconnect was cancelled
			expect(localConsumer.instance).to.not.be.undefined;
			expect(localConsumer.instance?.prop).to.eq('value from provider');

			localConsumer.hostDisconnected();
			await Promise.resolve();
			provider.hostDisconnected();
		});

		it('rejects pending promise after deferred disconnect completes', async () => {
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(element, testContextAlias);
			localConsumer.hostConnected();

			const promise = localConsumer.asPromise({ preventTimeout: true });

			localConsumer.hostDisconnected();

			// After one microtask, the deferred disconnect should reject the promise
			await promise.then(
				() => {
					expect.fail('Promise should not resolve');
				},
				(reason) => {
					expect(reason).to.be.a('string');
					expect(reason).to.include('host was disconnected');
				},
			);
		});

		it('survives multiple rapid disconnect/reconnect cycles', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(element, testContextAlias, () => {});
			localConsumer.hostConnected();

			expect(localConsumer.instance?.prop).to.eq('value from provider');

			// Rapid disconnect/reconnect cycles
			localConsumer.hostDisconnected();
			localConsumer.hostConnected();
			localConsumer.hostDisconnected();
			localConsumer.hostConnected();
			localConsumer.hostDisconnected();
			localConsumer.hostConnected();

			// Let all pending microtasks resolve
			await Promise.resolve();

			// Instance should still be intact after all the thrashing
			expect(localConsumer.instance).to.not.be.undefined;
			expect(localConsumer.instance?.prop).to.eq('value from provider');

			localConsumer.hostDisconnected();
			await Promise.resolve();
			provider.hostDisconnected();
		});

		it('does not call unprovide callback twice when hostDisconnected is called twice', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let undefinedCallCount = 0;
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					if (_instance === undefined) {
						undefinedCallCount++;
					}
				},
			);
			localConsumer.hostConnected();

			// Call hostDisconnected twice without reconnecting
			localConsumer.hostDisconnected();
			localConsumer.hostDisconnected();

			// Let deferred disconnects resolve
			await Promise.resolve();

			// Should only have received undefined once, not twice
			expect(undefinedCallCount).to.eq(1);

			provider.hostDisconnected();
		});

		it('handles provider disconnecting while consumer deferred disconnect is pending', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let lastInstance: UmbTestContextConsumerClass | undefined;
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					lastInstance = _instance;
				},
			);
			localConsumer.hostConnected();

			expect(lastInstance?.prop).to.eq('value from provider');

			// Both disconnect in the same synchronous block
			localConsumer.hostDisconnected();
			provider.hostDisconnected();

			// Let microtasks resolve
			await Promise.resolve();

			expect(localConsumer.instance).to.be.undefined;
		});

		it('destroy cancels pending deferred disconnect', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let undefinedCallCount = 0;
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					if (_instance === undefined) {
						undefinedCallCount++;
					}
				},
			);
			localConsumer.hostConnected();

			// Disconnect then immediately destroy
			localConsumer.hostDisconnected();
			localConsumer.destroy();

			const countAfterDestroy = undefinedCallCount;

			// Let deferred disconnect microtask resolve
			await Promise.resolve();

			// The deferred disconnect should not have fired a second callback after destroy
			expect(undefinedCallCount).to.eq(countAfterDestroy);

			provider.hostDisconnected();
		});

		it('reconnect picks up a new provider after disconnect', async () => {
			const providerA = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			providerA.hostConnected();

			let lastInstance: UmbTestContextConsumerClass | UmbTestAlternativeContextConsumerClass | undefined;
			const localConsumer = new UmbContextConsumer(
				element,
				testContextAlias,
				(_instance: UmbTestContextConsumerClass | UmbTestAlternativeContextConsumerClass | undefined) => {
					lastInstance = _instance;
				},
			);
			localConsumer.hostConnected();

			expect((lastInstance as UmbTestContextConsumerClass)?.prop).to.eq('value from provider');

			// Disconnect consumer and let deferred disconnect complete
			localConsumer.hostDisconnected();
			await Promise.resolve();

			expect(lastInstance).to.be.undefined;

			// Swap providers while consumer is disconnected
			providerA.hostDisconnected();
			const providerB = new UmbContextProvider(
				document.body,
				testContextAlias,
				new UmbTestAlternativeContextConsumerClass(),
			);
			providerB.hostConnected();

			// Reconnect — should pick up providerB
			localConsumer.hostConnected();

			expect((lastInstance as UmbTestAlternativeContextConsumerClass)?.alternativeProp).to.eq(
				'value from alternative provider',
			);

			localConsumer.hostDisconnected();
			await Promise.resolve();
			providerB.hostDisconnected();
		});

		it('callback receives undefined then new instance when provider swaps', async () => {
			const instanceA = new UmbTestContextConsumerClass();
			const providerA = new UmbContextProvider(document.body, testContextAlias, instanceA);
			providerA.hostConnected();

			const receivedInstances: Array<UmbTestContextConsumerClass | undefined> = [];
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					receivedInstances.push(_instance);
				},
			);
			localConsumer.hostConnected();

			expect(receivedInstances).to.have.length(1);
			expect(receivedInstances[0]?.prop).to.eq('value from provider');

			// Provider A disconnects — consumer should get undefined via unprovide event
			providerA.hostDisconnected();

			expect(receivedInstances).to.have.length(2);
			expect(receivedInstances[1]).to.be.undefined;

			// Provider B connects — consumer should pick it up via provide event
			const instanceB = new UmbTestContextConsumerClass();
			instanceB.prop = 'value from provider B';
			const providerB = new UmbContextProvider(document.body, testContextAlias, instanceB);
			providerB.hostConnected();

			expect(receivedInstances).to.have.length(3);
			expect(receivedInstances[2]?.prop).to.eq('value from provider B');

			localConsumer.hostDisconnected();
			await Promise.resolve();
			providerB.hostDisconnected();
		});

		/**
		 * Regression test: after a full disconnect cycle (deferred disconnect completes),
		 * hostDisconnected() dismantles the scope listeners and assigns #currentScope = window
		 * directly (bypassing #setCurrentScope). On the next hostConnected(), #setCurrentScope(window)
		 * becomes a no-op because #currentScope already equals window, leaving the consumer without
		 * provide/unprovide listeners. A provider connecting after reconnect goes undetected.
		 */
		it('detects provider that connects after consumer reconnects following a full disconnect cycle', async () => {
			const providerA = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			providerA.hostConnected();

			let lastInstance: UmbTestContextConsumerClass | undefined;
			const localConsumer = new UmbContextConsumer<UmbTestContextConsumerClass>(
				element,
				testContextAlias,
				(_instance) => {
					lastInstance = _instance;
				},
			);
			localConsumer.hostConnected();

			// Consumer has context from provider A
			expect(lastInstance?.prop).to.eq('value from provider');

			// Full disconnect cycle: disconnect consumer AND provider, let deferred disconnect complete
			providerA.hostDisconnected();
			localConsumer.hostDisconnected();
			await Promise.resolve();

			expect(lastInstance).to.be.undefined;

			// Consumer reconnects — no provider available yet
			localConsumer.hostConnected();

			// request() runs synchronously during hostConnected but finds no provider
			expect(lastInstance).to.be.undefined;

			// Now a NEW provider appears AFTER the consumer has reconnected.
			// This relies on the provide event listener being active on window.
			const instanceB = new UmbTestContextConsumerClass();
			instanceB.prop = 'value from provider B';
			const providerB = new UmbContextProvider(document.body, testContextAlias, instanceB);
			providerB.hostConnected();

			// Consumer should detect provider B via the provide event and call request() again
			try {
				expect(lastInstance?.prop).to.eq('value from provider B');
			} finally {
				providerB.hostDisconnected();
				localConsumer.destroy();
			}
		});
	});

	describe('Implementation with discriminator method', () => {
		let element: HTMLElement;
		beforeEach(() => {
			element = document.createElement('div');
			document.body.appendChild(element);
		});
		afterEach(() => {
			document.body.removeChild(element);
		});

		interface A extends UmbContextMinimal {
			prop: string;
		}

		function discriminator(instance: unknown): instance is A {
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			return typeof (instance as any).prop === 'string';
		}

		function badDiscriminator(instance: unknown): instance is A {
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			return typeof (instance as any).notExistingProp === 'string';
		}

		it('discriminator determines the instance type', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(instance: A | undefined) => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(instance?.prop).to.eq('value from provider');
						provider.destroy();
						localConsumer.destroy();
						done();
					}
				},
			);
			localConsumer.hostConnected();
			provider.hostConnected();

			// This bit of code is not really a test but it serves as a TypeScript type test, making sure the given type is matches the one given from the Discriminator method.
			type TestType = Exclude<typeof localConsumer.instance, undefined> extends A ? true : never;
			const test: TestType = true;
			expect(test).to.be.true;
		});

		it('approving discriminator still fires callback', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(_instance) => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(_instance?.prop).to.eq('value from provider');
						localConsumer.hostDisconnected();
						provider.hostDisconnected();
						done();
					}
				},
			);
			localConsumer.hostConnected();
		});

		it('disapproving discriminator does not fire callback', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, badDiscriminator),
				(_instance) => {
					callbackCount++;
					assert.fail('Callback should not be called more than once');
				},
			);
			const requestEvent = oneEvent(localConsumer.getHostElement(), UMB_CONTEXT_REQUEST_EVENT_TYPE);
			localConsumer.hostConnected();

			// Wait for to ensure the above request didn't succeed:
			await requestEvent;
			await Promise.resolve();
			expect(callbackCount).to.equal(0, 'Callback should never have been called');
			localConsumer.hostDisconnected();
			provider.hostDisconnected();
		});

		it('context api of same context alias will prevent request from propagating', async () => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const alternativeProvider = new UmbContextProvider(
				element,
				testContextAlias,
				new UmbTestAlternativeContextConsumerClass(),
			);
			alternativeProvider.hostConnected();

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(_instance) => {
					callbackCount++;
				},
			);
			const requestEvent = oneEvent(localConsumer.getHostElement(), UMB_CONTEXT_REQUEST_EVENT_TYPE);
			localConsumer.hostConnected();

			await requestEvent;
			await Promise.resolve();
			// Wait for to ensure the above request didn't succeed:

			expect(callbackCount).to.equal(0, 'Callback should never have been called');
			localConsumer.hostDisconnected();
			provider.hostDisconnected();
		});

		it('context api of same context alias will NOT prevent request from propagating when set to passContextAliasMatches', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const alternativeProvider = new UmbContextProvider(
				element,
				testContextAlias,
				new UmbTestAlternativeContextConsumerClass(),
			);
			alternativeProvider.hostConnected();

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(_instance) => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(_instance?.prop).to.eq('value from provider');
						localConsumer.hostDisconnected();
						provider.hostDisconnected();
					} else {
						expect(_instance).to.be.undefined;
						done();
					}
				},
			);
			localConsumer.passContextAliasMatches();
			localConsumer.hostConnected();
		});
	});
});
