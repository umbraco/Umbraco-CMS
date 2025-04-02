import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbReadOnlyVariantStateManager } from './variant-property-state.manager.js';
import { UmbVariantId } from '../../variant/variant-id.class.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbVariantPropertyStateManager', () => {
	let manager: UmbReadOnlyVariantStateManager;

	const invariantVariant = UmbVariantId.CreateInvariant();
	const englishVariant = UmbVariantId.Create({ culture: 'en', segment: null });
	const propA = { unique: 'propA' };
	const propB = { unique: 'propB' };

	const stateInv = { unique: '1', message: 'State 1', state: true, variantId: invariantVariant };
	const stateEn = { unique: '2', message: 'State 2', state: true, variantId: englishVariant };
	const statePlain = { unique: '3', message: 'State 3', state: true };
	const stateNoInv = { unique: '01', message: 'State 01', state: false, variantId: invariantVariant };
	const stateNoEn = { unique: '02', message: 'State 02', state: false, variantId: englishVariant };
	const stateNoPlain = { unique: '03', message: 'State 03', state: false };

	const statePropAInv = {
		unique: 'a1',
		message: 'State 1',
		state: true,
		variantId: invariantVariant,
		propertyType: propA,
	};
	const statePropBEn = {
		unique: 'b2',
		message: 'State 2',
		state: true,
		variantId: englishVariant,
		propertyType: propB,
	};
	const statePropAPlain = { unique: 'a3', message: 'State 3', state: true, propertyType: propA };
	const stateNoPropAInv = {
		unique: 'a01',
		message: 'State 01',
		state: false,
		variantId: invariantVariant,
		propertyType: propA,
	};
	const stateNoPropBEn = {
		unique: 'b02',
		message: 'State 02',
		state: false,
		variantId: englishVariant,
		propertyType: propB,
	};
	const stateNoPropAPlain = { unique: 'a03', message: 'State 03', state: false, propertyType: propA };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbReadOnlyVariantStateManager(hostElement);
	});

	describe('VariantIds based states', () => {
		it('works with variantIds class instances in the state data.', () => {
			manager.addState(stateInv);
			manager.addState(stateEn);
			expect(manager.getStates()[0].variantId?.compare(invariantVariant)).to.be.true;
			expect(manager.getStates()[0].variantId?.compare(englishVariant)).to.be.false;
			expect(manager.getStates()[1].variantId?.compare(englishVariant)).to.be.true;
			expect(manager.getStates()[1].variantId?.compare(invariantVariant)).to.be.false;
		});

		it('is not generally on when no states', () => {
			expect(manager.getIsOn()).to.be.false;
		});

		it('is generally on when states', () => {
			manager.addState(stateInv);
			expect(manager.getIsOn()).to.be.true;
		});

		it('is not on for a variant when no states', (done) => {
			manager
				.isOnForVariantAndProperty(invariantVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on for present variant', (done) => {
			manager.addState(stateEn);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant', (done) => {
			manager.addState(stateInv);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant and incompatible property', (done) => {
			manager.addState(statePropAInv);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
		it('is not on for compatible variant with incompatible property', (done) => {
			manager.addState(statePropAInv);

			manager
				.isOnForVariantAndProperty(invariantVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant with compatible property', (done) => {
			manager.addState(statePropAInv);

			manager
				.isOnForVariantAndProperty(englishVariant, propA)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on by generic state', (done) => {
			manager.addState(statePlain);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific variant states false', (done) => {
			manager.addState(statePlain);
			manager.addState(stateNoEn);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when generic variant states false', (done) => {
			manager.addState(stateNoPlain);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific state states true', (done) => {
			manager.addState(stateNoPlain);
			manager.addState(stateEn);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('a negative specific state wins', (done) => {
			manager.addState(stateNoPlain);
			manager.addState(stateEn);
			manager.addState(stateNoEn);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative general state wins', (done) => {
			manager.addState(stateNoPlain);
			manager.addState(statePlain);

			manager
				.isOnForVariantAndProperty(englishVariant, propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a specific state wins over general states', (done) => {
			manager.addState(stateNoPropAPlain);
			manager.addState(statePropAPlain);
			manager.addState(statePropAInv);

			manager
				.isOnForVariantAndProperty(invariantVariant, propA)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('a specific negative state wins over general states', (done) => {
			manager.addState(stateNoPropAPlain);
			manager.addState(statePropAPlain);
			manager.addState(stateNoPropAInv);

			manager
				.isOnForVariantAndProperty(invariantVariant, propA)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});
});
