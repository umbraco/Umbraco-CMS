import { UmbInputMultiUrlElement } from './input-multi-url.element.js';
import type { UmbLinkPickerLink } from '../../link-picker-modal/types.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbMediaItemRepository, UmbMediaUrlRepository } from '@umbraco-cms/backoffice/media';

describe('UmbInputMultiUrlElement - reorder refetch', () => {
	let itemCallCount = 0;
	let urlCallCount = 0;
	const originalItemRequestItems = UmbMediaItemRepository.prototype.requestItems;
	const originalUrlRequestItems = UmbMediaUrlRepository.prototype.requestItems;

	beforeEach(() => {
		itemCallCount = 0;
		urlCallCount = 0;

		UmbMediaItemRepository.prototype.requestItems = async function (uniques: Array<string>) {
			itemCallCount++;
			return { data: uniques.map((unique) => ({ unique, name: `Name-${unique}` }) as any) };
		};

		UmbMediaUrlRepository.prototype.requestItems = async function (uniques: Array<string>) {
			urlCallCount++;
			return { data: uniques.map((unique) => ({ unique, url: `/media/${unique}` }) as any) };
		};
	});

	afterEach(() => {
		UmbMediaItemRepository.prototype.requestItems = originalItemRequestItems;
		UmbMediaUrlRepository.prototype.requestItems = originalUrlRequestItems;
	});

	it('does not re-request name/url for links that are only being reordered', async () => {
		const element = await fixture<UmbInputMultiUrlElement>(html`<umb-input-multi-url></umb-input-multi-url>`);

		const linkA: UmbLinkPickerLink = { type: 'media', unique: 'aaaa', name: '', url: '' };
		const linkB: UmbLinkPickerLink = { type: 'media', unique: 'bbbb', name: '', url: '' };
		const linkC: UmbLinkPickerLink = { type: 'media', unique: 'cccc', name: '', url: '' };

		element.urls = [linkA, linkB, linkC];
		await element.updateComplete;
		await new Promise((resolve) => setTimeout(resolve, 20));

		expect(itemCallCount, 'sanity check: initial render fetches names').to.be.greaterThan(0);
		expect(urlCallCount, 'sanity check: initial render fetches urls').to.be.greaterThan(0);

		itemCallCount = 0;
		urlCallCount = 0;

		// Simulate what the drag sorter's onChange handler does: the same links,
		// re-ordered, assigned back through the `urls` setter.
		element.urls = [linkC, linkA, linkB];
		await element.updateComplete;
		await new Promise((resolve) => setTimeout(resolve, 20));

		expect(itemCallCount, 'name should not be re-requested for already-resolved uniques on reorder').to.equal(0);
		expect(urlCallCount, 'url should not be re-requested for already-resolved uniques on reorder').to.equal(0);
	});
});
