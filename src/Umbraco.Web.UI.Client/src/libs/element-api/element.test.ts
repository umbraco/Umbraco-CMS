import { expect } from '@open-wc/testing';
import { UmbElementMixin } from './element.mixin.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

@customElement('test-my-umb-element')
class UmbTestUmbElement extends UmbElementMixin(HTMLElement) {}

describe('UmbElementMixin', () => {
	let hostElement: UmbTestUmbElement;

	beforeEach(() => {
		hostElement = document.createElement('test-my-umb-element') as UmbTestUmbElement;
	});

	describe('Element general controller API', () => {
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
			it('has an destroy method', () => {
				expect(hostElement).to.have.property('destroy').that.is.a('function');
			});
		});
	});

	describe('Element helper methods API', () => {
		describe('methods', () => {
			it('has an hasController method', () => {
				expect(hostElement).to.have.property('getHostElement').that.is.a('function');
			});
			it('has an hasController should return it self', () => {
				expect(hostElement.getHostElement()).to.be.equal(hostElement);
			});
			it('has an observe method', () => {
				expect(hostElement).to.have.property('observe').that.is.a('function');
			});
			it('has an provideContext method', () => {
				expect(hostElement).to.have.property('provideContext').that.is.a('function');
			});
			it('has an consumeContext method', () => {
				expect(hostElement).to.have.property('consumeContext').that.is.a('function');
			});
			it('has an getContext method', () => {
				expect(hostElement).to.have.property('getContext').that.is.a('function');
			});
			it('has an localization class instance', () => {
				expect(hostElement).to.have.property('localize').that.is.a('object');
			});
		});
	});

	describe('Controllers lifecycle', () => {
		it('observe is removed when destroyed', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			ctrl.destroy();

			// The controller is removed from the host:
			expect(hostElement.hasController(ctrl)).to.be.false;
		});

		it('observe is destroyed then removed', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			hostElement.removeController(ctrl);

			// The controller is removed from the host:
			expect(hostElement.hasController(ctrl)).to.be.false;
		});

		it('observe is destroyed then removed via alias', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			hostElement.removeControllerByAlias('observer');

			// The controller is removed from the host:
			expect(hostElement.hasController(ctrl)).to.be.false;
		});

		it('observe is removed when replaced with alias', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is removed from the host:
			expect(hostElement.hasController(ctrl)).to.be.false;
			// The controller is new one is there instead:
			expect(hostElement.hasController(ctrl2)).to.be.true;
		});

		it('observe is removed when replaced with alias made of hash of callback method', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {});

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(myObservable, () => {});

			// The controller is removed from the host:
			expect(hostElement.hasController(ctrl)).to.be.false;
			// The controller is new one is there instead:
			expect(hostElement.hasController(ctrl2)).to.be.true;
		});

		it('observe is NOT removed when controller alias does not align', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {});

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(myObservable, (value) => {
				const a = value + 'bla';
			});

			// The controller is not removed from the host:
			expect(hostElement.hasController(ctrl)).to.be.true;
			expect(hostElement.hasController(ctrl2)).to.be.true;
		});

		it('observe is removed when observer is undefined and using the same alias', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(
				undefined,
				() => {
					const a = 1;
				},
				'observer',
			);

			// The controller is removed from the host, and the new one was NOT added:
			expect(hostElement.hasController(ctrl)).to.be.false;
			expect(ctrl2).to.be.undefined;
		});

		it('observe is removed when observer is undefined and using the same callback method', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {});

			// The controller is now added to the host:
			expect(hostElement.hasController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(undefined, () => {});

			// The controller is removed from the host, and the new one was NOT added:
			expect(hostElement.hasController(ctrl)).to.be.false;
			expect(ctrl2).to.be.undefined;
		});

		it('an undefined observer executes the callback method with undefined', () => {
			let callbackWasCalled = false;
			const ctrl = hostElement.observe(undefined, (value) => {
				expect(value).to.be.undefined;
				callbackWasCalled = true;
			});
			expect(callbackWasCalled).to.be.true;
			expect(ctrl).to.be.undefined;
			expect(hostElement.hasController(ctrl)).to.be.false;
		});
	});
});
