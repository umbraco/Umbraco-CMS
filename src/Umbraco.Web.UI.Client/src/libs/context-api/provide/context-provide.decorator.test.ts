import { UmbContextToken } from '../token/context-token.js';
import type { UmbContextMinimal } from '../types.js';
import { provideContext } from './context-provide.decorator.js';
import { aTimeout, elementUpdated, expect, fixture } from '@open-wc/testing';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

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
});
