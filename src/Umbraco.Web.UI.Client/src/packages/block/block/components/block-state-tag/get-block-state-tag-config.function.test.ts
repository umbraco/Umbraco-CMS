import { getBlockStateTagConfig } from './get-block-state-tag-config.function.js';
import { expect } from '@open-wc/testing';
import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';

describe('getBlockStateTagConfig', () => {
	describe('external content', () => {
		it('shows the trashed tag, even though the block is not exposed', () => {
			const config = getBlockStateTagConfig({
				unpublished: true,
				isExternalContent: true,
				variantState: UmbElementVariantState.TRASHED,
			});
			expect(config?.labelKey).to.equal('content_trashed');
			expect(config?.color).to.equal('danger');
		});

		it('shows the pending changes tag, even though the block is exposed', () => {
			const config = getBlockStateTagConfig({
				unpublished: false,
				isExternalContent: true,
				variantState: UmbElementVariantState.PUBLISHED_PENDING_CHANGES,
			});
			expect(config?.labelKey).to.equal('content_publishedPendingChanges');
			expect(config?.color).to.equal('warning');
		});

		it('shows no tag for a published element', () => {
			const config = getBlockStateTagConfig({
				unpublished: false,
				isExternalContent: true,
				variantState: UmbElementVariantState.PUBLISHED,
			});
			expect(config).to.equal(undefined);
		});

		[UmbElementVariantState.DRAFT, UmbElementVariantState.NOT_CREATED, null, undefined].forEach((variantState) => {
			it(`shows the draft tag with the library element description for state ${variantState}`, () => {
				const config = getBlockStateTagConfig({ unpublished: true, isExternalContent: true, variantState });
				expect(config?.labelKey).to.equal('blockEditor_notExposedLabel');
				expect(config?.titleKey).to.equal('blockEditor_notPublishedLibraryElementDescription');
			});
		});
	});

	describe('local content', () => {
		it('shows the draft tag with the expose description when not exposed', () => {
			const config = getBlockStateTagConfig({ unpublished: true, isExternalContent: false });
			expect(config?.labelKey).to.equal('blockEditor_notExposedLabel');
			expect(config?.titleKey).to.equal('blockEditor_notExposedDescription');
		});

		it('shows no tag when exposed', () => {
			expect(getBlockStateTagConfig({ unpublished: false, isExternalContent: false })).to.equal(undefined);
		});

		it('ignores a variant state that was left over from external content', () => {
			const config = getBlockStateTagConfig({
				unpublished: false,
				isExternalContent: false,
				variantState: UmbElementVariantState.TRASHED,
			});
			expect(config).to.equal(undefined);
		});
	});
});
