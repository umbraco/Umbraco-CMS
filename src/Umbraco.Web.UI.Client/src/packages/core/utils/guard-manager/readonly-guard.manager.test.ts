import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbReadOnlyGuardManager } from './readonly-guard.manager.js';
import type { UmbGuardRule } from './guard.manager.base.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbReadOnlyGuardManager', () => {
	let manager: UmbReadOnlyGuardManager<UmbGuardRule>;
	const rulePositive = { unique: '1', message: 'Rule 1', permitted: true };
	const ruleNegative = { unique: '-1', message: 'Rule -1', permitted: false };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbReadOnlyGuardManager<UmbGuardRule>(hostElement);
	});

	describe('Rule based outcomes', () => {
		it('is not permitted when there are no rules and fallback defaults to not permitted', (done) => {
			manager.permitted
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted by a single positive rule', (done) => {
			manager.addRule(rulePositive);

			manager.permitted
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('a negative rule wins over a positive rule', (done) => {
			manager.addRules([rulePositive, ruleNegative]);

			manager.permitted
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});

	describe('Fallback', () => {
		it('permitted reacts to late fallback updates when no rules match', () => {
			const emitted: boolean[] = [];
			const subscription = manager.permitted.subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();
			manager.fallbackToNotPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([false, true, false]);
		});

		it('permitted is stable when a matching rule determines the result', () => {
			manager.addRule(rulePositive);
			const emitted: boolean[] = [];
			const subscription = manager.permitted.subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();
			manager.fallbackToNotPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([true]);
		});

		it('getPermitted reflects the current fallback when no rules match', () => {
			expect(manager.getPermitted()).to.equal(false);
			manager.fallbackToPermitted();
			expect(manager.getPermitted()).to.equal(true);
			manager.fallbackToNotPermitted();
			expect(manager.getPermitted()).to.equal(false);
		});

		it('getPermitted is unaffected by the fallback when a rule determines the result', () => {
			manager.addRule(rulePositive);
			expect(manager.getPermitted()).to.equal(true);
			manager.fallbackToNotPermitted();
			expect(manager.getPermitted()).to.equal(true);
		});
	});
});
