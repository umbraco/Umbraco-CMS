import { expect } from '@open-wc/testing';
import { UmbContextConsumer } from './consume/context-consumer';
import { UmbContextAlias } from './context-alias';
import { UmbContextProvider } from './provide/context-provider';

const testContextAlias = 'my-test-context';

class MyClass {
	prop = 'value from provider';
}

describe('ContextAlias', () => {
	const contextAlias = new UmbContextAlias<MyClass>(testContextAlias);
	const typedProvider = new UmbContextProvider(document.body, contextAlias, new MyClass());
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
			console.log('got instance', _instance);
			expect(_instance).to.be.instanceOf(MyClass);
			expect(_instance.prop).to.eq('value from provider');
			done();
		});

		localConsumer.hostConnected();
	});

	it('can be consumed using the inner string alias', (done) => {
		const element = document.createElement('div');
		document.body.appendChild(element);

		const localConsumer = new UmbContextConsumer(element, testContextAlias, (_instance: MyClass) => {
			expect(_instance).to.be.instanceOf(MyClass);
			expect(_instance.prop).to.eq('value from provider');
			done();
		});

		localConsumer.hostConnected();
	});
});
