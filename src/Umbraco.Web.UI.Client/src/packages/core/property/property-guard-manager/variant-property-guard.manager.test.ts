import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantPropertyGuardManager } from './variant-property-guard.manager.js';
import { UmbVariantId } from '../../variant/variant-id.class.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbVariantPropertyGuardManager', () => {
	let manager: UmbVariantPropertyGuardManager;

	const invariantVariant = UmbVariantId.CreateInvariant();
	const englishVariant = UmbVariantId.Create({ culture: 'en', segment: null });
	const propA = { unique: 'propA' };
	const propB = { unique: 'propB' };

	const ruleInv = { unique: '1', message: 'State 1', permitted: true, variantId: invariantVariant };
	const ruleEn = { unique: '2', message: 'State 2', permitted: true, variantId: englishVariant };
	const rulePlain = { unique: '3', message: 'State 3', permitted: true };
	const ruleNoInv = { unique: '01', message: 'State 01', permitted: false, variantId: invariantVariant };
	const ruleNoEn = { unique: '02', message: 'State 02', permitted: false, variantId: englishVariant };
	const ruleNoPlain = { unique: '03', message: 'State 03', permitted: false };

	const statePropAInv = {
		unique: 'a1',
		message: 'State 1',
		permitted: true,
		variantId: invariantVariant,
		propertyType: propA,
	};
	const statePropBEn = {
		unique: 'b2',
		message: 'State 2',
		permitted: true,
		variantId: englishVariant,
		propertyType: propB,
	};
	const statePropAPlain = { unique: 'a3', message: 'State 3', permitted: true, propertyType: propA };
	const stateNoPropAInv = {
		unique: 'a01',
		message: 'State 01',
		permitted: false,
		variantId: invariantVariant,
		propertyType: propA,
	};
	const stateNoPropBEn = {
		unique: 'b02',
		message: 'State 02',
		permitted: false,
		variantId: englishVariant,
		propertyType: propB,
	};
	const stateNoPropAPlain = { unique: 'a03', message: 'State 03', permitted: false, propertyType: propA };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbVariantPropertyGuardManager(hostElement);
	});

	describe('VariantIds based states', () => {
		it('works with variantIds class instances in the state data.', () => {
			manager.addRule(ruleInv);
			manager.addRule(ruleEn);
			expect(manager.getRules()[0].variantId?.compare(invariantVariant)).to.be.true;
			expect(manager.getRules()[0].variantId?.compare(englishVariant)).to.be.false;
			expect(manager.getRules()[1].variantId?.compare(englishVariant)).to.be.true;
			expect(manager.getRules()[1].variantId?.compare(invariantVariant)).to.be.false;
		});

		it('is not permitted for a variant when no states', (done) => {
			manager
				.isPermittedForVariantAndProperty(invariantVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted for present variant', (done) => {
			manager.addRule(ruleEn);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted for incompatible variant', (done) => {
			manager.addRule(ruleInv);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted for incompatible variant and incompatible property', (done) => {
			manager.addRule(statePropAInv);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
		it('is not permitted for compatible variant with incompatible property', (done) => {
			manager.addRule(statePropAInv);

			manager
				.isPermittedForVariantAndProperty(invariantVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted for incompatible variant with compatible property', (done) => {
			manager.addRule(statePropAInv);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propA, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted by generic state', (done) => {
			manager.addRule(rulePlain);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted when specific variant states false', (done) => {
			manager.addRule(rulePlain);
			manager.addRule(ruleNoEn);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted when generic variant states false', (done) => {
			manager.addRule(ruleNoPlain);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted despite a specific state is permitting', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative specific state wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);
			manager.addRule(ruleNoEn);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative general state wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(rulePlain);

			manager
				.isPermittedForVariantAndProperty(englishVariant, propB, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a specific state does not win over a general negative rule', (done) => {
			manager.addRule(stateNoPropAPlain);
			manager.addRule(statePropAPlain);
			manager.addRule(statePropAInv);

			manager
				.isPermittedForVariantAndProperty(invariantVariant, propA, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a specific negative state wins over general permitting rule', (done) => {
			manager.addRule(stateNoPropAPlain);
			manager.addRule(statePropAPlain);
			manager.addRule(stateNoPropAInv);

			manager
				.isPermittedForVariantAndProperty(invariantVariant, propA, invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});
});
