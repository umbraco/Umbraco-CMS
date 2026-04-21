import { UmbContextToken } from '../token/context-token.js';
import type { UmbContextMinimal } from '../types.js';
import { provideContext } from './context-provide.decorator.js';
import { aTimeout, elementUpdated, expect, fixture } from '@open-wc/testing';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { html } from '@umbraco-cms/backoffice/external/lit';

class UmbTestContextConsumerClass implements UmbContextMinimal {
	public prop: string;

	constructor(initialValue = 'value from provider') {
		this.prop = initialValue;
	}

	getHostElement() {
		return document.body;
	}
}

const testToken = new UmbContextToken<UmbTestContextConsumerClass>('my-test-context', 'testApi');

class MyTestRootElement extends UmbLitElement {
	@provideContext({ context: testToken })
	providerInstance = new UmbTestContextConsumerClass();
}

customElements.define('my-provide-test-element', MyTestRootElement);

class MyTestElement extends UmbLitElement {
	contextValue?: UmbTestContextConsumerClass;

	constructor() {
		super();

		this.consumeContext(testToken, (value) => {
			this.contextValue = value;
		});
	}
}

customElements.define('my-consume-test-element', MyTestElement);

describe('@provide decorator', () => {
	let rootElement: MyTestRootElement;
	let element: MyTestElement;

	beforeEach(async () => {
		rootElement = await fixture<MyTestRootElement>(
			`<my-provide-test-element><my-consume-test-element></my-consume-test-element></my-provide-test-element>`,
		);
		element = rootElement.querySelector('my-consume-test-element') as MyTestElement;
	});

	afterEach(() => {});

	it('should receive a context value when provided on the host', () => {
		expect(element.contextValue).to.equal(rootElement.providerInstance);
	});

	it('should work when the decorator is used in a controller', async () => {
		class MyController extends UmbControllerBase {
			@provideContext({ context: testToken })
			providerInstance = new UmbTestContextConsumerClass('new value');
		}

		const controller = new MyController(element);

		await elementUpdated(element);

		expect(element.contextValue).to.equal(controller.providerInstance);
		expect(controller.providerInstance.prop).to.equal('new value');
	});

	it('should not update the instance when the property changes', async () => {
		// we do not support setting a new value on a provided property
		// as it would require a lot more logic to handle updating the context consumers
		// So for now we just warn the user that this is not supported
		// This might be revisited in the future if there is a need for it

		const originalProviderInstance = rootElement.providerInstance;

		const newProviderInstance = new UmbTestContextConsumerClass('new value from provider');
		rootElement.providerInstance = newProviderInstance;

		await aTimeout(0);

		expect(element.contextValue).to.equal(originalProviderInstance);
		expect(element.contextValue?.prop).to.equal(originalProviderInstance.prop);
	});

	it('should update the context value when the provider instance is replaced', async () => {
		const newProviderInstance = new UmbTestContextConsumerClass();
		newProviderInstance.prop = 'new value from provider';

		class MyUpdateTestElement extends UmbLitElement {
			@provideContext({ context: testToken })
			providerInstance = newProviderInstance;
		}
		customElements.define('my-update-provide-test-element', MyUpdateTestElement);

		const newProvider = await fixture<MyUpdateTestElement>(
			`<my-update-provide-test-element><my-consume-test-element></my-consume-test-element></my-update-provide-test-element>`,
		);
		const element = newProvider.querySelector('my-consume-test-element') as MyTestElement;

		await elementUpdated(element);

		expect(element.contextValue).to.equal(newProviderInstance);
		expect(element.contextValue?.prop).to.equal(newProviderInstance.prop);
	});

	it('should provide context to descendants on first render', async () => {
		// The provider decorator's controller must be set up before the descendant's consume
		// runs its request — otherwise the descendant would see undefined during its first render.
		const providerInstance = new UmbTestContextConsumerClass('early value');

		class TimingProviderElement extends UmbLitElement {
			@provideContext({ context: testToken })
			providerInstance = providerInstance;
		}
		customElements.define('timing-provider-element', TimingProviderElement);

		// Capture the context value seen by the descendant DURING its first render(), not after.
		// If the provider isn't set up in time, render() would see undefined.
		class TimingConsumerElement extends UmbLitElement {
			public renderedValues: (UmbTestContextConsumerClass | undefined)[] = [];
			contextValue?: UmbTestContextConsumerClass;

			constructor() {
				super();
				this.consumeContext(testToken, (value) => {
					this.contextValue = value;
				});
			}

			override render() {
				this.renderedValues.push(this.contextValue);
				return html`<div>${this.contextValue?.prop ?? 'no value'}</div>`;
			}
		}
		customElements.define('timing-consumer-element', TimingConsumerElement);

		const providerEl = await fixture<TimingProviderElement>(
			`<timing-provider-element><timing-consumer-element></timing-consumer-element></timing-provider-element>`,
		);
		const consumerEl = providerEl.querySelector('timing-consumer-element') as TimingConsumerElement;

		await elementUpdated(consumerEl);

		// The first render must already have seen the context value.
		expect(consumerEl.renderedValues[0], 'context value seen during first render').to.equal(providerInstance);
		// The descendant must never have rendered with undefined.
		expect(consumerEl.renderedValues, 'consumer rendered with undefined at some point').to.not.include(undefined);
	});

	it('should destroy without throwing when the legacy init-controller wrapper is registered', async () => {
		// Regression guard: the legacy @provideContext path registers a tiny init-only
		// UmbController to defer reading element[propertyKey] until hostConnected. That
		// wrapper's destroy() must remove itself from the host — otherwise the host's
		// destroy loop detects a controller that "does not remove itself" and throws.
		class DestroyProbeElement extends UmbLitElement {
			@provideContext({ context: testToken })
			providerInstance = new UmbTestContextConsumerClass();
		}
		customElements.define('destroy-probe-element', DestroyProbeElement);

		const el = await fixture<DestroyProbeElement>(`<destroy-probe-element></destroy-probe-element>`);
		expect(() => el.destroy()).to.not.throw();
	});
});
