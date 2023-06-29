import { expect } from '@open-wc/testing';
import { UmbControllerHostElement, UmbControllerHostElementMixin } from './controller-host-element.mixin.js';
import { UmbBaseController } from './controller.class.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('test-my-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

export class UmbTestControllerImplementationElement extends UmbBaseController {
	testIsConnected = false;
	testIsDestroyed = false;

	hostConnected(): void {
		super.hostConnected();
		this.testIsConnected = true;
	}
	hostDisconnected(): void {
		super.hostDisconnected();
		this.testIsConnected = false;
	}

	public destroy(): void {
		super.destroy();
		this.testIsDestroyed = true;
	}
}

describe('UmbController', () => {
	type NewType = UmbControllerHostElement;

	let hostElement: NewType;

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
		let controller: UmbTestControllerImplementationElement;
		beforeEach(() => {
			controller = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');
		});

		describe('methods', () => {
			it('has an controllerAlias property', () => {
				expect(controller).to.have.property('controllerAlias').that.is.a('string');
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
		it('controller is removed from host when destroyed', () => {
			const ctrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');
			const subCtrl = new UmbTestControllerImplementationElement(ctrl, 'my-test-context');

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
			const ctrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');
			const subCtrl = new UmbTestControllerImplementationElement(ctrl, 'my-test-context');

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

		it('hostConnected & hostDisconnected is triggered accordingly to the state of the controller host.', () => {
			const ctrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');
			const subCtrl = new UmbTestControllerImplementationElement(ctrl, 'my-test-context');

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
	});

	describe('Controllers against other Controller', () => {
		it('controller is replaced by another controller using the same string as controller-alias', () => {
			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');
			const secondCtrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is replaced by another controller using the the same symbol as controller-alias', () => {
			const mySymbol = Symbol();
			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, mySymbol);
			const secondCtrl = new UmbTestControllerImplementationElement(hostElement, mySymbol);

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is not replacing another controller when using the undefined as controller-alias', () => {
			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, undefined);
			const secondCtrl = new UmbTestControllerImplementationElement(hostElement, undefined);

			expect(hostElement.hasController(firstCtrl)).to.be.true;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('sub controllers is not replacing another sub controllers when using the same controller-alias', () => {
			const mySymbol = Symbol();

			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, undefined);
			const secondCtrl = new UmbTestControllerImplementationElement(hostElement, undefined);

			const firstSubCtrl = new UmbTestControllerImplementationElement(firstCtrl, mySymbol);
			const secondSubCtrl = new UmbTestControllerImplementationElement(secondCtrl, mySymbol);

			expect(firstCtrl.hasController(firstSubCtrl)).to.be.true;
			expect(secondCtrl.hasController(secondSubCtrl)).to.be.true;
		});
	});
});
