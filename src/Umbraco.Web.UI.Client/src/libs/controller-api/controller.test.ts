import type { UmbControllerHostElement } from './controller-host-element.interface.js';
import { UmbControllerHostElementMixin } from './controller-host-element.mixin.js';
import { UmbControllerHostMixin } from './controller-host.mixin.js';
import type { UmbControllerAlias } from './controller-alias.type.js';
import type { UmbControllerHost } from './controller-host.interface.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestControllerImplementation extends UmbControllerHostMixin(class {}) {
	testIsConnected = false;
	testIsDestroyed = false;

	private _host: UmbControllerHost;
	readonly controllerAlias: UmbControllerAlias;

	constructor(host: UmbControllerHost, controllerAlias?: UmbControllerAlias) {
		super();
		this._host = host;
		this.controllerAlias = controllerAlias ?? Symbol(); // This will fallback to a Symbol, ensuring that this class is only appended to the controller host once.
		this._host.addUmbController(this);
	}

	getHostElement() {
		// Different from class.mixin implementation is that we here have a ? to make sure the bad test can run.
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

describe('UmbController', () => {
	let hostElement: UmbControllerHostElement;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
	});

	describe('Controller Host Public API', () => {
		describe('methods', () => {
			it('has an hasUmbController method', () => {
				expect(hostElement).to.have.property('hasUmbController').that.is.a('function');
			});
			it('has an getUmbControllers method', () => {
				expect(hostElement).to.have.property('getUmbControllers').that.is.a('function');
			});
			it('has an addUmbController method', () => {
				expect(hostElement).to.have.property('addUmbController').that.is.a('function');
			});
			it('has an removeUmbControllerByAlias method', () => {
				expect(hostElement).to.have.property('removeUmbControllerByAlias').that.is.a('function');
			});
			it('has an removeUmbController method', () => {
				expect(hostElement).to.have.property('removeUmbController').that.is.a('function');
			});
			it('has an hostConnected method', () => {
				expect(hostElement).to.have.property('hostConnected').that.is.a('function');
			});
			it('has an hostDisconnected method', () => {
				expect(hostElement).to.have.property('hostDisconnected').that.is.a('function');
			});
			it('has an destroy method', () => {
				expect(hostElement).to.have.property('destroy').that.is.a('function');
			});
		});
	});

	describe('Controller Public API', () => {
		let controller: UmbTestControllerImplementation;
		beforeEach(() => {
			controller = new UmbTestControllerImplementation(hostElement, 'my-test-controller-alias');
		});

		describe('methods', () => {
			it('has an controllerAlias property', () => {
				expect(controller).to.have.property('controllerAlias').that.is.a('string');
				expect(controller.controllerAlias).to.be.equal('my-test-controller-alias');
			});
			it('has an hasUmbController method', () => {
				expect(controller).to.have.property('hasUmbController').that.is.a('function');
			});
			it('has an getUmbControllers method', () => {
				expect(controller).to.have.property('getUmbControllers').that.is.a('function');
			});
			it('has an addUmbController method', () => {
				expect(controller).to.have.property('addUmbController').that.is.a('function');
			});
			it('has an removeUmbControllerByAlias method', () => {
				expect(controller).to.have.property('removeUmbControllerByAlias').that.is.a('function');
			});
			it('has an removeUmbController method', () => {
				expect(controller).to.have.property('removeUmbController').that.is.a('function');
			});
			it('has an hostConnected method', () => {
				expect(controller).to.have.property('hostConnected').that.is.a('function');
			});
			it('has an hostDisconnected method', () => {
				expect(controller).to.have.property('hostDisconnected').that.is.a('function');
			});
			it('has an destroy method', () => {
				expect(controller).to.have.property('destroy').that.is.a('function');
			});
		});
	});

	describe('Controllers lifecycle', () => {
		it('host-element relation is removed when destroyed', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);

			// The host does own a reference to its controller:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;
			// The controller does own a reference to its host:
			expect(hostElement.getHostElement()).to.be.equal(hostElement);

			ctrl.destroy();

			// The relation is removed:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
			expect(ctrl.getHostElement()).to.be.undefined;
			expect(ctrl.testIsConnected).to.be.false;
			expect(ctrl.testIsDestroyed).to.be.true;
		});

		it('controller is removed from host when destroyed', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);

			expect(hostElement.hasUmbController(ctrl)).to.be.true;
			expect(ctrl.hasUmbController(subCtrl)).to.be.true;

			ctrl.destroy();

			expect(hostElement.hasUmbController(ctrl)).to.be.false;
			expect(ctrl.testIsConnected).to.be.false;
			expect(ctrl.testIsDestroyed).to.be.true;
			expect(ctrl.hasUmbController(subCtrl)).to.be.false;
			expect(subCtrl.testIsConnected).to.be.false;
			expect(subCtrl.testIsDestroyed).to.be.true;
		});

		it('controller is destroyed when removed from host', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);

			expect(ctrl.testIsDestroyed).to.be.false;
			expect(subCtrl.testIsDestroyed).to.be.false;
			expect(hostElement.hasUmbController(ctrl)).to.be.true;
			expect(ctrl.hasUmbController(subCtrl)).to.be.true;

			hostElement.removeUmbController(ctrl);

			expect(ctrl.testIsDestroyed).to.be.true;
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
			expect(subCtrl.testIsDestroyed).to.be.true;
			expect(ctrl.hasUmbController(subCtrl)).to.be.false;
		});

		it('all controllers are destroyed when the hosting controller gets destroyed', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);
			const subCtrl2 = new UmbTestControllerImplementation(ctrl);
			const subSubCtrl1 = new UmbTestControllerImplementation(subCtrl);
			const subSubCtrl2 = new UmbTestControllerImplementation(subCtrl);

			expect(ctrl.testIsDestroyed).to.be.false;
			expect(hostElement.hasUmbController(ctrl)).to.be.true;
			// Subs:
			expect(subCtrl.testIsDestroyed).to.be.false;
			expect(subCtrl2.testIsDestroyed).to.be.false;
			expect(ctrl.hasUmbController(subCtrl)).to.be.true;
			expect(ctrl.hasUmbController(subCtrl2)).to.be.true;
			// Sub subs:
			expect(subSubCtrl1.testIsDestroyed).to.be.false;
			expect(subSubCtrl2.testIsDestroyed).to.be.false;
			expect(subCtrl.hasUmbController(subSubCtrl1)).to.be.true;
			expect(subCtrl.hasUmbController(subSubCtrl2)).to.be.true;

			ctrl.destroy();

			expect(ctrl.testIsDestroyed).to.be.true;
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
			// Subs:
			expect(subCtrl.testIsDestroyed).to.be.true;
			expect(subCtrl2.testIsDestroyed).to.be.true;
			expect(ctrl.hasUmbController(subCtrl)).to.be.false;
			expect(ctrl.hasUmbController(subCtrl2)).to.be.false;
			// Sub subs:
			expect(subSubCtrl1.testIsDestroyed).to.be.true;
			expect(subSubCtrl2.testIsDestroyed).to.be.true;
			expect(subCtrl.hasUmbController(subSubCtrl1)).to.be.false;
			expect(subCtrl.hasUmbController(subSubCtrl2)).to.be.false;
		});

		it('hostConnected & hostDisconnected is triggered accordingly to the state of the controller host.', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);

			expect(hostElement.hasUmbController(ctrl)).to.be.true;
			expect(ctrl.hasUmbController(subCtrl)).to.be.true;
			expect(ctrl.testIsConnected).to.be.false;
			expect(subCtrl.testIsConnected).to.be.false;

			document.body.appendChild(hostElement);

			expect(ctrl.testIsConnected).to.be.true;
			expect(subCtrl.testIsConnected).to.be.true;

			document.body.removeChild(hostElement);

			expect(ctrl.testIsConnected).to.be.false;
			expect(subCtrl.testIsConnected).to.be.false;
		});

		it('hostConnected is triggered if controller host is already connected at time of adding controller.', async () => {
			document.body.appendChild(hostElement);

			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);

			expect(hostElement.hasUmbController(ctrl)).to.be.true;
			expect(ctrl.hasUmbController(subCtrl)).to.be.true;
			expect(ctrl.testIsConnected).to.be.false;
			expect(subCtrl.testIsConnected).to.be.false;

			// Wait one JS cycle, to ensure that the hostConnected is triggered. (Currently its by design that we trigger the hostConnected with one cycle delay)
			await Promise.resolve();

			expect(ctrl.testIsConnected).to.be.true;
			expect(subCtrl.testIsConnected).to.be.true;

			document.body.removeChild(hostElement);

			expect(ctrl.testIsConnected).to.be.false;
			expect(subCtrl.testIsConnected).to.be.false;
		});
	});

	describe('Controllers against other Controllers', () => {
		it('controller is replaced by another controller using the same string as controller-alias', () => {
			const firstCtrl = new UmbTestControllerImplementation(hostElement, 'my-test-alias');
			const secondCtrl = new UmbTestControllerImplementation(hostElement, 'my-test-alias');

			expect(hostElement.hasUmbController(firstCtrl)).to.be.false;
			expect(hostElement.hasUmbController(secondCtrl)).to.be.true;
		});

		it('controller is replaced by another controller using the the same symbol as controller-alias', () => {
			const mySymbol = Symbol();
			const firstCtrl = new UmbTestControllerImplementation(hostElement, mySymbol);
			const secondCtrl = new UmbTestControllerImplementation(hostElement, mySymbol);

			expect(hostElement.hasUmbController(firstCtrl)).to.be.false;
			expect(hostElement.hasUmbController(secondCtrl)).to.be.true;
		});

		it('controller is not replacing another controller when using the undefined as controller-alias', () => {
			const firstCtrl = new UmbTestControllerImplementation(hostElement, undefined);
			const secondCtrl = new UmbTestControllerImplementation(hostElement, undefined);

			expect(hostElement.hasUmbController(firstCtrl)).to.be.true;
			expect(hostElement.hasUmbController(secondCtrl)).to.be.true;
		});

		it('sub controllers is not replacing sub controllers of another host when using the same controller-alias', () => {
			const mySymbol = Symbol();

			const firstCtrl = new UmbTestControllerImplementation(hostElement, undefined);
			const secondCtrl = new UmbTestControllerImplementation(hostElement, undefined);

			const firstSubCtrl = new UmbTestControllerImplementation(firstCtrl, mySymbol);
			const secondSubCtrl = new UmbTestControllerImplementation(secondCtrl, mySymbol);

			expect(firstCtrl.hasUmbController(firstSubCtrl)).to.be.true;
			expect(secondCtrl.hasUmbController(secondSubCtrl)).to.be.true;
		});
	});
});
