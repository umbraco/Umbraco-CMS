import { expect } from '@open-wc/testing';
import { UmbControllerHostElement, UmbControllerHostMixin } from './controller-host.mixin.js';
import { UmbBaseController } from './controller.class.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('test-my-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostMixin(HTMLElement) {}

export class UmbTestControllerImplementationElement extends UmbBaseController {}

describe('UmbContextProvider', () => {
	type NewType = UmbControllerHostElement;

	let hostElement: NewType;

	beforeEach(() => {
		hostElement = document.createElement('test-my-controller-host') as UmbControllerHostElement;
	});

	describe('Destroyed controllers is gone from host', () => {
		it('controller is removed from host when destroyed', () => {
			const ctrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');

			expect(hostElement.hasController(ctrl)).to.be.true;

			ctrl.destroy();

			expect(hostElement.hasController(ctrl)).to.be.false;
		});
	});

	describe('Controllers against other Controller', () => {
		it('controller is replaced by another controller using the same string as alias', () => {
			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');
			const secondCtrl = new UmbTestControllerImplementationElement(hostElement, 'my-test-context');

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is replaced by another controller using the the same symbol as alias', () => {
			const mySymbol = Symbol();
			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, mySymbol);
			const secondCtrl = new UmbTestControllerImplementationElement(hostElement, mySymbol);

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('remove controller using a symbol', () => {
			const mySymbol = Symbol();
			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, mySymbol);

			expect(hostElement.hasController(firstCtrl)).to.be.true;

			hostElement.removeControllerByAlias(mySymbol);

			expect(hostElement.hasController(firstCtrl)).to.be.false;
		});

		it('controller is not replacing another controller when using the undefined as alias', () => {
			const firstCtrl = new UmbTestControllerImplementationElement(hostElement, undefined);
			const secondCtrl = new UmbTestControllerImplementationElement(hostElement, undefined);

			expect(hostElement.hasController(firstCtrl)).to.be.true;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});
	});
});
