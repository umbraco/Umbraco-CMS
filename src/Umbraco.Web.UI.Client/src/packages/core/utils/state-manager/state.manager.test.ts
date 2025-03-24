import { UmbStateManager } from './state.manager.js';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSelectionManager', () => {
	let manager: UmbStateManager;
	const state1 = { unique: '1', message: 'State 1' };
	const state2 = { unique: '2', message: 'State 2' };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbStateManager(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a states property', () => {
				expect(manager).to.have.property('states').to.be.an.instanceOf(Observable);
			});

			it('has a isOn property', () => {
				expect(manager).to.have.property('isOn').to.be.an.instanceOf(Observable);
			});

			it('has a isOff property', () => {
				expect(manager).to.have.property('isOff').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a start method', () => {
				expect(manager).to.have.property('start').that.is.a('function');
			});

			it('has a stop method', () => {
				expect(manager).to.have.property('stop').that.is.a('function');
			});

			it('has a addState method', () => {
				expect(manager).to.have.property('addState').that.is.a('function');
			});

			it('has a addStates method', () => {
				expect(manager).to.have.property('addStates').that.is.a('function');
			});

			it('has a removeState method', () => {
				expect(manager).to.have.property('removeState').that.is.a('function');
			});

			it('has a getStates method', () => {
				expect(manager).to.have.property('getStates').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(manager).to.have.property('clear').that.is.a('function');
			});

			it('has a getIsOn method', () => {
				expect(manager).to.have.property('getIsOn').that.is.a('function');
			});

			it('has a getIsOff method', () => {
				expect(manager).to.have.property('getIsOff').that.is.a('function');
			});
		});
	});

	describe('Add State', () => {
		it('throws an error if the state does not have a unique property', () => {
			// @ts-ignore - Testing invalid input
			expect(() => manager.addState({ message: 'State 1' })).to.throw();
		});

		it('adds a single state to the states array', () => {
			manager.addState(state1);
			expect(manager.getStates()).to.deep.equal([state1]);
		});

		it('adds multiple states to the states array', () => {
			manager.addStates([state1, state2]);
			expect(manager.getStates()).to.deep.equal([state1, state2]);
		});

		it('updates the observable', (done) => {
			manager.addState(state1);

			manager.states
				.subscribe((value) => {
					expect(value[0]).to.equal(state1);
					done();
				})
				.unsubscribe();
		});

		it('throws an error if the state manager is not running', () => {
			manager.stop();
			expect(() => manager.addState(state1)).to.throw();
		});
	});

	describe('Remove State', () => {
		beforeEach(() => {
			manager.addStates([state1, state2]);
		});

		it('removes a single state from the states array', () => {
			manager.removeState('1');
			expect(manager.getStates()).to.deep.equal([state2]);
		});

		it('removes multiple states from the states array', () => {
			manager.removeStates(['1', '2']);
			expect(manager.getStates()).to.deep.equal([]);
		});

		it('updates the observable', (done) => {
			manager.removeState('1');

			manager.states
				.subscribe((value) => {
					expect(value).to.deep.equal([state2]);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Get States', () => {
		it('returns all states', () => {
			manager.addStates([state1, state2]);
			expect(manager.getStates()).to.deep.equal([state1, state2]);
		});
	});

	describe('Clear', () => {
		beforeEach(() => {
			manager.addStates([state1, state2]);
		});

		it('clears all states', () => {
			manager.clear();
			expect(manager.getStates()).to.deep.equal([]);
		});

		it('updates the observable', (done) => {
			manager.clear();

			manager.states
				.subscribe((value) => {
					expect(value).to.deep.equal([]);
					done();
				})
				.unsubscribe();
		});
	});

	describe('getIsOn', () => {
		it('returns true if there are states', () => {
			manager.addState(state1);
			expect(manager.getIsOn()).to.be.true;
		});

		it('returns false if there are no states', () => {
			expect(manager.getIsOn()).to.be.false;
		});
	});

	describe('getIsOff', () => {
		it('returns true if there are no states', () => {
			expect(manager.getIsOff()).to.be.true;
		});

		it('returns false if there are states', () => {
			manager.addState(state1);
			expect(manager.getIsOff()).to.be.false;
		});
	});

	describe('start', () => {
		it('starts the state manager', () => {
			manager.stop();
			manager.start();
			expect(manager.getIsRunning()).to.be.true;
		});
	});

	describe('stop', () => {
		it('stops the state manager', () => {
			manager.stop();
			expect(manager.getIsRunning()).to.be.false;
		});
	});
});
