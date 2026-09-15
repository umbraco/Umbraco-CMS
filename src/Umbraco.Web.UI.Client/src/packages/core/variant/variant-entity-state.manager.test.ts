import { UmbVariantId } from './variant-id.class.js';
import { UmbVariantEntityStateManager, type UmbVariantEntityStateEntry } from './variant-entity-state.manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbVariantEntityStateManager', () => {
	let manager: UmbVariantEntityStateManager;
	const invariantVariant = UmbVariantId.CreateInvariant();
	const englishVariant = UmbVariantId.Create({ culture: 'en', segment: null });
	const danishVariant = UmbVariantId.Create({ culture: 'da', segment: null });

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbVariantEntityStateManager(hostElement);
	});

	describe('getStatesForVariant', () => {
		it('a universal entry (no variantId) matches every variant', () => {
			const universal: UmbVariantEntityStateEntry = { unique: 'trashed', label: 'Trashed' };
			manager.addState(universal);

			expect(manager.getStatesForVariant(englishVariant)).to.deep.equal([universal]);
			expect(manager.getStatesForVariant(danishVariant)).to.deep.equal([universal]);
			expect(manager.getStatesForVariant(invariantVariant)).to.deep.equal([universal]);
		});

		it('a variant-specific entry matches only its own variant', () => {
			const englishOnly: UmbVariantEntityStateEntry = { unique: 'en-state', label: 'English', variantId: englishVariant };
			manager.addState(englishOnly);

			expect(manager.getStatesForVariant(englishVariant)).to.deep.equal([englishOnly]);
			expect(manager.getStatesForVariant(danishVariant)).to.deep.equal([]);
		});

		it('returns multiple simultaneous entries for the same variant, not just one winner', () => {
			const universal: UmbVariantEntityStateEntry = { unique: 'trashed', label: 'Trashed', weight: 100 };
			const englishOnly: UmbVariantEntityStateEntry = {
				unique: 'en-state',
				label: 'Published',
				weight: 50,
				variantId: englishVariant,
			};
			manager.addStates([universal, englishOnly]);

			expect(manager.getStatesForVariant(englishVariant)).to.deep.equal([universal, englishOnly]);
		});

		it('orders results by weight descending, honoring the inherited getStates', () => {
			const low: UmbVariantEntityStateEntry = { unique: 'low', label: 'Low', weight: 1, variantId: englishVariant };
			const high: UmbVariantEntityStateEntry = { unique: 'high', label: 'High', weight: 100, variantId: englishVariant };
			manager.addStates([low, high]);

			expect(manager.getStatesForVariant(englishVariant).map((s) => s.unique)).to.deep.equal(['high', 'low']);
		});

		it('returns an empty array when nothing matches', () => {
			manager.addState({ unique: 'en-state', label: 'English', variantId: englishVariant });

			expect(manager.getStatesForVariant(danishVariant)).to.deep.equal([]);
		});
	});

	describe('statesForVariant', () => {
		it('emits the same result as getStatesForVariant, live', (done) => {
			const universal: UmbVariantEntityStateEntry = { unique: 'trashed', label: 'Trashed' };
			manager.addState(universal);

			manager
				.statesForVariant(englishVariant)
				.subscribe((value) => {
					expect(value).to.deep.equal(manager.getStatesForVariant(englishVariant));
					done();
				})
				.unsubscribe();
		});

		it('only matches its own variant, not others', (done) => {
			manager.addState({ unique: 'en-state', label: 'English', variantId: englishVariant });

			manager
				.statesForVariant(danishVariant)
				.subscribe((value) => {
					expect(value).to.deep.equal([]);
					done();
				})
				.unsubscribe();
		});

		it('re-emits when the registry changes', () => {
			const emitted: Array<Array<UmbVariantEntityStateEntry>> = [];
			const subscription = manager.statesForVariant(englishVariant).subscribe((value) => emitted.push(value));

			const englishOnly: UmbVariantEntityStateEntry = { unique: 'en-state', label: 'English', variantId: englishVariant };
			manager.addState(englishOnly);

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([[], [englishOnly]]);
		});
	});

	describe('getStatesForVariants', () => {
		it('returns each variantId paired with its matching states, matching getStatesForVariant per variant', () => {
			const universal: UmbVariantEntityStateEntry = { unique: 'trashed', label: 'Trashed' };
			const englishOnly: UmbVariantEntityStateEntry = { unique: 'en-state', label: 'English', variantId: englishVariant };
			manager.addStates([universal, englishOnly]);

			expect(manager.getStatesForVariants([englishVariant, danishVariant])).to.deep.equal([
				{ variantId: englishVariant, states: [universal, englishOnly] },
				{ variantId: danishVariant, states: [universal] },
			]);
		});

		it('returns an empty array for an empty variantId list', () => {
			manager.addState({ unique: 'trashed', label: 'Trashed' });

			expect(manager.getStatesForVariants([])).to.deep.equal([]);
		});
	});

	describe('replaceStates scoped by a producer-owned prefix', () => {
		it('drops only that producer\'s stale variant entries, leaving other producers and other variants untouched', () => {
			const trashed: UmbVariantEntityStateEntry = { unique: 'trashed', label: 'Trashed' };
			const oldEnglishPublishState: UmbVariantEntityStateEntry = {
				unique: 'UMB_PUBLISH_STATE_en',
				label: 'Draft',
				variantId: englishVariant,
			};
			const danishPublishState: UmbVariantEntityStateEntry = {
				unique: 'UMB_PUBLISH_STATE_da',
				label: 'Published',
				variantId: danishVariant,
			};
			manager.addStates([trashed, oldEnglishPublishState, danishPublishState]);

			const newEnglishPublishState: UmbVariantEntityStateEntry = {
				unique: 'UMB_PUBLISH_STATE_en',
				label: 'Published',
				variantId: englishVariant,
			};
			manager.replaceStates(
				(entry) => entry.unique.toString().startsWith('UMB_PUBLISH_STATE_en'),
				[newEnglishPublishState],
			);

			expect(manager.getStates()).to.deep.equal([trashed, danishPublishState, newEnglishPublishState]);
		});
	});

	describe('statesForVariants', () => {
		it('emits resolved states per variant, and re-emits when the registry changes', (done) => {
			const universal: UmbVariantEntityStateEntry = { unique: 'trashed', label: 'Trashed' };
			manager.addState(universal);

			const emitted: Array<Array<{ variantId: UmbVariantId; states: Array<UmbVariantEntityStateEntry> }>> = [];
			const variantIdsState = new UmbBasicState<Array<UmbVariantId>>([englishVariant, danishVariant]);

			manager
				.statesForVariants(variantIdsState.asObservable())
				.subscribe((value) => {
					emitted.push(value);
					expect(value.find((v) => v.variantId.compare(englishVariant))?.states).to.deep.equal([universal]);
					expect(value.find((v) => v.variantId.compare(danishVariant))?.states).to.deep.equal([universal]);
					done();
				})
				.unsubscribe();
		});
	});
});
