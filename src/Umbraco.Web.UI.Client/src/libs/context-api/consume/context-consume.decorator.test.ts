import { UmbContextToken } from '../token/context-token.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextProvider } from '../provide/context-provider.js';
import { consumeContext } from './context-consume.decorator.js';
import { aTimeout, elementUpdated, expect, fixture } from '@open-wc/testing';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { html, state } from '@umbraco-cms/backoffice/external/lit';

class UmbTestContextConsumerClass implements UmbContextMinimal {
	public prop: string = 'value from provider';
	getHostElement() {
		return undefined as any;
	}
}

const testToken = new UmbContextToken<UmbTestContextConsumerClass>('my-test-context');

class MyTestElement extends UmbLitElement {
	@consumeContext({
		context: testToken,
	})
	@state()
	contextValue?: UmbTestContextConsumerClass;

	override render() {
		return html`<div>${this.contextValue?.prop ?? 'no context'}</div>`;
	}
}

customElements.define('my-consume-test-element', MyTestElement);

describe('@consume decorator', () => {
	let provider: UmbContextProvider;
	let element: MyTestElement;

	beforeEach(async () => {
		provider = new UmbContextProvider(document.body, testToken, new UmbTestContextConsumerClass());
		provider.hostConnected();

		element = await fixture<MyTestElement>(`<my-consume-test-element></my-consume-test-element>`);
	});

	afterEach(() => {
		provider.destroy();
		(provider as any) = undefined;
	});

	it('should receive a context value when provided on the host', () => {
		expect(element.contextValue).to.equal(provider.providerInstance());
		expect(element.contextValue?.prop).to.equal('value from provider');
	});

	it('should render the value from the context', async () => {
		expect(element).shadowDom.to.equal('<div>value from provider</div>');
	});

	it('should work when the decorator is used in a controller', async () => {
		class MyController extends UmbControllerBase {
			@consumeContext({ context: testToken })
			contextValue?: UmbTestContextConsumerClass;
		}

		const controller = new MyController(element);

		await elementUpdated(element);

		expect(element.contextValue).to.equal(provider.providerInstance());
		expect(controller.contextValue).to.equal(provider.providerInstance());
	});

	it('should have called the callback first', async () => {
		let callbackCalled = false;

		class MyCallbackTestElement extends UmbLitElement {
			@consumeContext({
				context: testToken,
				callback: () => {
					callbackCalled = true;
				},
			})
			contextValue?: UmbTestContextConsumerClass;
		}

		customElements.define('my-callback-consume-test-element', MyCallbackTestElement);

		const callbackElement = await fixture<MyCallbackTestElement>(
			`<my-callback-consume-test-element></my-callback-consume-test-element>`,
		);

		await elementUpdated(callbackElement);

		expect(callbackCalled).to.be.true;
		expect(callbackElement.contextValue).to.equal(provider.providerInstance());
	});

	it('should update the context value when the provider instance changes', async () => {
		const newProviderInstance = new UmbTestContextConsumerClass();
		newProviderInstance.prop = 'new value from provider';

		const newProvider = new UmbContextProvider(element, testToken, newProviderInstance);
		newProvider.hostConnected();

		await elementUpdated(element);

		expect(element.contextValue).to.equal(newProvider.providerInstance());
		expect(element.contextValue?.prop).to.equal(newProviderInstance.prop);
	});

	it('should be able to consume without subscribing', async () => {
		class MyNoSubscribeTestController extends UmbControllerBase {
			@consumeContext({ context: testToken, subscribe: false })
			contextValue?: UmbTestContextConsumerClass;
		}

		const controller = new MyNoSubscribeTestController(element);
		await aTimeout(0); // Wait a tick for promise to resolve

		expect(controller.contextValue).to.equal(provider.providerInstance());

		const newProviderInstance = new UmbTestContextConsumerClass();
		newProviderInstance.prop = 'new value from provider';

		const newProvider = new UmbContextProvider(element, testToken, newProviderInstance);
		newProvider.hostConnected();

		await aTimeout(0); // Wait a tick for promise to resolve

		// Should still be the old value
		expect(controller.contextValue).to.not.equal(newProvider.providerInstance());
		expect(controller.contextValue?.prop).to.equal('value from provider');
	});
});
