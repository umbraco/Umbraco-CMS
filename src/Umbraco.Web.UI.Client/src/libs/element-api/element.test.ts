import { UmbElementMixin } from './element.mixin.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { type UmbObserverController, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

@customElement('test-my-umb-element')
class UmbTestUmbElement extends UmbElementMixin(HTMLElement) {}

describe('UmbElementMixin', () => {
	let hostElement: UmbTestUmbElement;

	beforeEach(() => {
		hostElement = document.createElement('test-my-umb-element') as UmbTestUmbElement;
	});

	describe('Element general controller API', () => {
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
			it('has an destroy method', () => {
				expect(hostElement).to.have.property('destroy').that.is.a('function');
			});
		});
	});

	describe('Element helper methods API', () => {
		describe('methods', () => {
			it('has an hasUmbController method', () => {
				expect(hostElement).to.have.property('getHostElement').that.is.a('function');
			});
			it('has an hasUmbController should return it self', () => {
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
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			ctrl.destroy();

			// The controller is removed from the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
		});

		it('observe is destroyed then removed', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			hostElement.removeUmbController(ctrl);

			// The controller is removed from the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
		});

		it('observe is destroyed then removed via alias', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			hostElement.removeUmbControllerByAlias('observer');

			// The controller is removed from the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
		});

		it('observe is removed when replaced with alias', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is removed from the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
			// The controller is new one is there instead:
			expect(hostElement.hasUmbController(ctrl2)).to.be.true;
		});

		it('observe is removed when replaced with alias made of hash of callback method', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {});

			// The controller is now added to the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(myObservable, () => {});

			// The controller is removed from the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
			// The controller is new one is there instead:
			expect(hostElement.hasUmbController(ctrl2)).to.be.true;
		});

		it('observe is NOT removed when controller alias does not align', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {});

			// The controller is now added to the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(myObservable, (value) => {
				// eslint-disable-next-line @typescript-eslint/no-unused-vars
				const a = value + 'bla';
			});

			// The controller is not removed from the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;
			expect(hostElement.hasUmbController(ctrl2)).to.be.true;
		});

		it('observe is removed when observer is undefined and using the same alias', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {}, 'observer');

			// The controller is now added to the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(
				undefined,
				() => {
					// eslint-disable-next-line @typescript-eslint/no-unused-vars
					const a = 1;
				},
				'observer',
			);

			// The controller is removed from the host, and the new one was NOT added:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
			expect(ctrl2).to.be.undefined;
		});

		it('observe is removed when observer is undefined and using the same callback method', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, () => {});

			// The controller is now added to the host:
			expect(hostElement.hasUmbController(ctrl)).to.be.true;

			const ctrl2 = hostElement.observe(undefined, () => {});

			// The controller is removed from the host, and the new one was NOT added:
			expect(hostElement.hasUmbController(ctrl)).to.be.false;
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
		});
	});
	describe('Observe types', () => {
		// Type helpers for TSC Type Checking:
		type CheckType<T, ExpectedType> = T extends ExpectedType ? ExpectedType : never;
		type ReverseCheckType<T, ExpectedType> = T extends ExpectedType ? never : T;

		it('observes Observable of String with corresponding callback method value type', () => {
			const myState = new UmbStringState('hello');
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, (value) => {
				type A = typeof value;
				const check: CheckType<A, string> = value;
				const check2: ReverseCheckType<A, undefined> = value;
				expect(check).to.be.equal('hello');
				expect(check2).to.be.equal('hello');
			});
			const check: CheckType<typeof ctrl, UmbObserverController<string>> = ctrl;
			const check2: ReverseCheckType<typeof ctrl, UmbObserverController<undefined>> = ctrl;

			expect(hostElement.hasUmbController(check)).to.be.true;
			expect(check === check2).to.be.true; // Just to use the const for something.
		});

		it('observes Observable of String and Undefined with corresponding callback method value type', () => {
			const myState = new UmbStringState(undefined);
			const myObservable = myState.asObservable();

			const ctrl = hostElement.observe(myObservable, (value) => {
				type A = typeof value;
				const check: CheckType<A, string | undefined> = value;
				expect(check).to.be.undefined;
			});
			const check: CheckType<typeof ctrl, UmbObserverController<string | undefined>> = ctrl;
			const check2: ReverseCheckType<typeof ctrl, UmbObserverController<undefined>> = ctrl;
			const check3: ReverseCheckType<typeof ctrl, UmbObserverController<string>> = ctrl;

			expect(hostElement.hasUmbController(check)).to.be.true;
			expect(check2 === check3).to.be.true; // Just to use the const for something.
		});

		it('observes potential undefined Observable of String with corresponding callback method value type', () => {
			let myState: UmbStringState<string> | undefined = undefined;
			// eslint-disable-next-line no-constant-condition
			if (1 === 1) {
				myState = new UmbStringState('hello');
			}
			const myObservable = myState?.asObservable();

			const ctrl = hostElement.observe(myObservable, (value) => {
				type A = typeof value;
				const check: CheckType<A, string | undefined> = value;
				const check2: CheckType<A, string> = value as string;
				const check3: CheckType<A, undefined> = value as undefined;
				expect(check).to.be.equal('hello');
				expect(check2 === check3).to.be.true; // Just to use the const for something.
			});
			const check: CheckType<typeof ctrl, UmbObserverController<string | undefined> | undefined> = ctrl;
			const check2: ReverseCheckType<typeof ctrl, UmbObserverController<undefined>> = ctrl;
			const check3: ReverseCheckType<typeof ctrl, UmbObserverController<string>> = ctrl;

			if (ctrl) {
				expect(hostElement.hasUmbController(ctrl)).to.be.true;
			} else {
				expect(ctrl).to.be.undefined;
			}
			expect(check === check3 && check2 === check3).to.be.true; // Just to use the const for something.
		});

		it('observes potential undefined Observable of String and Null with corresponding callback method value type', () => {
			let myState: UmbStringState<null> | undefined = undefined;
			// eslint-disable-next-line no-constant-condition
			if (1 === 1) {
				myState = new UmbStringState(null);
			}
			const myObservable = myState?.asObservable();

			const ctrl = hostElement.observe(myObservable, (value) => {
				type A = typeof value;
				const check: CheckType<A, string | null | undefined> = value;
				const check2: CheckType<A, string> = value as string;
				const check3: CheckType<A, null> = value as null;
				const check4: CheckType<A, undefined> = value as undefined;
				expect(check).to.be.equal(null);
				expect(check2 === check3 && check2 === check4).to.be.true; // Just to use the const for something.
			});
			// Because the source is potentially undefined, the controller could be undefined and the value of the callback method could be undefined [NL]
			const check: CheckType<typeof ctrl, UmbObserverController<string | null | undefined> | undefined> = ctrl;
			const check2: ReverseCheckType<typeof ctrl, UmbObserverController<string>> = ctrl;
			const check3: ReverseCheckType<typeof ctrl, UmbObserverController<null>> = ctrl;
			const check4: ReverseCheckType<typeof ctrl, UmbObserverController<undefined>> = ctrl;

			if (ctrl) {
				expect(hostElement.hasUmbController(ctrl)).to.be.true;
			} else {
				expect(ctrl).to.be.undefined;
			}
			expect(check === check2 && check3 === check4).to.be.true; // Just to use the const for something.
		});
	});
});
