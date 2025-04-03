import { UmbGuardManagerBase, type UmbGuardRuleEntry } from './guard.manager.base.js';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbPermissionGuardManager', () => {
	let manager: UmbGuardManagerBase;
	const rule1: UmbGuardRuleEntry = { unique: '1', message: 'Rule 1', permitted: true };
	const rule2: UmbGuardRuleEntry = { unique: '2', message: 'Rule 2', permitted: true };
	const ruleFalse: UmbGuardRuleEntry = { unique: '-1', message: 'Rule -1', permitted: false };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbGuardManagerBase(hostElement);
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a fallbackToDisallowed method', () => {
				expect(manager).to.have.property('fallbackToDisallowed').that.is.a('function');
			});

			it('has a fallbackToPermitted method', () => {
				expect(manager).to.have.property('fallbackToPermitted').that.is.a('function');
			});

			it('has a addRule method', () => {
				expect(manager).to.have.property('addRule').that.is.a('function');
			});

			it('has a addRules method', () => {
				expect(manager).to.have.property('addRules').that.is.a('function');
			});

			it('has a removeRule method', () => {
				expect(manager).to.have.property('removeRule').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(manager).to.have.property('clear').that.is.a('function');
			});
		});
	});

	describe('Add Rule', () => {
		it('adds a single state to the states array', () => {
			manager.addRule(rule1);
			expect(manager.getRules()).to.deep.equal([rule1]);
		});

		it('adds multiple states to the states array', () => {
			manager.addRules([rule1, rule2]);
			expect(manager.getRules()).to.deep.equal([rule1, rule2]);
		});

		it('updates the observable', (done) => {
			manager.addRule(rule1);

			manager.rules
				.subscribe((value) => {
					expect(value[0]).to.deep.equal(rule1);
					done();
				})
				.unsubscribe();
		});

		it('sort negative states first', () => {
			manager.addRules([rule1, ruleFalse, rule2]);
			expect(manager.getRules()).to.deep.equal([ruleFalse, rule1, rule2]);
		});
	});

	describe('Remove State', () => {
		beforeEach(() => {
			manager.addRules([rule1, rule2]);
		});

		it('removes a single state from the states array', () => {
			manager.removeRule('1');
			expect(manager.getRules()).to.deep.equal([rule2]);
		});

		it('removes multiple states from the states array', () => {
			manager.removeRules(['1', '2']);
			expect(manager.getRules()).to.deep.equal([]);
		});

		it('updates the observable', (done) => {
			manager.removeRule('1');

			manager.rules
				.subscribe((value) => {
					expect(value).to.deep.equal([rule2]);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Get States', () => {
		it('returns all states', () => {
			manager.addRules([rule1, rule2]);
			expect(manager.getRules()).to.deep.equal([rule1, rule2]);
		});
	});

	describe('Clear', () => {
		beforeEach(() => {
			manager.addRules([rule1, rule2]);
		});

		it('clears all states', () => {
			manager.clear();
			expect(manager.getRules()).to.deep.equal([]);
		});

		it('updates the observable', (done) => {
			manager.clear();

			manager.rules
				.subscribe((value) => {
					expect(value).to.deep.equal([]);
					done();
				})
				.unsubscribe();
		});
	});
});
