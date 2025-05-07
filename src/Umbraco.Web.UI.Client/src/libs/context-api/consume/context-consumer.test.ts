import { UmbContextProvider } from '../provide/context-provider.js';
import { UmbContextToken } from '../token/context-token.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextConsumer } from './context-consumer.js';
import type { UmbContextRequestEventImplementation } from './context-request.event.js';
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

		it('does not respond to a non existing api alias', (done) => {
			const provider = new UmbContextProvider(
				document.body,
				testContextAliasAndApiAlias,
				new UmbTestContextConsumerClass(),
			);
			provider.hostConnected();

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(element, testContextAliasAndNotExistingApiAlias, (context) => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(context).to.be.undefined;
					done();
				} else {
					assert.fail('Callback should not be called more than once');
				}
			});
			localConsumer.hostConnected();

			// Delayed check to make sure the callback is not called.
			Promise.resolve().then(() => {
				localConsumer.hostDisconnected();
				provider.hostDisconnected();
			});
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

		it('disapproving discriminator does not fire callback', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			let callbackCount = 0;

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, badDiscriminator),
				(_instance) => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(_instance).to.be.undefined;
						done();
					} else {
						assert.fail('Callback should not be called more than once');
					}
				},
			);
			localConsumer.hostConnected();

			// Wait for to ensure the above request didn't succeed:
			Promise.resolve().then(() => {
				localConsumer.hostDisconnected();
				provider.hostDisconnected();
			});
		});

		it('context api of same context alias will prevent request from propagating', (done) => {
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
						expect(_instance).to.be.undefined;
						done();
					}
				},
			);
			localConsumer.hostConnected();

			// Wait for to ensure the above request didn't succeed:
			Promise.resolve().then(() => {
				localConsumer.hostDisconnected();
				provider.hostDisconnected();
			});
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
