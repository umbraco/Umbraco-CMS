import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostInterface, UmbControllerHostMixin } from './controller-host.mixin';
import { UmbContextProviderController } from '@umbraco-cms/context-api';

class MyClass {
	prop = 'value from provider';
}

@customElement('test-my-controller-host')
export class MyHostElement extends UmbControllerHostMixin(HTMLElement) {}

describe('UmbContextProvider', () => {
	type NewType = UmbControllerHostInterface;

	let hostElement: NewType;
	const contextInstance = new MyClass();

	beforeEach(() => {
		hostElement = document.createElement('test-my-controller-host') as UmbControllerHostInterface;
	});

	describe('Destroyed controllers is gone from host', () => {
		it('has a host property', () => {
			const ctrl = new UmbContextProviderController(hostElement, 'my-test-context', contextInstance);

			expect(hostElement.hasController(ctrl)).to.be.true;

			ctrl.destroy();

			expect(hostElement.hasController(ctrl)).to.be.false;
		});
	});

	describe('Unique controllers replace each other', () => {
		it('has a host property', () => {
			const firstCtrl = new UmbContextProviderController(hostElement, 'my-test-context', contextInstance);
			const secondCtrl = new UmbContextProviderController(hostElement, 'my-test-context', contextInstance);

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});
	});
});
