import { UmbControllerBase } from './controller-base.class.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-controller-base-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestControllerBaseHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestControllerBase extends UmbControllerBase {
	hostConnectedCalls = 0;
	hostDisconnectedCalls = 0;

	override hostConnected() {
		super.hostConnected();
		this.hostConnectedCalls++;
	}

	override hostDisconnected() {
		super.hostDisconnected();
		this.hostDisconnectedCalls++;
	}
}

describe('UmbControllerBase', () => {
	let host: UmbControllerHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-controller-base-host></umb-test-controller-base-host>`);
	});

	it('registers itself on the host on construction', () => {
		const controller = new UmbTestControllerBase(host);
		expect(host.hasUmbController(controller)).to.be.true;
	});

	it('exposes the host element via getHostElement()', () => {
		const controller = new UmbTestControllerBase(host);
		expect(controller.getHostElement()).to.equal(host);
	});

	it('auto-assigns a Symbol controllerAlias when none is provided', () => {
		const a = new UmbTestControllerBase(host);
		const b = new UmbTestControllerBase(host);
		// Each controller without an alias gets its own unique Symbol, so they
		// MUST coexist on the host (no replacement-by-alias).
		expect(typeof a.controllerAlias).to.equal('symbol');
		expect(a.controllerAlias).to.not.equal(b.controllerAlias);
		expect(host.hasUmbController(a)).to.be.true;
		expect(host.hasUmbController(b)).to.be.true;
	});

	it('replaces a previous controller registered with the same string alias', () => {
		const first = new UmbTestControllerBase(host, 'shared-alias');
		const second = new UmbTestControllerBase(host, 'shared-alias');
		expect(host.hasUmbController(first)).to.be.false;
		expect(host.hasUmbController(second)).to.be.true;
	});

	describe('destroy()', () => {
		it('removes itself from the host', () => {
			const controller = new UmbTestControllerBase(host);
			expect(host.hasUmbController(controller)).to.be.true;

			controller.destroy();

			expect(host.hasUmbController(controller)).to.be.false;
		});

		it('clears its _host reference (so getHostElement returns undefined afterwards)', () => {
			const controller = new UmbTestControllerBase(host);
			controller.destroy();
			expect(controller.getHostElement()).to.be.undefined;
		});

		it('also destroys nested child controllers', () => {
			const parent = new UmbTestControllerBase(host);
			const child = new UmbTestControllerBase(parent);
			expect(parent.hasUmbController(child)).to.be.true;

			parent.destroy();

			expect(host.hasUmbController(parent)).to.be.false;
			expect(parent.hasUmbController(child)).to.be.false;
		});

		it('is safe to call repeatedly (idempotent)', () => {
			const controller = new UmbTestControllerBase(host);
			expect(() => {
				controller.destroy();
				controller.destroy();
				controller.destroy();
			}).to.not.throw();
			expect(host.hasUmbController(controller)).to.be.false;
		});

		it('triggers hostDisconnected on the controller when removed from a connected host', async () => {
			const controller = new UmbTestControllerBase(host);

			await Promise.resolve();
			expect(controller.hostConnectedCalls).to.equal(1);
			expect(controller.hostDisconnectedCalls).to.equal(0);

			host.removeUmbController(controller);
			expect(controller.hostConnectedCalls).to.equal(1);
			expect(controller.hostDisconnectedCalls).to.equal(1);
		});
	});
});
