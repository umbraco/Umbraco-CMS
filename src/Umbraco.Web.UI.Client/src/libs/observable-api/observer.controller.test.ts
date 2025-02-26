import { UmbObjectState } from './states/object-state.js';
import { UmbObserverController } from './observer.controller.js';
import { simpleHashCode } from './utils/simple-hash-code.function.js';
import { expect } from '@open-wc/testing';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

describe('UmbObserverController', () => {
	describe('Observer Controllers against other Observer Controllers', () => {
		let hostElement: UmbControllerHostElement;

		beforeEach(() => {
			hostElement = document.createElement('umb-controller-host') as UmbControllerHostElement;
		});

		it('controller is replaced by another controller using the same string as controller-alias', () => {
			const state = new UmbObjectState(undefined);
			const observable = state.asObservable();

			const callbackMethod = () => {};

			const firstCtrl = new UmbObserverController(hostElement, observable, callbackMethod, 'my-test-alias');
			const secondCtrl = new UmbObserverController(hostElement, observable, callbackMethod, 'my-test-alias');

			expect(hostElement.hasUmbController(firstCtrl)).to.be.false;
			expect(hostElement.hasUmbController(secondCtrl)).to.be.true;
		});

		it('controller is replaced by another controller using the the same symbol as controller-alias', () => {
			const state = new UmbObjectState(undefined);
			const observable = state.asObservable();

			const callbackMethod = () => {};

			const mySymbol = Symbol();
			const firstCtrl = new UmbObserverController(hostElement, observable, callbackMethod, mySymbol);
			const secondCtrl = new UmbObserverController(hostElement, observable, callbackMethod, mySymbol);

			expect(hostElement.hasUmbController(firstCtrl)).to.be.false;
			expect(hostElement.hasUmbController(secondCtrl)).to.be.true;
		});

		it('controller is NOT replacing another controller when using a null for controller-alias', () => {
			const state = new UmbObjectState(undefined);
			const observable = state.asObservable();

			const callbackMethod = () => {};

			// Imitates the behavior of the observe method in the UmbClassMixin
			let controllerAlias1 = null;
			controllerAlias1 ??= controllerAlias1 === undefined ? simpleHashCode(callbackMethod.toString()) : undefined;

			const firstCtrl = new UmbObserverController(hostElement, observable, callbackMethod, controllerAlias1);

			let controllerAlias2 = null;
			controllerAlias2 ??= controllerAlias2 === undefined ? simpleHashCode(callbackMethod.toString()) : undefined;
			const secondCtrl = new UmbObserverController(hostElement, observable, callbackMethod, controllerAlias2);

			expect(hostElement.hasUmbController(firstCtrl)).to.be.true;
			expect(hostElement.hasUmbController(secondCtrl)).to.be.true;
		});
	});
});
