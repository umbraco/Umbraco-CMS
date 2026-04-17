import { UmbContextToken } from '../token/context-token.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextProvider } from '../provide/context-provider.js';
import { observedFrom } from './observed-from.decorator.js';
import { aTimeout, elementUpdated, expect, fixture } from '@open-wc/testing';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { html, state } from '@umbraco-cms/backoffice/external/lit';

class UmbTestObservedContext implements UmbContextMinimal {
	#name = new UmbStringState('initial name');
	readonly name = this.#name.asObservable();

	#isActive = new UmbBooleanState(false);
	readonly isActive = this.#isActive.asObservable();

	setName(value: string) {
		this.#name.setValue(value);
	}
	setIsActive(value: boolean) {
		this.#isActive.setValue(value);
	}

	getHostElement() {
		return document.body;
	}
}

const testToken = new UmbContextToken<UmbTestObservedContext>('observed-from-test-context');

describe('@observedFrom decorator', () => {
	let providerInstance: UmbTestObservedContext;
	let provider: UmbContextProvider;

	beforeEach(() => {
		providerInstance = new UmbTestObservedContext();
		provider = new UmbContextProvider(document.body, testToken, providerInstance);
		provider.hostConnected();
	});

	afterEach(() => {
		provider.destroy();
	});

	it('binds the decorated property to the observable slice', async () => {
		class MyElement extends UmbLitElement {
			@observedFrom(testToken, (ctx) => ctx.name)
			@state()
			name?: string;

			override render() {
				return html`<div>${this.name ?? 'no value'}</div>`;
			}
		}
		customElements.define('my-observed-from-element-1', MyElement);

		const el = await fixture<MyElement>(`<my-observed-from-element-1></my-observed-from-element-1>`);
		await elementUpdated(el);

		expect(el.name).to.equal('initial name');
	});

	it('re-renders when the observable emits', async () => {
		class MyElement extends UmbLitElement {
			@observedFrom(testToken, (ctx) => ctx.name)
			@state()
			name?: string;

			override render() {
				return html`<div>${this.name ?? 'no value'}</div>`;
			}
		}
		customElements.define('my-observed-from-element-2', MyElement);

		const el = await fixture<MyElement>(`<my-observed-from-element-2></my-observed-from-element-2>`);
		await elementUpdated(el);

		providerInstance.setName('updated name');
		await elementUpdated(el);

		expect(el.name).to.equal('updated name');
	});

	it('uses the default value until the observable emits', async () => {
		class MyElement extends UmbLitElement {
			@observedFrom(testToken, (ctx) => ctx.isActive, { default: false })
			@state()
			isActive?: boolean;
		}
		customElements.define('my-observed-from-element-3', MyElement);

		const el = await fixture<MyElement>(`<my-observed-from-element-3></my-observed-from-element-3>`);

		// Before context resolution, default should be applied
		expect(el.isActive).to.equal(false);

		await elementUpdated(el);

		// Observable should have emitted the initial false by now
		expect(el.isActive).to.equal(false);

		// Emit a new value
		providerInstance.setIsActive(true);
		await elementUpdated(el);
		expect(el.isActive).to.equal(true);
	});

	it('receives context when provider mounts after consumer (late arrival)', async () => {
		provider.destroy();

		class LateElement extends UmbLitElement {
			@observedFrom(testToken, (ctx) => ctx.name)
			@state()
			name?: string;
		}
		customElements.define('my-observed-from-element-4', LateElement);

		const el = await fixture<LateElement>(`<my-observed-from-element-4></my-observed-from-element-4>`);
		await elementUpdated(el);

		expect(el.name).to.be.undefined;

		// Now mount the provider
		const lateInstance = new UmbTestObservedContext();
		lateInstance.setName('late name');
		const lateProvider = new UmbContextProvider(document.body, testToken, lateInstance);
		lateProvider.hostConnected();

		await elementUpdated(el);

		expect(el.name).to.equal('late name');

		lateProvider.destroy();
		// Restore for afterEach
		provider = new UmbContextProvider(document.body, testToken, providerInstance);
		provider.hostConnected();
	});

	it('re-subscribes when a new provider is introduced', async () => {
		class MyElement extends UmbLitElement {
			@observedFrom(testToken, (ctx) => ctx.name)
			@state()
			name?: string;
		}
		customElements.define('my-observed-from-element-5', MyElement);

		const el = await fixture<MyElement>(`<my-observed-from-element-5></my-observed-from-element-5>`);
		await elementUpdated(el);

		expect(el.name).to.equal('initial name');

		// Introduce a closer provider
		const newInstance = new UmbTestObservedContext();
		newInstance.setName('new provider name');
		const newProvider = new UmbContextProvider(el, testToken, newInstance);
		newProvider.hostConnected();

		await elementUpdated(el);
		expect(el.name).to.equal('new provider name');

		newProvider.destroy();
	});

	it('handles context being unprovided (clears observation)', async () => {
		class MyElement extends UmbLitElement {
			@observedFrom(testToken, (ctx) => ctx.name)
			@state()
			name?: string;
		}
		customElements.define('my-observed-from-element-6', MyElement);

		const el = await fixture<MyElement>(`<my-observed-from-element-6></my-observed-from-element-6>`);
		await elementUpdated(el);
		expect(el.name).to.equal('initial name');

		provider.destroy();
		await aTimeout(0);

		// After unprovide, the observation alias is cleared; property keeps its last value
		// (current UmbContextConsumer semantics — no automatic reset to default)
		expect(el.name).to.equal('initial name');
	});
});

describe('this.observeContext helper', () => {
	let providerInstance: UmbTestObservedContext;
	let provider: UmbContextProvider;

	beforeEach(() => {
		providerInstance = new UmbTestObservedContext();
		provider = new UmbContextProvider(document.body, testToken, providerInstance);
		provider.hostConnected();
	});

	afterEach(() => {
		provider.destroy();
	});

	it('calls the callback when the observable emits', async () => {
		const received: string[] = [];

		class MyElement extends UmbLitElement {
			constructor() {
				super();
				this.observeContext(
					testToken,
					(ctx) => ctx.name,
					(value) => {
						if (typeof value === 'string') received.push(value);
					},
				);
			}
		}
		customElements.define('my-observe-context-element-1', MyElement);

		const el = await fixture<MyElement>(`<my-observe-context-element-1></my-observe-context-element-1>`);
		await elementUpdated(el);

		expect(received).to.include('initial name');

		providerInstance.setName('second');
		await aTimeout(0);

		expect(received).to.include('second');
	});

	it('re-subscribes when a new provider is introduced', async () => {
		const received: string[] = [];

		class MyElement extends UmbLitElement {
			constructor() {
				super();
				this.observeContext(
					testToken,
					(ctx) => ctx.name,
					(value) => {
						if (typeof value === 'string') received.push(value);
					},
				);
			}
		}
		customElements.define('my-observe-context-element-2', MyElement);

		const el = await fixture<MyElement>(`<my-observe-context-element-2></my-observe-context-element-2>`);
		await elementUpdated(el);

		const newInstance = new UmbTestObservedContext();
		newInstance.setName('new provider emit');
		const newProvider = new UmbContextProvider(el, testToken, newInstance);
		newProvider.hostConnected();

		await aTimeout(0);

		expect(received).to.include('new provider emit');

		newProvider.destroy();
	});
});
