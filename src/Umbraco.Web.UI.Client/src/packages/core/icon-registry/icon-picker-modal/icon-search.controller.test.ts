import { expect } from '@open-wc/testing';
import { UmbIconSearchController } from './icon-search.controller.js';
import type { UmbIconDefinition } from '../types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-icon-search-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const mockIcons: Array<UmbIconDefinition> = [
	{
		name: 'icon-truck',
		path: () => Promise.resolve({ default: '' }),
		keywords: ['lorry', 'vehicle', 'van', 'shipping', 'delivery'],
		groups: ['transport', 'vehicle'],
		related: ['icon-shipping'],
	},
	{
		name: 'icon-shipping',
		path: () => Promise.resolve({ default: '' }),
		keywords: ['box', 'package', 'parcel'],
		groups: ['transport'],
		related: ['icon-truck'],
	},
	{
		name: 'icon-heart',
		path: () => Promise.resolve({ default: '' }),
		keywords: ['love', 'like', 'favorite'],
		groups: ['item', 'social'],
		related: [],
	},
	{
		name: 'icon-search',
		path: () => Promise.resolve({ default: '' }),
		keywords: ['find', 'magnifying glass'],
		groups: ['action'],
	},
	{
		name: 'icon-no-metadata',
		path: () => Promise.resolve({ default: '' }),
	},
	{
		name: 'icon-plane',
		path: () => Promise.resolve({ default: '' }),
		keywords: ['airplane', 'flight'],
		groups: ['transport vehicle'],
	},
];

describe('UmbIconSearchController', () => {
	let host: UmbTestControllerHostElement;
	let controller: UmbIconSearchController;

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
		controller = new UmbIconSearchController(host);
		controller.setIcons(mockIcons);
	});

	it('should return empty array for empty query', async () => {
		expect(await controller.search('')).to.deep.equal([]);
		expect(await controller.search('   ')).to.deep.equal([]);
	});

	it('should match by icon name substring', async () => {
		const results = await controller.search('truck');
		expect(results[0].name).to.equal('icon-truck');
	});

	it('should match by keyword', async () => {
		const results = await controller.search('delivery');
		expect(results.some((r) => r.name === 'icon-truck')).to.be.true;
	});

	it('should match by group', async () => {
		const results = await controller.search('transport');
		expect(results.some((r) => r.name === 'icon-truck')).to.be.true;
		expect(results.some((r) => r.name === 'icon-shipping')).to.be.true;
	});

	it('should rank exact name match higher than keyword match', async () => {
		const results = await controller.search('heart');
		expect(results[0].name).to.equal('icon-heart');
	});

	it('should include fuzzy matches for typos', async () => {
		const results = await controller.search('truk');
		expect(results.some((r) => r.name === 'icon-truck')).to.be.true;
	});

	it('should fuzzy-match typos against tokens within a multi-word group', async () => {
		// Group string on icon-plane is "transport vehicle". A typo'd query
		// "transprtt" must fuzzy-match the "transport" token within the group,
		// not the full "transport vehicle" string (where the extra word
		// dominates the edit distance and pushes similarity below threshold).
		const results = await controller.search('transprtt');
		expect(results.some((r) => r.name === 'icon-plane')).to.be.true;
	});

	it('should match a partial word against a token within a multi-word group', async () => {
		// "vehicl" should match icon-plane whose group is "transport vehicle",
		// regardless of whether substring or fuzzy tier catches it.
		const results = await controller.search('vehicl');
		expect(results.some((r) => r.name === 'icon-plane')).to.be.true;
	});

	it('should append related icons after primary results', async () => {
		const results = await controller.search('truck');
		const truckIndex = results.findIndex((r) => r.name === 'icon-truck');
		const shippingIndex = results.findIndex((r) => r.name === 'icon-shipping');
		expect(truckIndex).to.be.greaterThan(-1);
		expect(shippingIndex).to.be.greaterThan(-1);
		expect(shippingIndex).to.be.greaterThan(truckIndex);
	});

	it('should not duplicate related icons already in primary results', async () => {
		const results = await controller.search('transport');
		const truckCount = results.filter((r) => r.name === 'icon-truck').length;
		const shippingCount = results.filter((r) => r.name === 'icon-shipping').length;
		expect(truckCount).to.equal(1);
		expect(shippingCount).to.equal(1);
	});

	it('should match icons without metadata by name only', async () => {
		const results = await controller.search('no-metadata');
		expect(results.some((r) => r.name === 'icon-no-metadata')).to.be.true;
	});

	it('should match multi-word keyword tokens', async () => {
		const results = await controller.search('magnifying');
		expect(results.some((r) => r.name === 'icon-search')).to.be.true;
	});
	it('should match multi-word keyword tokens', async () => {
		const results = await controller.search('glass');
		expect(results.some((r) => r.name === 'icon-search')).to.be.true;
	});

	it('should return no results for unrelated query', async () => {
		const results = await controller.search('xyznonexistent');
		expect(results).to.have.length(0);
	});

	it('should abort a prior in-flight search when a new one starts', async () => {
		// Enough icons to force a yield within the scoring loop.
		const many: Array<UmbIconDefinition> = [];
		for (let i = 0; i < 200; i++) {
			many.push({ name: `icon-bulk-${i}`, path: () => Promise.resolve({ default: '' }) });
		}
		many.push(...mockIcons);
		controller.setIcons(many);

		const first = controller.search('truck');
		const second = controller.search('heart');

		let firstError: unknown;
		try {
			await first;
		} catch (err) {
			firstError = err;
		}
		expect((firstError as DOMException)?.name).to.equal('AbortError');

		const secondResults = await second;
		expect(secondResults[0].name).to.equal('icon-heart');
	});

	it('should abort an in-flight search on destroy', async () => {
		const many: Array<UmbIconDefinition> = [];
		for (let i = 0; i < 200; i++) {
			many.push({ name: `icon-bulk-${i}`, path: () => Promise.resolve({ default: '' }) });
		}
		many.push(...mockIcons);
		controller.setIcons(many);

		const pending = controller.search('truck');
		controller.destroy();

		let error: unknown;
		try {
			await pending;
		} catch (err) {
			error = err;
		}
		expect((error as DOMException)?.name).to.equal('AbortError');
	});
});
