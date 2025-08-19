import { UmbManagementApiItemDataCache } from './cache.js';
import { expect } from '@open-wc/testing';

describe('UmbManagementApiItemDataCache', () => {
	let cache: UmbManagementApiItemDataCache<{ id: string }>;

	beforeEach(() => {
		cache = new UmbManagementApiItemDataCache();
	});

	describe('Public API', () => {
		describe('properties', () => {});

		describe('methods', () => {
			it('has a has method', () => {
				expect(cache).to.have.property('has').that.is.a('function');
			});

			it('has a set method', () => {
				expect(cache).to.have.property('set').that.is.a('function');
			});

			it('has a get method', () => {
				expect(cache).to.have.property('get').that.is.a('function');
			});

			it('has a getAll method', () => {
				expect(cache).to.have.property('getAll').that.is.a('function');
			});

			it('has a delete method', () => {
				expect(cache).to.have.property('delete').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(cache).to.have.property('clear').that.is.a('function');
			});
		});
	});

	describe('Has', () => {
		it('returns true if the item exists in the cache', () => {
			cache.set('item1', { id: 'item1' });
			expect(cache.has('item1')).to.be.true;
		});

		it('returns false if the item does not exist in the cache', () => {
			expect(cache.has('item2')).to.be.false;
		});
	});

	describe('Set', () => {
		it('adds an item to the cache', () => {
			cache.set('item1', { id: 'item1' });
			expect(cache.has('item1')).to.be.true;
		});

		it('updates an existing item in the cache', () => {
			cache.set('item1', { id: 'item1' });
			cache.set('item1', { id: 'item1-updated' });
			expect(cache.get('item1')).to.deep.equal({ id: 'item1-updated' });
		});
	});

	describe('Get', () => {
		it('returns an item from the cache', () => {
			cache.set('item1', { id: 'item1' });
			expect(cache.get('item1')).to.deep.equal({ id: 'item1' });
		});

		it('returns undefined if the item does not exist in the cache', () => {
			expect(cache.get('item2')).to.be.undefined;
		});
	});

	describe('GetAll', () => {
		it('returns all items from the cache', () => {
			cache.set('item1', { id: 'item1' });
			cache.set('item2', { id: 'item2' });
			expect(cache.getAll()).to.deep.equal([{ id: 'item1' }, { id: 'item2' }]);
		});
	});

	describe('Delete', () => {
		it('removes an item from the cache', () => {
			cache.set('item1', { id: 'item1' });
			cache.delete('item1');
			expect(cache.has('item1')).to.be.false;
		});
	});

	describe('Clear', () => {
		it('removes all items from the cache', () => {
			cache.set('item1', { id: 'item1' });
			cache.set('item2', { id: 'item2' });
			cache.clear();
			expect(cache.has('item1')).to.be.false;
			expect(cache.has('item2')).to.be.false;
		});
	});
});
