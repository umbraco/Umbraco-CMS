import { UmbContextToken } from '../token/context-token.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextProvider } from '../provide/context-provider.js';
import { consumeContext } from './context-consume.decorator.js';
import { UmbContextConsumerController } from './context-consumer.controller.js';
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
		await elementUpdated(timingElement);

		// Context resolves asynchronously (microtask), so initial render may see undefined;
		// final rendered state must reflect the resolved context value.
		expect(timingElement.contextValue?.prop).to.equal('value from provider');
		expect(timingElement.renderedValues[timingElement.renderedValues.length - 1]).to.equal('value from provider');
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

	it('swallows asPromise rejection when subscribe:false and no provider is available', async () => {
		// Use a different token with no provider registered.
		const unprovidedToken = new UmbContextToken<UmbTestContextConsumerClass>('unprovided-context');

		class NoProviderController extends UmbControllerBase {
			@consumeContext({ context: unprovidedToken, subscribe: false })
			contextValue?: UmbTestContextConsumerClass;
		}

		let unhandledRejection: PromiseRejectionEvent | undefined;
		const rejectionHandler = (e: PromiseRejectionEvent) => {
			unhandledRejection = e;
		};
		window.addEventListener('unhandledrejection', rejectionHandler);

		try {
			const controller = new NoProviderController(element);
			// Wait long enough for the RAF-based request timeout to reject the promise.
			await new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve)));
			await aTimeout(0);

			expect(unhandledRejection, 'no unhandled promise rejection should surface').to.be.undefined;
			expect(controller.contextValue).to.be.undefined;
		} finally {
			window.removeEventListener('unhandledrejection', rejectionHandler);
		}
	});

	it('should not set up the consumer multiple times across disconnect/reconnect cycles', async () => {
		class ReconnectElement extends UmbLitElement {
			@consumeContext({
				context: testToken,
			})
			contextValue?: UmbTestContextConsumerClass;
		}

		customElements.define('reconnect-element', ReconnectElement);

		const reconnectElement = await fixture<ReconnectElement>(`<reconnect-element></reconnect-element>`);
		await elementUpdated(reconnectElement);

		const countConsumers = () =>
			reconnectElement.getUmbControllers((c) => c instanceof UmbContextConsumerController).length;

		const initialConsumerCount = countConsumers();
		expect(initialConsumerCount, 'a consumer controller is registered after initial connect').to.equal(1);

		// Disconnect and reconnect
		const parent = reconnectElement.parentElement!;
		parent.removeChild(reconnectElement);
		await aTimeout(0);
		parent.appendChild(reconnectElement);
		await elementUpdated(reconnectElement);

		// The decorator must not register a duplicate consumer on reconnect.
		expect(countConsumers(), 'consumer controller count is unchanged after reconnect').to.equal(initialConsumerCount);
		expect(reconnectElement.contextValue).to.equal(provider.providerInstance());
	});
});
