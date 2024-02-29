import { expect } from '@open-wc/testing';
import { type UmbControllerHostElement, UmbControllerHostElementMixin } from './controller-host-element.mixin.js';
import { UmbControllerHostMixin } from './controller-host.mixin.js';
import type { UmbControllerAlias } from './controller-alias.type.js';
import type { UmbControllerHost } from './controller-host.interface.js';
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
		this._host.addController(this);
	}

	getHostElement() {
		// Different from class.mixin implementation is that we here have a ? to make sure the bad test can run.
		return this._host?.getHostElement();
	}

	hostConnected(): void {
		super.hostConnected();
		this.testIsConnected = true;
	}
	hostDisconnected(): void {
		super.hostDisconnected();
		this.testIsConnected = false;
	}

	public destroy(): void {
		if (this._host) {
			this._host.removeController(this);
			this._host = undefined as any;
		}
		super.destroy();
		this.testIsDestroyed = true;
	}
}

describe('UmbController', () => {
	let hostElement: UmbControllerHostElement;

	beforeEach(() => {
		hostElement = document.createElement('test-my-controller-host') as UmbControllerHostElement;
	});

	describe('Controller Host Public API', () => {
		describe('methods', () => {
			it('has an hasController method', () => {
				expect(hostElement).to.have.property('hasController').that.is.a('function');
			});
			it('has an getControllers method', () => {
				expect(hostElement).to.have.property('getControllers').that.is.a('function');
			});
			it('has an addController method', () => {
				expect(hostElement).to.have.property('addController').that.is.a('function');
			});
			it('has an removeControllerByAlias method', () => {
				expect(hostElement).to.have.property('removeControllerByAlias').that.is.a('function');
			});
			it('has an removeController method', () => {
				expect(hostElement).to.have.property('removeController').that.is.a('function');
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
			it('has an hasController method', () => {
				expect(controller).to.have.property('hasController').that.is.a('function');
			});
			it('has an getControllers method', () => {
				expect(controller).to.have.property('getControllers').that.is.a('function');
			});
			it('has an addController method', () => {
				expect(controller).to.have.property('addController').that.is.a('function');
			});
			it('has an removeControllerByAlias method', () => {
				expect(controller).to.have.property('removeControllerByAlias').that.is.a('function');
			});
			it('has an removeController method', () => {
				expect(controller).to.have.property('removeController').that.is.a('function');
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
			expect(hostElement.hasController(ctrl)).to.be.true;
			// The controller does own a reference to its host:
			expect(hostElement.getHostElement()).to.be.equal(hostElement);

			ctrl.destroy();

			// The relation is removed:
			expect(hostElement.hasController(ctrl)).to.be.false;
			expect(ctrl.getHostElement()).to.be.undefined;
			expect(ctrl.testIsConnected).to.be.false;
			expect(ctrl.testIsDestroyed).to.be.true;
		});

		it('controller is removed from host when destroyed', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);

			expect(hostElement.hasController(ctrl)).to.be.true;
			expect(ctrl.hasController(subCtrl)).to.be.true;

			ctrl.destroy();

			expect(hostElement.hasController(ctrl)).to.be.false;
			expect(ctrl.testIsConnected).to.be.false;
			expect(ctrl.testIsDestroyed).to.be.true;
			expect(ctrl.hasController(subCtrl)).to.be.false;
			expect(subCtrl.testIsConnected).to.be.false;
			expect(subCtrl.testIsDestroyed).to.be.true;
		});

		it('controller is destroyed when removed from host', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);

			expect(ctrl.testIsDestroyed).to.be.false;
			expect(subCtrl.testIsDestroyed).to.be.false;
			expect(hostElement.hasController(ctrl)).to.be.true;
			expect(ctrl.hasController(subCtrl)).to.be.true;

			hostElement.removeController(ctrl);

			expect(ctrl.testIsDestroyed).to.be.true;
			expect(hostElement.hasController(ctrl)).to.be.false;
			expect(subCtrl.testIsDestroyed).to.be.true;
			expect(ctrl.hasController(subCtrl)).to.be.false;
		});

		it('all controllers are destroyed when the hosting controller gets destroyed', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);
			const subCtrl2 = new UmbTestControllerImplementation(ctrl);
			const subSubCtrl1 = new UmbTestControllerImplementation(subCtrl);
			const subSubCtrl2 = new UmbTestControllerImplementation(subCtrl);

			expect(ctrl.testIsDestroyed).to.be.false;
			expect(hostElement.hasController(ctrl)).to.be.true;
			// Subs:
			expect(subCtrl.testIsDestroyed).to.be.false;
			expect(subCtrl2.testIsDestroyed).to.be.false;
			expect(ctrl.hasController(subCtrl)).to.be.true;
			expect(ctrl.hasController(subCtrl2)).to.be.true;
			// Sub subs:
			expect(subSubCtrl1.testIsDestroyed).to.be.false;
			expect(subSubCtrl2.testIsDestroyed).to.be.false;
			expect(subCtrl.hasController(subSubCtrl1)).to.be.true;
			expect(subCtrl.hasController(subSubCtrl2)).to.be.true;

			ctrl.destroy();

			expect(ctrl.testIsDestroyed).to.be.true;
			expect(hostElement.hasController(ctrl)).to.be.false;
			// Subs:
			expect(subCtrl.testIsDestroyed).to.be.true;
			expect(subCtrl2.testIsDestroyed).to.be.true;
			expect(ctrl.hasController(subCtrl)).to.be.false;
			expect(ctrl.hasController(subCtrl2)).to.be.false;
			// Sub subs:
			expect(subSubCtrl1.testIsDestroyed).to.be.true;
			expect(subSubCtrl2.testIsDestroyed).to.be.true;
			expect(subCtrl.hasController(subSubCtrl1)).to.be.false;
			expect(subCtrl.hasController(subSubCtrl2)).to.be.false;
		});

		it('hostConnected & hostDisconnected is triggered accordingly to the state of the controller host.', () => {
			const ctrl = new UmbTestControllerImplementation(hostElement);
			const subCtrl = new UmbTestControllerImplementation(ctrl);

			expect(hostElement.hasController(ctrl)).to.be.true;
			expect(ctrl.hasController(subCtrl)).to.be.true;
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

			expect(hostElement.hasController(ctrl)).to.be.true;
			expect(ctrl.hasController(subCtrl)).to.be.true;
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

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is replaced by another controller using the the same symbol as controller-alias', () => {
			const mySymbol = Symbol();
			const firstCtrl = new UmbTestControllerImplementation(hostElement, mySymbol);
			const secondCtrl = new UmbTestControllerImplementation(hostElement, mySymbol);

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is not replacing another controller when using the undefined as controller-alias', () => {
			const firstCtrl = new UmbTestControllerImplementation(hostElement, undefined);
			const secondCtrl = new UmbTestControllerImplementation(hostElement, undefined);

			expect(hostElement.hasController(firstCtrl)).to.be.true;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('sub controllers is not replacing sub controllers of another host when using the same controller-alias', () => {
			const mySymbol = Symbol();

			const firstCtrl = new UmbTestControllerImplementation(hostElement, undefined);
			const secondCtrl = new UmbTestControllerImplementation(hostElement, undefined);

			const firstSubCtrl = new UmbTestControllerImplementation(firstCtrl, mySymbol);
			const secondSubCtrl = new UmbTestControllerImplementation(secondCtrl, mySymbol);

			expect(firstCtrl.hasController(firstSubCtrl)).to.be.true;
			expect(secondCtrl.hasController(secondSubCtrl)).to.be.true;
		});
	});
});
