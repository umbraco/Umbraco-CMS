import { expect } from '@open-wc/testing';
import { searchIcons } from './icon-search.function.js';
import type { UmbIconDefinition } from '../types.js';

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
];

describe('searchIcons', () => {
	it('should return empty array for empty query', () => {
		expect(searchIcons(mockIcons, '')).to.deep.equal([]);
		expect(searchIcons(mockIcons, '   ')).to.deep.equal([]);
	});

	it('should match by icon name substring', () => {
		const results = searchIcons(mockIcons, 'truck');
		expect(results[0].name).to.equal('icon-truck');
	});

	it('should match by keyword', () => {
		const results = searchIcons(mockIcons, 'delivery');
		expect(results.some((r) => r.name === 'icon-truck')).to.be.true;
	});

	it('should match by group', () => {
		const results = searchIcons(mockIcons, 'transport');
		expect(results.some((r) => r.name === 'icon-truck')).to.be.true;
		expect(results.some((r) => r.name === 'icon-shipping')).to.be.true;
	});

	it('should rank exact name match higher than keyword match', () => {
		const results = searchIcons(mockIcons, 'heart');
		expect(results[0].name).to.equal('icon-heart');
	});

	it('should include fuzzy matches for typos', () => {
		const results = searchIcons(mockIcons, 'truk');
		expect(results.some((r) => r.name === 'icon-truck')).to.be.true;
	});

	it('should append related icons after primary results', () => {
		const results = searchIcons(mockIcons, 'truck');
		const truckIndex = results.findIndex((r) => r.name === 'icon-truck');
		const shippingIndex = results.findIndex((r) => r.name === 'icon-shipping');
		expect(truckIndex).to.be.greaterThan(-1);
		expect(shippingIndex).to.be.greaterThan(-1);
		expect(shippingIndex).to.be.greaterThan(truckIndex);
	});

	it('should not duplicate related icons already in primary results', () => {
		const results = searchIcons(mockIcons, 'transport');
		// Both truck and shipping match "transport" group directly
		// They are also related to each other, but should not appear twice
		const truckCount = results.filter((r) => r.name === 'icon-truck').length;
		const shippingCount = results.filter((r) => r.name === 'icon-shipping').length;
		expect(truckCount).to.equal(1);
		expect(shippingCount).to.equal(1);
	});

	it('should match icons without metadata by name only', () => {
		const results = searchIcons(mockIcons, 'no-metadata');
		expect(results.some((r) => r.name === 'icon-no-metadata')).to.be.true;
	});

	it('should match multi-word keyword tokens', () => {
		const results = searchIcons(mockIcons, 'magnifying');
		expect(results.some((r) => r.name === 'icon-search')).to.be.true;
	});

	it('should return no results for unrelated query', () => {
		const results = searchIcons(mockIcons, 'xyznonexistent');
		expect(results).to.have.length(0);
	});
});
