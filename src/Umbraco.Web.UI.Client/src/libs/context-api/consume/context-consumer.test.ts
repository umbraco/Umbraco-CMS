import { UmbContextProvider } from '../provide/context-provider.js';
import { UmbContextToken } from '../token/context-token.js';
import { UmbContextConsumer } from './context-consumer.js';
import type { UmbContextRequestEventImplementation } from './context-request.event.js';
import { UMB_CONTEXT_REQUEST_EVENT_TYPE } from './context-request.event.js';
import { expect, oneEvent } from '@open-wc/testing';

const testContextAlias = 'my-test-context';
const testContextAliasAndApiAlias = 'my-test-context#testApi';
const testContextAliasAndNotExistingApiAlias = 'my-test-context#notExistingTestApi';

class UmbTestContextConsumerClass {
	public prop: string = 'value from provider';
}

class UmbTestAlternativeContextConsumerClass {
	public alternativeProp: string = 'value from alternative provider';
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
		it('works with UmbContextProvider', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				testContextAlias,
				(_instance: UmbTestContextConsumerClass | undefined) => {
					if (_instance) {
						expect(_instance.prop).to.eq('value from provider');
						done();
						localConsumer.hostDisconnected();
						provider.hostDisconnected();
					}
				},
			);
			localConsumer.hostConnected();
		});

		it('works with host as a method', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				() => element,
				testContextAlias,
				(_instance: UmbTestContextConsumerClass | undefined) => {
					if (_instance) {
						expect(_instance.prop).to.eq('value from provider');
						done();
						localConsumer.hostDisconnected();
						provider.hostDisconnected();
					}
				},
			);
			localConsumer.hostConnected();
		});

		it('works with host method returning undefined', async () => {
			const element = undefined;

			const localConsumer = new UmbContextConsumer(
				() => element,
				testContextAlias,
				(_instance: UmbTestContextConsumerClass | undefined) => {
					if (_instance) {
						expect.fail('Callback should not be called when never permitted');
					}
				},
			);
			localConsumer.hostConnected();

			await Promise.resolve();

			localConsumer.hostDisconnected();
		});

		/*
		Unprovided feature is out commented currently. I'm not sure there is a use case. So lets leave the code around until we know for sure.
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
				}
			);
			localConsumer.hostConnected();
		});
		*/
	});

	describe('Implementation with Api Alias', () => {
		it('responds when api alias matches', (done) => {
			const provider = new UmbContextProvider(
				document.body,
				testContextAliasAndApiAlias,
				new UmbTestContextConsumerClass(),
			);
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(element, testContextAliasAndApiAlias, (_instance) => {
				if (_instance) {
					expect((_instance as UmbTestContextConsumerClass).prop).to.eq('value from provider');
					localConsumer.hostDisconnected();
					provider.hostDisconnected();
					done();
				}
			});
			localConsumer.hostConnected();
		});

		it('does not respond to a non existing api alias', (done) => {
			const provider = new UmbContextProvider(
				document.body,
				testContextAliasAndApiAlias,
				new UmbTestContextConsumerClass(),
			);
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(element, testContextAliasAndNotExistingApiAlias, () => {
				expect(false).to.be.true;
			});
			localConsumer.hostConnected();

			// Delayed check to make sure the callback is not called.
			Promise.resolve().then(() => {
				localConsumer.hostDisconnected();
				provider.hostDisconnected();
				done();
			});
		});
	});

	describe('Implementation with discriminator method', () => {
		type A = { prop: string };

		function discriminator(instance: unknown): instance is A {
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			return typeof (instance as any).prop === 'string';
		}

		function badDiscriminator(instance: unknown): instance is A {
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			return typeof (instance as any).notExistingProp === 'string';
		}

		it('discriminator determines the instance type', (done) => {
			const localConsumer = new UmbContextConsumer(
				document.body,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(instance: A) => {
					expect(instance.prop).to.eq('value from provider');
					done();
					localConsumer.destroy();
				},
			);
			localConsumer.hostConnected();

			// This bit of code is not really a test but it serves as a TypeScript type test, making sure the given type is matches the one given from the Discriminator method.
			type TestType = Exclude<typeof localConsumer.instance, undefined> extends A ? true : never;
			const test: TestType = true;
			expect(test).to.be.true;
		});

		it('approving discriminator still fires callback', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(_instance) => {
					expect(_instance.prop).to.eq('value from provider');
					done();
					localConsumer.hostDisconnected();
					provider.hostDisconnected();
				},
			);
			localConsumer.hostConnected();
		});

		it('disapproving discriminator does not fire callback', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, badDiscriminator),
				(_instance) => {
					expect(_instance.prop).to.eq('this must not happen!');
				},
			);
			localConsumer.hostConnected();

			// Wait for to ensure the above request didn't succeed:
			Promise.resolve().then(() => {
				done();
				localConsumer.hostDisconnected();
				provider.hostDisconnected();
			});
		});

		it('context api of same context alias will prevent request from propagating', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const alternativeProvider = new UmbContextProvider(
				element,
				testContextAlias,
				new UmbTestAlternativeContextConsumerClass(),
			);
			alternativeProvider.hostConnected();

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(_instance) => {
					expect(_instance.prop).to.eq('this must not happen!');
				},
			);
			localConsumer.hostConnected();

			// Wait for to ensure the above request didn't succeed:
			Promise.resolve().then(() => {
				done();
				localConsumer.hostDisconnected();
				provider.hostDisconnected();
			});
		});

		it('context api of same context alias will NOT prevent request from propagating when set to exactMatch', (done) => {
			const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
			provider.hostConnected();

			const element = document.createElement('div');
			document.body.appendChild(element);

			const alternativeProvider = new UmbContextProvider(
				element,
				testContextAlias,
				new UmbTestAlternativeContextConsumerClass(),
			);
			alternativeProvider.hostConnected();

			const localConsumer = new UmbContextConsumer(
				element,
				new UmbContextToken(testContextAlias, undefined, discriminator),
				(_instance) => {
					expect(_instance.prop).to.eq('value from provider');
					done();
					localConsumer.hostDisconnected();
					provider.hostDisconnected();
				},
			);
			localConsumer.passContextAliasMatches();
			localConsumer.hostConnected();
		});
	});
});
