import { UmbContextConsumer } from '../consume/context-consumer.js';
import { UmbContextRequestEventImplementation } from '../consume/context-request.event.js';
import { UmbContextProvider } from './context-provider.js';
import { expect } from '@open-wc/testing';

class UmbTestContextProviderClass {
	prop = 'value from provider';
	getHostElement() {
		return undefined as unknown as Element;
	}
}

describe('UmbContextProvider', () => {
	let instance: UmbTestContextProviderClass;
	let provider: UmbContextProvider;

	beforeEach(() => {
		instance = new UmbTestContextProviderClass();
		provider = new UmbContextProvider(document.body, 'my-test-context', instance);
		provider.hostConnected();
	});

	afterEach(async () => {
		provider.hostDisconnected();
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has an attach method', () => {
				expect(provider).to.have.property('hostConnected').that.is.a('function');
			});

			it('has a detach method', () => {
				expect(provider).to.have.property('hostDisconnected').that.is.a('function');
			});
		});
	});

	it('handles context request events', (done) => {
		const event = new UmbContextRequestEventImplementation(
			'my-test-context',
			'default',
			(_instance: UmbTestContextProviderClass) => {
				expect(_instance.prop).to.eq('value from provider');
				done();
				return true;
			},
		);

		document.body.dispatchEvent(event);
	});

	it('works with UmbContextConsumer', (done) => {
		const element = document.createElement('div');
		document.body.appendChild(element);

		let callbackCount = 0;

		const localConsumer = new UmbContextConsumer(
			element,
			'my-test-context',
			(_instance: UmbTestContextProviderClass | undefined) => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(_instance?.prop).to.eq('value from provider');
					done();
					localConsumer.hostDisconnected();
				}
			},
		);
		localConsumer.hostConnected();
	});
});
