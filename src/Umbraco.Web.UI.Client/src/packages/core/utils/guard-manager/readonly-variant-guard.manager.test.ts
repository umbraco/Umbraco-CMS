import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UmbReadOnlyVariantGuardManager } from './readonly-variant-guard.manager.js';
import { UmbVariantId } from '../../variant/variant-id.class.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbReadOnlyVariantGuardManager', () => {
	let manager: UmbReadOnlyVariantGuardManager;
	const invariantVariant = UmbVariantId.CreateInvariant();
	const englishVariant = UmbVariantId.Create({ culture: 'en', segment: null });
	const ruleInv = { unique: '1', message: 'State 1', permitted: true, variantId: invariantVariant };
	const ruleEn = { unique: '2', message: 'State 2', permitted: true, variantId: englishVariant };
	const rulePlain = { unique: '3', message: 'State 3', permitted: true };
	const ruleNoInv = { unique: '01', message: 'State 01', permitted: false, variantId: invariantVariant };
	const ruleNoEn = { unique: '02', message: 'State 02', permitted: false, variantId: englishVariant };
	const ruleNoPlain = { unique: '03', message: 'State 03', permitted: false };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbReadOnlyVariantGuardManager(hostElement);
	});

	describe('VariantIds based rules', () => {
		it('works with variantIds class instances in the rule data.', () => {
			manager.addRule(ruleInv);
			manager.addRule(ruleEn);
			expect(manager.getRules()[0].variantId?.compare(invariantVariant)).to.be.true;
			expect(manager.getRules()[0].variantId?.compare(englishVariant)).to.be.false;
			expect(manager.getRules()[1].variantId?.compare(englishVariant)).to.be.true;
			expect(manager.getRules()[1].variantId?.compare(invariantVariant)).to.be.false;
		});

		it('is not permitted for a variant when no rules', (done) => {
			manager
				.isPermittedForVariant(invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted for present variant', (done) => {
			manager.addRule(ruleEn);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted for incompatible variant', (done) => {
			manager.addRule(ruleInv);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted by generic rule', (done) => {
			manager.addRule(rulePlain);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted when specific permitted does not permit', (done) => {
			manager.addRule(rulePlain);
			manager.addRule(ruleNoEn);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted when generic permitted does not permit', (done) => {
			manager.addRule(ruleNoPlain);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted despite a specific rule permits', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative specific rule wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);
			manager.addRule(ruleNoEn);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative general rule wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(rulePlain);

			manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});

	describe('Fallback', () => {
		it('isPermittedForVariant reacts to late fallback updates when no rules match', () => {
			const emitted: boolean[] = [];
			const subscription = manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();
			manager.fallbackToNotPermitted();
			manager.fallbackToPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([false, true, false, true]);
		});

		it('isPermittedForVariant is stable when a matching rule determines the result', () => {
			manager.addRule(ruleEn);
			const emitted: boolean[] = [];
			const subscription = manager
				.isPermittedForVariant(englishVariant)
				.subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();
			manager.fallbackToNotPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([true]);
		});

		it('isPermittedForVariant uses late fallback updates for variants without a matching rule', () => {
			manager.addRule(ruleEn);
			const emitted: boolean[] = [];
			const subscription = manager
				.isPermittedForVariant(invariantVariant)
				.subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([false, true]);
		});

		it('getIsPermittedForVariant reflects the current fallback when no rules match', () => {
			expect(manager.getIsPermittedForVariant(englishVariant)).to.equal(false);
			manager.fallbackToPermitted();
			expect(manager.getIsPermittedForVariant(englishVariant)).to.equal(true);
			manager.fallbackToNotPermitted();
			expect(manager.getIsPermittedForVariant(englishVariant)).to.equal(false);
		});

		it('isPermittedForObservableVariant reacts to late fallback updates when no rules match', () => {
			const variantIdState = new UmbBasicState<UmbVariantId | undefined>(englishVariant);
			const emitted: Array<boolean | undefined> = [];
			const subscription = manager
				.isPermittedForObservableVariant(variantIdState.asObservable())
				.subscribe((value) => emitted.push(value));

			manager.fallbackToPermitted();
			manager.fallbackToNotPermitted();

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([false, true, false]);
		});
	});
});
