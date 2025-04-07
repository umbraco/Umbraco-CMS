import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbReadonlyVariantGuardManager } from './readonly-variant-guard.manager.js';
import { UmbVariantId } from '../../variant/variant-id.class.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbReadonlyVariantGuardManager', () => {
	let manager: UmbReadonlyVariantGuardManager;
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
		manager = new UmbReadonlyVariantGuardManager(hostElement);
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

		it('is not on for a variant when no rules', (done) => {
			manager
				.permittedForVariant(invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on for present variant', (done) => {
			manager.addRule(ruleEn);

			manager
				.permittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant', (done) => {
			manager.addRule(ruleInv);

			manager
				.permittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on by generic rule', (done) => {
			manager.addRule(rulePlain);

			manager
				.permittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific permitted does not permit', (done) => {
			manager.addRule(rulePlain);
			manager.addRule(ruleNoEn);

			manager
				.permittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when generic permitted does not permit', (done) => {
			manager.addRule(ruleNoPlain);

			manager
				.permittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific rule permits', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);

			manager
				.permittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('a negative specific rule wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);
			manager.addRule(ruleNoEn);

			manager
				.permittedForVariant(englishVariant)
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
				.permittedForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});
});
