import type { UmbElementItemModel } from '../../item/repository/types.js';
import { UmbElementVariantState } from '../../variant-state.js';
import { UmbElementPublishAwarenessApi } from './element-publish-awareness.api.js';
import { expect } from '@open-wc/testing';

function createItem(...states: Array<UmbElementVariantState | null | undefined>): UmbElementItemModel {
	return {
		entityType: 'element',
		unique: 'element-1',
		variants: states.map((state, index) => ({ name: `Variant ${index}`, culture: `c${index}`, state })),
	} as unknown as UmbElementItemModel;
}

describe('UmbElementPublishAwarenessApi', () => {
	let api: UmbElementPublishAwarenessApi;

	beforeEach(() => {
		api = new UmbElementPublishAwarenessApi();
	});

	afterEach(() => {
		api.destroy();
	});

	it('needs attention when the element is a draft', () => {
		expect(api.needsAttention(createItem(UmbElementVariantState.DRAFT))).to.be.true;
	});

	it('needs attention when the element is published with pending changes', () => {
		expect(api.needsAttention(createItem(UmbElementVariantState.PUBLISHED_PENDING_CHANGES))).to.be.true;
	});

	it('does not need attention when the element is fully published', () => {
		expect(api.needsAttention(createItem(UmbElementVariantState.PUBLISHED))).to.be.false;
	});

	it('does not need attention when the element is trashed', () => {
		expect(api.needsAttention(createItem(UmbElementVariantState.TRASHED))).to.be.false;
	});

	it('does not need attention when no variant has been created', () => {
		expect(api.needsAttention(createItem(UmbElementVariantState.NOT_CREATED))).to.be.false;
		expect(api.needsAttention(createItem(null))).to.be.false;
		expect(api.needsAttention(createItem(undefined))).to.be.false;
	});

	it('does not need attention when the element has no variants', () => {
		expect(api.needsAttention(createItem())).to.be.false;
	});

	it('needs attention when any variant is worse than published, regardless of variant order', () => {
		expect(
			api.needsAttention(createItem(UmbElementVariantState.PUBLISHED, UmbElementVariantState.DRAFT)),
			'draft last',
		).to.be.true;
		expect(
			api.needsAttention(
				createItem(UmbElementVariantState.PUBLISHED_PENDING_CHANGES, UmbElementVariantState.PUBLISHED),
			),
			'pending changes first',
		).to.be.true;
	});

	it('does not need attention when the only other variants are published or not created', () => {
		expect(
			api.needsAttention(createItem(UmbElementVariantState.PUBLISHED, UmbElementVariantState.NOT_CREATED)),
		).to.be.false;
	});
});
