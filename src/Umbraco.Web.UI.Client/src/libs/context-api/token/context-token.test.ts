import { expect } from '@open-wc/testing';
import { UmbContextConsumer } from '../consume/context-consumer';
import { UmbContextProvider } from '../provide/context-provider';
import { UmbContextToken } from './context-token';

const testContextAlias = 'my-test-context';

class UmbTestContextTokenClass {
	prop = 'value from provider';
}

describe('ContextAlias', () => {
	const contextAlias = new UmbContextToken<UmbTestContextTokenClass>(testContextAlias);
	const typedProvider = new UmbContextProvider(document.body, contextAlias, new UmbTestContextTokenClass());
	typedProvider.hostConnected();

	after(() => {
		typedProvider.hostDisconnected();
	});

	it('toString returns the alias', () => {
		expect(contextAlias.toString()).to.eq(testContextAlias);
	});

	it('can be consumed directly', (done) => {
		const element = document.createElement('div');
		document.body.appendChild(element);

		const localConsumer = new UmbContextConsumer(element, contextAlias, (_instance) => {
			expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
			expect(_instance.prop).to.eq('value from provider');
			done();
		});

		localConsumer.hostConnected();
	});

	it('can be consumed using the inner string alias', (done) => {
		const element = document.createElement('div');
		document.body.appendChild(element);

		const localConsumer = new UmbContextConsumer(element, testContextAlias, (_instance: UmbTestContextTokenClass) => {
			expect(_instance).to.be.instanceOf(UmbTestContextTokenClass);
			expect(_instance.prop).to.eq('value from provider');
			done();
		});

		localConsumer.hostConnected();
	});
});
