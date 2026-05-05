import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNameWriteGuardManager } from './name-write-guard.manager.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbNameWriteGuardManager', () => {
	let manager: UmbNameWriteGuardManager;
	const rulePositive = { unique: '1', message: 'Rule 1', permitted: true };
	const ruleNegative = { unique: '-1', message: 'Rule -1', permitted: false };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbNameWriteGuardManager(hostElement);
	});

	describe('Rule based outcomes', () => {
		it('is not permitted when there are no rules and the fallback is not permitted', (done) => {
			manager
				.isPermittedForName()
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted by a single positive rule', (done) => {
			manager.addRule(rulePositive);

			manager
				.isPermittedForName()
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted by a single negative rule', (done) => {
			manager.addRule(ruleNegative);

			manager
				.isPermittedForName()
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative rule wins over a positive rule', (done) => {
			manager.addRules([rulePositive, ruleNegative]);

			manager
				.isPermittedForName()
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});

	describe('Fallback', () => {
		it('isPermittedForName reacts to late fallback updates when no rules match', () => {
			const emitted: boolean[] = [];
			const subscription = manager.isPermittedForName().subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();
			manager.fallbackToNotPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([false, true, false]);
		});

		it('isPermittedForName is stable when a matching rule determines the result', () => {
			manager.addRule(rulePositive);
			const emitted: boolean[] = [];
			const subscription = manager.isPermittedForName().subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();
			manager.fallbackToNotPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([true]);
		});
	});
});
