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

		it('is not on for a variant when no states', (done) => {
			manager
				.permittedForVariantAndProperty(invariantVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on for present variant', (done) => {
			manager.addRule(ruleEn);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant', (done) => {
			manager.addRule(ruleInv);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant and incompatible property', (done) => {
			manager.addRule(statePropAInv);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
		it('is not on for compatible variant with incompatible property', (done) => {
			manager.addRule(statePropAInv);

			manager
				.permittedForVariantAndProperty(invariantVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant with compatible property', (done) => {
			manager.addRule(statePropAInv);

			manager
				.permittedForVariantAndProperty(englishVariant, propA)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on by generic state', (done) => {
			manager.addRule(rulePlain);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific variant states false', (done) => {
			manager.addRule(rulePlain);
			manager.addRule(ruleNoEn);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when generic variant states false', (done) => {
			manager.addRule(ruleNoPlain);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific state states true', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('a negative specific state wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(ruleEn);
			manager.addRule(ruleNoEn);

			manager
				.permittedForVariantAndProperty(englishVariant, propB)
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
				.permittedForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a specific state wins over general states', (done) => {
			manager.addRule(stateNoPropAPlain);
			manager.addRule(statePropAPlain);
			manager.addRule(statePropAInv);

			manager
				.permittedForVariantAndProperty(invariantVariant, propA)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('a specific negative state wins over general states', (done) => {
			manager.addRule(stateNoPropAPlain);
			manager.addRule(statePropAPlain);
			manager.addRule(stateNoPropAInv);

			manager
				.permittedForVariantAndProperty(invariantVariant, propA)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});
});
