import { UmbEntityStateManager, type UmbEntityStateEntry } from './entity-state.manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbEntityStateManager', () => {
	let manager: UmbEntityStateManager;
	const stateA: UmbEntityStateEntry = { unique: 'a', label: 'State A' };
	const stateB: UmbEntityStateEntry = { unique: 'b', label: 'State B' };
	const stateC: UmbEntityStateEntry = { unique: 'c', label: 'State C' };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbEntityStateManager(hostElement);
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has an addState method', () => {
				expect(manager).to.have.property('addState').that.is.a('function');
			});

			it('has an addStates method', () => {
				expect(manager).to.have.property('addStates').that.is.a('function');
			});

			it('has a removeState method', () => {
				expect(manager).to.have.property('removeState').that.is.a('function');
			});

			it('has a removeStates method', () => {
				expect(manager).to.have.property('removeStates').that.is.a('function');
			});

			it('has a getStates method', () => {
				expect(manager).to.have.property('getStates').that.is.a('function');
			});

			it('has a replaceStates method', () => {
				expect(manager).to.have.property('replaceStates').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(manager).to.have.property('clear').that.is.a('function');
			});
		});
	});

	describe('Add State', () => {
		it('adds a single state to the states array', () => {
			manager.addState(stateA);
			expect(manager.getStates()).to.deep.equal([stateA]);
		});

		it('adds multiple states to the states array', () => {
			manager.addStates([stateA, stateB]);
			expect(manager.getStates()).to.deep.equal([stateA, stateB]);
		});

		it('upserts by unique — adding a repeat unique does not throw and replaces the existing entry', () => {
			manager.addState(stateA);
			const updatedStateA: UmbEntityStateEntry = { unique: 'a', label: 'Updated State A' };
			expect(() => manager.addState(updatedStateA)).to.not.throw();
			expect(manager.getStates()).to.deep.equal([updatedStateA]);
		});

		it('updates the observable', (done) => {
			manager.addState(stateA);

			manager.states
				.subscribe((value) => {
					expect(value[0]).to.deep.equal(stateA);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Remove State', () => {
		beforeEach(() => {
			manager.addStates([stateA, stateB]);
		});

		it('removes a single state from the states array', () => {
			manager.removeState('a');
			expect(manager.getStates()).to.deep.equal([stateB]);
		});

		it('removes multiple states from the states array', () => {
			manager.removeStates(['a', 'b']);
			expect(manager.getStates()).to.deep.equal([]);
		});

		it('updates the observable', (done) => {
			manager.removeState('a');

			manager.states
				.subscribe((value) => {
					expect(value).to.deep.equal([stateB]);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Get States', () => {
		it('returns all states', () => {
			manager.addStates([stateA, stateB]);
			expect(manager.getStates()).to.deep.equal([stateA, stateB]);
		});

		it('orders states by weight descending', () => {
			manager.addStates([
				{ unique: 'low', label: 'Low', weight: 1 },
				{ unique: 'high', label: 'High', weight: 100 },
				{ unique: 'mid', label: 'Mid', weight: 50 },
			]);

			expect(manager.getStates().map((s) => s.unique)).to.deep.equal(['high', 'mid', 'low']);
		});

		it('keeps insertion order for equal-weight states', () => {
			manager.addStates([
				{ unique: 'first', label: 'First', weight: 10 },
				{ unique: 'second', label: 'Second', weight: 10 },
			]);

			expect(manager.getStates().map((s) => s.unique)).to.deep.equal(['first', 'second']);
		});

		it('defaults a missing weight to 0', () => {
			manager.addStates([
				{ unique: 'noWeight', label: 'No weight' },
				{ unique: 'negative', label: 'Negative', weight: -1 },
			]);

			expect(manager.getStates().map((s) => s.unique)).to.deep.equal(['noWeight', 'negative']);
		});

		it('does not mutate the underlying states between calls', () => {
			manager.addStates([stateB, stateA]);
			manager.getStates();
			expect(manager.getStates()).to.deep.equal([stateB, stateA]);
		});

		it('the states observable is weight-sorted too', (done) => {
			manager.addStates([
				{ unique: 'low', label: 'Low', weight: 1 },
				{ unique: 'high', label: 'High', weight: 100 },
			]);

			manager.states
				.subscribe((value) => {
					expect(value.map((s) => s.unique)).to.deep.equal(['high', 'low']);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Replace States', () => {
		beforeEach(() => {
			manager.addStates([stateA, stateB, stateC]);
		});

		it('removes only predicate-matching states and adds the new ones', () => {
			manager.replaceStates(
				(s) => s.unique === 'a' || s.unique === 'b',
				[{ unique: 'a2', label: 'Replacement for A' }],
			);

			expect(manager.getStates()).to.deep.equal([stateC, { unique: 'a2', label: 'Replacement for A' }]);
		});

		it('is a pure append when the predicate matches nothing', () => {
			manager.replaceStates(
				(s) => s.unique === 'does-not-exist',
				[{ unique: 'd', label: 'State D' }],
			);

			expect(manager.getStates()).to.deep.equal([stateA, stateB, stateC, { unique: 'd', label: 'State D' }]);
		});

		it('is a pure replace when the predicate matches everything', () => {
			manager.replaceStates(
				() => true,
				[{ unique: 'only', label: 'Only state' }],
			);

			expect(manager.getStates()).to.deep.equal([{ unique: 'only', label: 'Only state' }]);
		});
	});

	describe('Clear', () => {
		beforeEach(() => {
			manager.addStates([stateA, stateB]);
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
});
