import { UmbContextConsumer } from '../consume/context-consumer.js';
import { UmbContextProvider } from '../provide/context-provider.js';
import { UmbContextToken } from './context-token.js';
import { expect } from '@open-wc/testing';

const testContextAlias = 'my-test-context';
const testApiAlias = 'my-test-api';

class UmbTestContextTokenClass {
	prop = 'value from provider';
	getHostElement() {
		return undefined as unknown as Element;
	}
}

describe('UmbContextToken', () => {
	describe('Simple context token', () => {
		const contextToken = new UmbContextToken<UmbTestContextTokenClass>(testContextAlias);
		const typedProvider = new UmbContextProvider(document.body, contextToken, new UmbTestContextTokenClass());
		typedProvider.hostConnected();

		after(() => {
			typedProvider.hostDisconnected();
		});

		it('toString returns the alias', () => {
			expect(contextToken.toString()).to.eq(testContextAlias + '#' + 'default');
		});

		it('can be used to consume a context API', (done) => {
			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				contextToken,
				(_instance: UmbTestContextTokenClass | undefined) => {
					expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
					expect(_instance?.prop).to.eq('value from provider');
					done();
					localConsumer.destroy(); // We do not want to react to when the provider is disconnected.
				},
			);

			localConsumer.hostConnected();
		});

		it('gives the same result when using the string alias', (done) => {
			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				testContextAlias,
				(_instance: UmbTestContextTokenClass | undefined) => {
					expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
					expect(_instance?.prop).to.eq('value from provider');
					done();
					localConsumer.destroy(); // We do not want to react to when the provider is disconnected.
				},
			);

			localConsumer.hostConnected();
		});
	});

	describe('Context Token with alternative api alias', () => {
		const contextToken = new UmbContextToken<UmbTestContextTokenClass>(testContextAlias, testApiAlias);
		const typedProvider = new UmbContextProvider(document.body, contextToken, new UmbTestContextTokenClass());
		typedProvider.hostConnected();

		after(() => {
			typedProvider.hostDisconnected();
		});

		it('toString returns the alias', () => {
			expect(contextToken.toString()).to.eq(testContextAlias + '#' + testApiAlias);
		});

		it('can be used to consume a context API', (done) => {
			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				contextToken,
				(_instance: UmbTestContextTokenClass | undefined) => {
					expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
					expect(_instance?.prop).to.eq('value from provider');
					done();
					localConsumer.destroy(); // We do not want to react to when the provider is disconnected.
				},
			);

			localConsumer.hostConnected();
		});

		it('gives the same result when using the string alias', (done) => {
			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				testContextAlias,
				(_instance: UmbTestContextTokenClass | undefined) => {
					expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
					expect(_instance?.prop).to.eq('value from provider');
					done();
					localConsumer.destroy(); // We do not want to react to when the provider is disconnected.
				},
			);

			localConsumer.hostConnected();
		});
	});

	describe('Context Token with discriminator method', () => {
		const contextToken = new UmbContextToken<UmbTestContextTokenClass>(
			testContextAlias,
			undefined,
			(instance): instance is UmbTestContextTokenClass => instance.prop === 'value from provider',
		);
		const typedProvider = new UmbContextProvider(document.body, contextToken, new UmbTestContextTokenClass());
		typedProvider.hostConnected();

		after(() => {
			typedProvider.hostDisconnected();
		});

		it('toString returns the alias', () => {
			expect(contextToken.toString()).to.eq(testContextAlias + '#' + 'default');
		});

		it('can be used to consume a context API', (done) => {
			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				contextToken,
				(_instance: UmbTestContextTokenClass | undefined) => {
					expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
					expect(_instance?.prop).to.eq('value from provider');
					done();
					localConsumer.destroy(); // We do not want to react to when the provider is disconnected.
				},
			);

			localConsumer.hostConnected();
		});

		it('gives the same result when using the string alias', (done) => {
			const element = document.createElement('div');
			document.body.appendChild(element);

			const localConsumer = new UmbContextConsumer(
				element,
				testContextAlias,
				(_instance: UmbTestContextTokenClass | undefined) => {
					expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
					expect(_instance?.prop).to.eq('value from provider');
					done();
					localConsumer.destroy(); // We do not want to react to when the provider is disconnected.
				},
			);

			localConsumer.hostConnected();
		});
	});
});
