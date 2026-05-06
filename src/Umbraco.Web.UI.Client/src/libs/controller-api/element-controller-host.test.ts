import { UmbElementControllerHost } from './element-controller-host.js';
import { UmbControllerHostElementMixin } from './controller-host-element.mixin.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerAlias } from './controller-alias.type.js';
import type { UmbControllerHost } from './controller-host.interface.js';
import { UmbControllerHostMixin } from './controller-host.mixin.js';

class UmbTestContext {
	value = 'test-context-value';
	getHostElement() {
		return undefined as unknown as Element;
	}
}

class UmbTestController extends UmbControllerHostMixin(class {}) {
	testIsConnected = false;
	testIsDestroyed = false;

	private _host: UmbControllerHost;
	readonly controllerAlias: UmbControllerAlias;

	constructor(host: UmbControllerHost, controllerAlias?: UmbControllerAlias) {
		super();
		this._host = host;
		this.controllerAlias = controllerAlias ?? Symbol();
		this._host.addUmbController(this);
	}

	getHostElement() {
		return this._host?.getHostElement();
	}

	override hostConnected(): void {
		super.hostConnected();
		this.testIsConnected = true;
	}
	override hostDisconnected(): void {
		super.hostDisconnected();
		this.testIsConnected = false;
	}

	public override destroy(): void {
		if (this._host) {
			this._host.removeUmbController(this);
			this._host = undefined as any;
		}
		super.destroy();
		this.testIsDestroyed = true;
	}
}

@customElement('test-element-controller-host-consumer')
class UmbTestConsumerElement extends UmbControllerHostElementMixin(HTMLElement) {
	public contextValue?: string;
	constructor() {
		super();
		new UmbContextConsumerController<UmbTestContext>(this, 'test-context', (context) => {
			this.contextValue = context?.value;
		});
	}
}

describe('UmbElementControllerHost', () => {
	describe('getHostElement', () => {
		it('returns the element passed in the constructor', () => {
			const element = document.createElement('div');
			const host = new UmbElementControllerHost(element);
			expect(host.getHostElement()).to.equal(element);
		});
	});

	describe('Controller lifecycle', () => {
		let element: HTMLElement;
		let host: UmbElementControllerHost;

		beforeEach(() => {
			element = document.createElement('div');
			host = new UmbElementControllerHost(element);
		});

		it('controllers are connected when hostConnected is called', async () => {
			const ctrl = new UmbTestController(host);
			expect(ctrl.testIsConnected).to.be.false;

			host.hostConnected();
			await Promise.resolve();

			expect(ctrl.testIsConnected).to.be.true;
		});

		it('controllers are disconnected when hostDisconnected is called', async () => {
			const ctrl = new UmbTestController(host);
			host.hostConnected();
			await Promise.resolve();
			expect(ctrl.testIsConnected).to.be.true;

			host.hostDisconnected();

			expect(ctrl.testIsConnected).to.be.false;
		});

		it('controllers are destroyed when host is destroyed', async () => {
			const ctrl = new UmbTestController(host);
			host.hostConnected();
			await Promise.resolve();

			host.destroy();

			expect(ctrl.testIsDestroyed).to.be.true;
			expect(host.hasUmbController(ctrl)).to.be.false;
		});

		it('sub-controllers are destroyed when host is destroyed', async () => {
			const ctrl = new UmbTestController(host);
			const subCtrl = new UmbTestController(ctrl);
			host.hostConnected();
			await Promise.resolve();

			host.destroy();

			expect(ctrl.testIsDestroyed).to.be.true;
			expect(subCtrl.testIsDestroyed).to.be.true;
		});
	});

	describe('Context provision', () => {
		it('provides context to descendant elements via the backed element', async () => {
			const wrapper = await fixture<HTMLElement>(html`
				<div>
					<test-element-controller-host-consumer></test-element-controller-host-consumer>
				</div>
			`);

			const consumer = wrapper.querySelector(
				'test-element-controller-host-consumer',
			) as UmbTestConsumerElement;

			const host = new UmbElementControllerHost(wrapper);
			host.hostConnected();
			new UmbContextProviderController(host, 'test-context', new UmbTestContext());

			// Wait for async context resolution
			await new Promise((resolve) => setTimeout(resolve, 0));

			expect(consumer.contextValue).to.equal('test-context-value');
		});

		it('does not provide context after host is destroyed', async () => {
			const wrapper = await fixture<HTMLElement>(html`
				<div>
					<test-element-controller-host-consumer></test-element-controller-host-consumer>
				</div>
			`);

			const host = new UmbElementControllerHost(wrapper);
			host.hostConnected();
			new UmbContextProviderController(host, 'test-context', new UmbTestContext());

			await new Promise((resolve) => setTimeout(resolve, 0));

			const consumer = wrapper.querySelector(
				'test-element-controller-host-consumer',
			) as UmbTestConsumerElement;
			expect(consumer.contextValue).to.equal('test-context-value');

			// Destroy and add a new consumer
			host.destroy();

			const newConsumer = document.createElement(
				'test-element-controller-host-consumer',
			) as UmbTestConsumerElement;
			wrapper.appendChild(newConsumer);

			await new Promise((resolve) => setTimeout(resolve, 0));

			expect(newConsumer.contextValue).to.be.undefined;
		});
	});
});
