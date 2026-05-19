import { UmbClassMixin } from './class.mixin.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbControllerHostElementMixin,
	type UmbControllerHostElement,
} from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-class-mixin-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestClassMixinHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestClass extends UmbClassMixin(EventTarget) {}

describe('UmbClassMixin', () => {
	let host: UmbControllerHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-class-mixin-host></umb-test-class-mixin-host>`);
	});

	describe('observe()', () => {
		it('subscribes to an observable and forwards values to the callback', async () => {
			const ctrl = new UmbTestClass(host);
			const state = new UmbBasicState(0);
			const values: Array<number> = [];

			ctrl.observe(state.asObservable(), (value) => values.push(value));

			// Initial value emitted synchronously on subscribe.
			expect(values).to.eql([0]);

			state.setValue(1);
			expect(values).to.eql([0, 1]);

			state.setValue(2);
			expect(values).to.eql([0, 1, 2]);

			ctrl.destroy();
		});

		it('stops emitting after the controller is destroyed', () => {
			const ctrl = new UmbTestClass(host);
			const state = new UmbBasicState(0);
			const values: Array<number> = [];

			ctrl.observe(state.asObservable(), (value) => values.push(value));
			expect(values).to.eql([0]);

			ctrl.destroy();
			state.setValue(99);

			// No new value should arrive after destroy.
			expect(values).to.eql([0]);
		});

		it('replaces a previous observation when called twice with the same callback identity', async () => {
			const ctrl = new UmbTestClass(host);
			const stateA = new UmbBasicState('a1');
			const stateB = new UmbBasicState('b1');
			const values: Array<string> = [];

			// The mixin auto-derives the controller alias from the callback's
			// `toString()` hash, so re-observing with the same callback function
			// replaces the previous observer.
			const callback = (value: string) => values.push(value);

			ctrl.observe(stateA.asObservable(), callback);
			expect(values).to.eql(['a1']);

			ctrl.observe(stateB.asObservable(), callback);
			// Second subscription emits its initial value.
			expect(values).to.contain('b1');

			// Updates to the abandoned observable must not reach the callback.
			const lengthBeforeMutation = values.length;
			stateA.setValue('a2');
			await aTimeout(0);
			expect(values.length).to.equal(lengthBeforeMutation);

			ctrl.destroy();
		});

		it('invokes the callback with undefined and removes any prior observer when source is undefined', () => {
			const ctrl = new UmbTestClass(host);
			const state = new UmbBasicState(0);
			const values: Array<number | undefined> = [];

			const callback = (value: number | undefined) => values.push(value);

			ctrl.observe(state.asObservable(), callback);
			expect(values).to.eql([0]);

			ctrl.observe(undefined, callback);

			// Callback was invoked once more, with undefined.
			expect(values[values.length - 1]).to.be.undefined;

			// And the abandoned observer must no longer receive updates.
			const lengthBefore = values.length;
			state.setValue(42);
			expect(values.length).to.equal(lengthBefore);

			ctrl.destroy();
		});
	});

	describe('provideContext / consumeContext', () => {
		// The context API requires every provided instance to satisfy
		// `UmbContextMinimal { getHostElement(): Element }` — the consumer reads
		// it to scope context lookups. Real callers usually achieve this by
		// extending `UmbContextBase`; in this test we're only exercising the
		// mixin's wiring, so we attach the method directly.
		const minimalApi = <T extends object>(instance: T, hostEl: Element): T & { getHostElement(): Element } =>
			Object.assign(instance, { getHostElement: () => hostEl });

		it('lets a child controller resolve a context provided by a parent', async () => {
			const TOKEN = new UmbContextToken<{ getHostElement(): Element; greeting: string }>(
				'UmbTestClassMixin.Greeting',
			);
			const provider = new UmbTestClass(host);
			const apiInstance = minimalApi({ greeting: 'hello' }, host);
			provider.provideContext(TOKEN, apiInstance);

			let received: typeof apiInstance | undefined;
			await new Promise<void>((resolve) => {
				const consumerHost = new UmbTestClass(provider);
				consumerHost.consumeContext(TOKEN, (value) => {
					received = value;
					resolve();
				});
			});

			expect(received).to.equal(apiInstance);
			provider.destroy();
		});

		it('getContext resolves the same instance asynchronously', async () => {
			const TOKEN = new UmbContextToken<{ getHostElement(): Element; count: number }>('UmbTestClassMixin.Counter');
			const provider = new UmbTestClass(host);
			const apiInstance = minimalApi({ count: 7 }, host);
			provider.provideContext(TOKEN, apiInstance);

			const consumerHost = new UmbTestClass(provider);
			const result = await consumerHost.getContext(TOKEN);
			expect(result).to.equal(apiInstance);

			provider.destroy();
		});
	});

	describe('destroy()', () => {
		it('removes the controller from its host', () => {
			const ctrl = new UmbTestClass(host);
			expect(host.hasUmbController(ctrl)).to.be.true;
			ctrl.destroy();
			expect(host.hasUmbController(ctrl)).to.be.false;
		});

		it('clears the host reference (getHostElement returns undefined after destroy)', () => {
			const ctrl = new UmbTestClass(host);
			ctrl.destroy();
			expect(ctrl.getHostElement()).to.be.undefined;
		});

		it('is safe to call repeatedly', () => {
			const ctrl = new UmbTestClass(host);
			expect(() => {
				ctrl.destroy();
				ctrl.destroy();
			}).to.not.throw();
		});
	});
});
