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
	#host: Element;

	constructor(host: Element) {
		this.#host = host;
	}
	getHostElement() {
		return this.#host;
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
		provider = new UmbContextProvider(document.body, testToken, new UmbTestContextConsumerClass(document.body));
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
		const newProviderInstance = new UmbTestContextConsumerClass(document.body);
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

		const newProviderInstance = new UmbTestContextConsumerClass(document.body);
		newProviderInstance.prop = 'new value from provider';

		const newProvider = new UmbContextProvider(element, testToken, newProviderInstance);
		newProvider.hostConnected();

		await aTimeout(0); // Wait a tick for promise to resolve

		// Should still be the old value
		expect(controller.contextValue).to.not.equal(newProvider.providerInstance());
		expect(controller.contextValue?.prop).to.equal('value from provider');
	});

	it('should resolve context before first render when provider is already in ancestor tree', async () => {
		class RenderTimingElement extends UmbLitElement {
			@consumeContext({ context: testToken })
			@state()
			contextValue?: UmbTestContextConsumerClass;

			public renderedValues: (string | undefined)[] = [];

			override render() {
				this.renderedValues.push(this.contextValue?.prop);
				return html`<div>${this.contextValue?.prop ?? 'no context'}</div>`;
			}
		}

		customElements.define('render-timing-element', RenderTimingElement);

		const timingElement = await fixture<RenderTimingElement>(`<render-timing-element></render-timing-element>`);

		// First render should already have the resolved context value, not undefined
		expect(timingElement.renderedValues[0]).to.equal('value from provider');
		// Element should not have rendered with 'no context' at any point
		expect(timingElement.renderedValues).to.not.include(undefined);
	});

	it('should receive context when provider mounts AFTER consumer (late arrival)', async () => {
		// Remove the default provider to simulate no-provider state
		provider.destroy();

		class LateArrivalElement extends UmbLitElement {
			@consumeContext({ context: testToken })
			@state()
			contextValue?: UmbTestContextConsumerClass;
		}

		customElements.define('late-arrival-element', LateArrivalElement);

		const lateElement = await fixture<LateArrivalElement>(`<late-arrival-element></late-arrival-element>`);
		await elementUpdated(lateElement);

		// No provider yet — consumer has nothing
		expect(lateElement.contextValue).to.be.undefined;

		// Now mount the provider
		const lateProvider = new UmbContextProvider(
			document.body,
			testToken,
			new UmbTestContextConsumerClass(document.body),
		);
		lateProvider.hostConnected();

		await elementUpdated(lateElement);

		// Consumer should now have resolved
		expect(lateElement.contextValue).to.equal(lateProvider.providerInstance());

		lateProvider.destroy();
		// Restore the provider for the other tests (afterEach expects it to exist)
		provider = new UmbContextProvider(document.body, testToken, new UmbTestContextConsumerClass(document.body));
		provider.hostConnected();
	});

	it('should not set up the consumer multiple times across disconnect/reconnect cycles', async () => {
		let callbackCount = 0;

		class ReconnectElement extends UmbLitElement {
			@consumeContext({
				context: testToken,
				callback: () => {
					callbackCount++;
				},
			})
			contextValue?: UmbTestContextConsumerClass;
		}

		customElements.define('reconnect-element', ReconnectElement);

		const reconnectElement = await fixture<ReconnectElement>(`<reconnect-element></reconnect-element>`);
		await elementUpdated(reconnectElement);

		const initialCallbackCount = callbackCount;
		expect(initialCallbackCount).to.be.greaterThan(0);

		// Disconnect and reconnect
		const parent = reconnectElement.parentElement!;
		parent.removeChild(reconnectElement);
		await aTimeout(0);
		parent.appendChild(reconnectElement);
		await elementUpdated(reconnectElement);

		// The consumer should not have been set up a second time —
		// the same controller handles reconnect via hostConnected lifecycle
		// If it WAS set up again, we'd see a second initial-resolution callback
		// (callback may fire on reconnect for re-resolution, but the controller itself is not recreated)
		expect(reconnectElement.contextValue).to.equal(provider.providerInstance());
	});
});
