import { UmbWorkspaceSplitViewManager } from './workspace-split-view-manager.controller.js';
import { UMB_WORKSPACE_PATH_VARIANT_DELIMITER } from './constants.js';
import { expect } from '@open-wc/testing';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

describe('UmbWorkspaceSplitViewManager', () => {
	let manager: UmbWorkspaceSplitViewManager;

	beforeEach(() => {
		manager = new UmbWorkspaceSplitViewManager();
	});

	describe('openVariants', () => {
		it('opens a single variant', () => {
			manager.openVariants([UmbVariantId.FromString('en-us')]);

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(1);
			expect(active[0]).to.deep.equal({ index: 0, culture: 'en-us', segment: null });
		});

		it('opens multiple variants indexed by position', () => {
			manager.openVariants([UmbVariantId.FromString('en-us'), UmbVariantId.FromString('da-dk')]);

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(2);
			expect(active[0]).to.deep.equal({ index: 0, culture: 'en-us', segment: null });
			expect(active[1]).to.deep.equal({ index: 1, culture: 'da-dk', segment: null });
		});

		it('adds a variant when opening more than are currently active', () => {
			manager.openVariants([UmbVariantId.FromString('en-us')]);
			manager.openVariants([UmbVariantId.FromString('en-us'), UmbVariantId.FromString('da-dk')]);

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(2);
			expect(active[1].culture).to.equal('da-dk');
		});

		it('removes excess variants when opening fewer than are currently active', () => {
			// Arrange: a split view with two variants.
			manager.openVariants([UmbVariantId.FromString('en-us'), UmbVariantId.FromString('da-dk')]);
			expect(manager.getActiveVariants()).to.have.lengthOf(2);

			// Act: navigate back to a single variant.
			manager.openVariants([UmbVariantId.FromString('en-us')]);

			// Assert: the stale second variant is gone.
			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(1);
			expect(active[0]).to.deep.equal({ index: 0, culture: 'en-us', segment: null });
		});

		it('replaces the active variant without merging when the count is unchanged', () => {
			manager.openVariants([UmbVariantId.FromString('en-us')]);
			manager.openVariants([UmbVariantId.FromString('da-dk')]);

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(1);
			expect(active[0]).to.deep.equal({ index: 0, culture: 'da-dk', segment: null });
		});

		it('preserves the segment of each variant', () => {
			manager.openVariants([UmbVariantId.FromString('en-us_seg1')]);

			const active = manager.getActiveVariants();
			expect(active[0]).to.deep.equal({ index: 0, culture: 'en-us', segment: 'seg1' });
		});

		it('emits a single batched update rather than one per intermediate mutation', () => {
			// Arrange: start in a split view, then subscribe and discard the initial (BehaviorSubject) emission.
			manager.openVariants([UmbVariantId.FromString('en-us'), UmbVariantId.FromString('da-dk')]);

			let emissions = 0;
			const subscription = manager.activeVariantsInfo.subscribe(() => emissions++);
			emissions = 0;

			// Act: a change that both removes one variant and replaces the other.
			manager.openVariants([UmbVariantId.FromString('sv-se')]);

			// Assert: observers see one consistent final state, not the intermediate removals/additions.
			expect(emissions).to.equal(1);
			subscription.unsubscribe();
		});
	});

	describe('setVariantParts', () => {
		it('opens a single variant from a route fragment', () => {
			manager.setVariantParts('en-us');

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(1);
			expect(active[0]).to.deep.equal({ index: 0, culture: 'en-us', segment: null });
		});

		it('opens both variants from a split-view route fragment', () => {
			manager.setVariantParts(`en-us${UMB_WORKSPACE_PATH_VARIANT_DELIMITER}da-dk`);

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(2);
			expect(active[0].culture).to.equal('en-us');
			expect(active[1].culture).to.equal('da-dk');
		});

		it('resolves the invariant culture to a null culture', () => {
			manager.setVariantParts('invariant');

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(1);
			expect(active[0]).to.deep.equal({ index: 0, culture: null, segment: null });
		});

		it('parses culture and segment from each fragment part', () => {
			manager.setVariantParts(`en-us_seg1${UMB_WORKSPACE_PATH_VARIANT_DELIMITER}da-dk_seg2`);

			const active = manager.getActiveVariants();
			expect(active).to.have.lengthOf(2);
			expect(active[0]).to.deep.equal({ index: 0, culture: 'en-us', segment: 'seg1' });
			expect(active[1]).to.deep.equal({ index: 1, culture: 'da-dk', segment: 'seg2' });
		});

		it('trims a split view down to a single variant when navigating from two parts to one', () => {
			manager.setVariantParts(`en-us${UMB_WORKSPACE_PATH_VARIANT_DELIMITER}da-dk`);
			expect(manager.getActiveVariants()).to.have.lengthOf(2);

			manager.setVariantParts('en-us');

			expect(manager.getActiveVariants()).to.have.lengthOf(1);
			expect(manager.getActiveVariants()[0].culture).to.equal('en-us');
		});
	});
});
