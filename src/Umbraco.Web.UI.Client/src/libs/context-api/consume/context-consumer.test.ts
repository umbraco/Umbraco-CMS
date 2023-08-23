import { expect, oneEvent } from '@open-wc/testing';
import { UmbContextProvider } from '../provide/context-provider.js';
import { UmbContextToken } from '../token/context-token.js';
import { UmbContextConsumer } from './context-consumer.js';
import { UmbContextRequestEventImplementation, umbContextRequestEventType } from './context-request.event.js';

const testContextAlias = 'my-test-context';

class UmbTestContextConsumerClass {
	public prop: string = 'value from provider';
}

describe('UmbContextConsumer', () => {
	let consumer: UmbContextConsumer;

	beforeEach(() => {
		// eslint-disable-next-line @typescript-eslint/no-empty-function
		consumer = new UmbContextConsumer(document.body, testContextAlias, () => {});
	});

	describe('Public API', () => {
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
				const listener = oneEvent(window, umbContextRequestEventType);

				consumer.hostConnected();

				const event = (await listener) as unknown as UmbContextRequestEventImplementation;
				expect(event).to.exist;
				expect(event.type).to.eq(umbContextRequestEventType);
				expect(event.contextAlias).to.eq(testContextAlias);
				consumer.hostDisconnected();
			});
		});
	});

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
			}
		);
		localConsumer.hostConnected();
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

describe('UmbContextConsumer with discriminator test', () => {

	type A = { prop: string };

	function discriminator(instance: unknown): instance is A {
		return typeof (instance as any).prop === 'string';
	}

	function badDiscriminator(instance: unknown): instance is A {
		return typeof (instance as any).notExistingProp === 'string';
	}

	it('discriminator determines the instance type', async () => {

		const localConsumer = new UmbContextConsumer(
			document.body,
			new UmbContextToken(testContextAlias, discriminator),
			(instance: A) => { console.log(instance)}
		);
		localConsumer.hostConnected();

		// This bit of code is not really a test but it serves as a TypeScript type test, making sure the given type is matches the one given from the Discriminator method.
		type TestType = Exclude<(typeof localConsumer.instance), undefined> extends A ? true : never;
		const test: TestType = true;
		expect(test).to.be.true;

		localConsumer.destroy();
	});

	it('approving discriminator still fires callback', (done) => {
		const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
		provider.hostConnected();

		const element = document.createElement('div');
		document.body.appendChild(element);

		const localConsumer = new UmbContextConsumer(
			element,
			new UmbContextToken(testContextAlias, discriminator),
			(_instance) => {
				expect(_instance.prop).to.eq('value from provider');
				done();
				localConsumer.hostDisconnected();
				provider.hostDisconnected();
			}
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
			new UmbContextToken(testContextAlias, badDiscriminator),
			(_instance) => {
				expect(_instance.prop).to.eq('this must not happen!');
			}
		);
		localConsumer.hostConnected();

		Promise.resolve().then(() => {
			done();
			localConsumer.hostDisconnected();
			provider.hostDisconnected();
		});
	});


});
