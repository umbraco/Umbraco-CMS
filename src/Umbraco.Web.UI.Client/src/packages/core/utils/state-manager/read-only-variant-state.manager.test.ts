import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbReadOnlyVariantStateManager } from './read-only-variant-state.manager.js';
import { UmbVariantId } from '../../variant/variant-id.class.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSelectionManager', () => {
	let manager: UmbReadOnlyVariantStateManager;
	const invariantVariant = UmbVariantId.CreateInvariant();
	const englishVariant = UmbVariantId.Create({ culture: 'en', segment: null });
	const stateInv = { unique: '1', message: 'State 1', state: true, variantId: invariantVariant };
	const stateEn = { unique: '2', message: 'State 2', state: true, variantId: englishVariant };
	const statePlain = { unique: '3', message: 'State 3', state: true };
	const stateNoInv = { unique: '01', message: 'State 01', state: false, variantId: invariantVariant };
	const stateNoEn = { unique: '02', message: 'State 02', state: false, variantId: englishVariant };
	const stateNoPlain = { unique: '03', message: 'State 03', state: false };

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
				.isOnForVariant(invariantVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on for present variant', (done) => {
			manager.addState(stateEn);

			manager
				.isOnForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant', (done) => {
			manager.addState(stateInv);

			manager
				.isOnForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on by generic state', (done) => {
			manager.addState(statePlain);

			manager
				.isOnForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific state states false', (done) => {
			manager.addState(statePlain);
			manager.addState(stateNoEn);

			manager
				.isOnForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when generic state states false', (done) => {
			manager.addState(stateNoPlain);

			manager
				.isOnForVariant(englishVariant)
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
				.isOnForVariant(englishVariant)
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
				.isOnForVariant(englishVariant)
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
				.isOnForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});
});
