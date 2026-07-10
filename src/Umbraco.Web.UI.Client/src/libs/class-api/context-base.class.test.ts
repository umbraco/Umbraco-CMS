import { UmbContextBase } from './context-base.class.js';
import { UmbControllerBase } from './controller-base.class.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-test-context-base-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestContextBaseHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestClass extends UmbControllerBase {}

interface UmbTestGreetingContext {
	greeting: string;
	getHostElement(): Element;
}

class UmbTestGreetingContextImpl extends UmbContextBase implements UmbTestGreetingContext {
	greeting = 'hello';
}

const UMB_TEST_GREETING_CONTEXT = new UmbContextToken<UmbTestGreetingContext>('UmbTestContextBase.Greeting');

describe('UmbContextBase', () => {
	let host: UmbControllerHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-context-base-host></umb-test-context-base-host>`);
	});

	it('registers itself as a controller on the host on construction', () => {
		const ctx = new UmbTestGreetingContextImpl(host, UMB_TEST_GREETING_CONTEXT);
		expect(host.hasUmbController(ctx)).to.be.true;
	});

	it('makes itself available to a consumer asking for the same token', async () => {
		const ctx = new UmbTestGreetingContextImpl(host, UMB_TEST_GREETING_CONTEXT);
		const consumerHost = new UmbTestClass(host);

		const received = await new Promise<UmbTestGreetingContext | undefined>((resolve) => {
			consumerHost.consumeContext(UMB_TEST_GREETING_CONTEXT, (value) => {
				resolve(value);
			});
		});

		expect(received).to.equal(ctx);
	});

	it('is unprovided when destroyed — consumer callback fires again with undefined', async () => {
		const ctx = new UmbTestGreetingContextImpl(host, UMB_TEST_GREETING_CONTEXT);
		const consumerHost = new UmbTestClass(host);

		const received: Array<UmbTestGreetingContext | undefined> = [];
		await new Promise<void>((resolve) => {
			consumerHost.consumeContext(UMB_TEST_GREETING_CONTEXT, (value) => {
				received.push(value);
				if (received.length === 1) resolve();
			});
		});

		expect(received[0]).to.equal(ctx);

		await Promise.resolve();

		expect(received.at(-1)).to.be.undefined;

		ctx.destroy();
	});

	it('is removed from the host on destroy', () => {
		const ctx = new UmbTestGreetingContextImpl(host, UMB_TEST_GREETING_CONTEXT);
		expect(host.hasUmbController(ctx)).to.be.true;
		ctx.destroy();
		expect(host.hasUmbController(ctx)).to.be.false;
	});

	it('accepts a string contextAlias as well as an UmbContextToken', () => {
		// `super(host, contextToken.toString())` — UmbContextBase forwards the
		// string form to UmbControllerBase, so the alias-based lookup still works.
		class StringAliasContext extends UmbContextBase {
			constructor(host: UmbControllerHostElement) {
				super(host, 'UmbTestContextBase.StringAlias');
			}
		}

		const ctx = new StringAliasContext(host);
		expect(host.hasUmbController(ctx)).to.be.true;
		ctx.destroy();
		expect(host.hasUmbController(ctx)).to.be.false;
	});
});
