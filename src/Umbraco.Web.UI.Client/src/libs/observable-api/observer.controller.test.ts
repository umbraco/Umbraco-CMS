import { expect } from '@open-wc/testing';
import { UmbObjectState } from './states/object-state.js';
import { UmbObserverController } from './observer.controller.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-my-observer-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbObserverController', () => {
	describe('Observer Controllers against other Observer Controllers', () => {
		let hostElement: UmbTestControllerHostElement;

		beforeEach(() => {
			hostElement = document.createElement('test-my-observer-controller-host') as UmbTestControllerHostElement;
		});

		it('controller is replaced by another controller using the same string as controller-alias', () => {
			const state = new UmbObjectState(undefined);
			const observable = state.asObservable();

			const callbackMethod = (state: unknown) => {};

			const firstCtrl = new UmbObserverController(hostElement, observable, callbackMethod, 'my-test-alias');
			const secondCtrl = new UmbObserverController(hostElement, observable, callbackMethod, 'my-test-alias');

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is replaced by another controller using the the same symbol as controller-alias', () => {
			const state = new UmbObjectState(undefined);
			const observable = state.asObservable();

			const callbackMethod = (state: unknown) => {};

			const mySymbol = Symbol();
			const firstCtrl = new UmbObserverController(hostElement, observable, callbackMethod, mySymbol);
			const secondCtrl = new UmbObserverController(hostElement, observable, callbackMethod, mySymbol);

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is replacing another controller when using the same callback method and no controller-alias', () => {
			const state = new UmbObjectState(undefined);
			const observable = state.asObservable();

			const callbackMethod = (state: unknown) => {};

			const firstCtrl = new UmbObserverController(hostElement, observable, callbackMethod);
			const secondCtrl = new UmbObserverController(hostElement, observable, callbackMethod);

			expect(hostElement.hasController(firstCtrl)).to.be.false;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});

		it('controller is NOT replacing another controller when using a null for controller-alias', () => {
			const state = new UmbObjectState(undefined);
			const observable = state.asObservable();

			const callbackMethod = (state: unknown) => {};

			const firstCtrl = new UmbObserverController(hostElement, observable, callbackMethod, null);
			const secondCtrl = new UmbObserverController(hostElement, observable, callbackMethod, null);

			expect(hostElement.hasController(firstCtrl)).to.be.true;
			expect(hostElement.hasController(secondCtrl)).to.be.true;
		});
	});
});
