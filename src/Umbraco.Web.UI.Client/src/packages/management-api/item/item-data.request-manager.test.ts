import { UmbManagementApiItemDataRequestManager } from './item-data.request-manager.js';
import { UmbManagementApiItemDataCache } from './cache.js';
import { UmbManagementApiInFlightRequestCache } from '../inflight-request/cache.js';
import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

interface TestItemModel {
	id: string;
	name: string;
}

// Mock server event context
class MockServerEventContext {
	#isConnected = new UmbBooleanState(undefined);
	isConnected = this.#isConnected.asObservable();

	setIsConnected(value: boolean | undefined) {
		this.#isConnected.setValue(value);
	}

	getHostElement() {
		return undefined as unknown as Element;
	}
}

@customElement('test-item-request-manager-host')
class UmbTestItemRequestManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbManagementApiItemDataRequestManager', () => {
	let hostElement: UmbTestItemRequestManagerHostElement;
	let manager: UmbManagementApiItemDataRequestManager<TestItemModel>;
	let dataCache: UmbManagementApiItemDataCache<TestItemModel>;
	let inflightRequestCache: UmbManagementApiInFlightRequestCache<TestItemModel>;
	let mockServerEventContext: MockServerEventContext;

	// Mock API function
	let mockGetItems: (ids: Array<string>) => Promise<{ data: Array<TestItemModel> }>;

	beforeEach(async () => {
		hostElement = new UmbTestItemRequestManagerHostElement();
		document.body.appendChild(hostElement);

		// Set up mock server event context
		mockServerEventContext = new MockServerEventContext();
		new UmbContextProviderController(
			hostElement,
			UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT,
			mockServerEventContext as unknown as typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE,
		);

		// Set up caches
		dataCache = new UmbManagementApiItemDataCache<TestItemModel>();
		inflightRequestCache = new UmbManagementApiInFlightRequestCache<TestItemModel>();

		// Set up mock API function
		mockGetItems = async (ids: Array<string>) => ({
			data: ids.map((id) => ({ id, name: `Item ${id}` })),
		});

		// Create manager
		manager = new UmbManagementApiItemDataRequestManager(hostElement, {
			getItems: mockGetItems,
			dataCache,
			inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});

		// Allow context consumption to complete
		await Promise.resolve();
	});

	afterEach(() => {
		manager.destroy();
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		it('has a getItems method', () => {
			expect(manager).to.have.property('getItems').that.is.a('function');
		});
	});

	describe('getItems', () => {
		it('fetches from the server when not connected to server events', async () => {
			let requestedIds: Array<string> = [];
			mockGetItems = async (ids: Array<string>) => {
				requestedIds = ids;
				return { data: ids.map((id) => ({ id, name: `Item ${id}` })) };
			};

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			const result = await manager.getItems(['item-1', 'item-2']);

			expect(requestedIds).to.deep.equal(['item-1', 'item-2']);
			expect(result.data).to.have.lengthOf(2);
		});

		it('returns cached data when connected to server events and items are cached', async () => {
			mockServerEventContext.setIsConnected(true);

			let getItemsCalled = false;
			mockGetItems = async (ids: Array<string>) => {
				getItemsCalled = true;
				return { data: ids.map((id) => ({ id, name: `Fresh Item ${id}` })) };
			};

			// Pre-populate cache
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });
			dataCache.set('item-2', { id: 'item-2', name: 'Cached Item 2' });

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			const result = await manager.getItems(['item-1', 'item-2']);

			expect(getItemsCalled).to.be.false;
			expect(result.data).to.have.lengthOf(2);
			expect(result.data?.find((i) => i.id === 'item-1')?.name).to.equal('Cached Item 1');
		});

		it('uses cached items and only fetches non-cached items when connected', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache with one item
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });

			let requestedIds: Array<string> = [];
			mockGetItems = async (ids: Array<string>) => {
				requestedIds = ids;
				return { data: ids.map((id) => ({ id, name: `Fresh Item ${id}` })) };
			};

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			const result = await manager.getItems(['item-1', 'item-2', 'item-3']);

			// Should only request non-cached items
			expect(requestedIds).to.deep.equal(['item-2', 'item-3']);

			// Result should contain all items (cached + fetched)
			expect(result.data).to.have.lengthOf(3);

			// Verify cached item was returned from cache
			const cachedItem = result.data?.find((item) => item.id === 'item-1');
			expect(cachedItem?.name).to.equal('Cached Item 1');
		});

		it('caches data after fetching when connected to server events', async () => {
			mockServerEventContext.setIsConnected(true);

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			await manager.getItems(['item-1', 'item-2']);

			expect(dataCache.has('item-1')).to.be.true;
			expect(dataCache.has('item-2')).to.be.true;
		});

		it('does not cache data after fetching when not connected to server events', async () => {
			mockServerEventContext.setIsConnected(false);

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			await manager.getItems(['item-1', 'item-2']);

			expect(dataCache.has('item-1')).to.be.false;
			expect(dataCache.has('item-2')).to.be.false;
		});

		it('deduplicates concurrent getItems calls with overlapping IDs', async () => {
			const requestedIdBatches: Array<Array<string>> = [];
			mockGetItems = async (ids: Array<string>) => {
				requestedIdBatches.push([...ids]);
				// Simulate network delay so both calls are concurrent
				await new Promise((resolve) => setTimeout(resolve, 50));
				return { data: ids.map((id) => ({ id, name: `Item ${id}` })) };
			};

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Make concurrent requests with overlapping IDs
			const [result1, result2] = await Promise.all([
				manager.getItems(['item-1', 'item-2', 'item-3']),
				manager.getItems(['item-2', 'item-3', 'item-4']),
			]);

			// The first call should request all 3 IDs, the second should only request item-4
			// (item-2 and item-3 are already inflight from the first call)
			const allRequestedIds = requestedIdBatches.flat();
			expect(allRequestedIds).to.include('item-1');
			expect(allRequestedIds).to.include('item-4');
			// item-2 and item-3 should only appear once across all batches
			expect(allRequestedIds.filter((id) => id === 'item-2')).to.have.lengthOf(1);
			expect(allRequestedIds.filter((id) => id === 'item-3')).to.have.lengthOf(1);

			// Both results should contain the items they requested
			expect(result1.data).to.have.lengthOf(3);
			expect(result2.data).to.have.lengthOf(3);

			expect(result1.data?.map((i) => i.id)).to.include.members(['item-1', 'item-2', 'item-3']);
			expect(result2.data?.map((i) => i.id)).to.include.members(['item-2', 'item-3', 'item-4']);
		});

		it('makes zero server requests when all IDs are already inflight', async () => {
			let getItemsCallCount = 0;
			mockGetItems = async (ids: Array<string>) => {
				getItemsCallCount++;
				await new Promise((resolve) => setTimeout(resolve, 50));
				return { data: ids.map((id) => ({ id, name: `Item ${id}` })) };
			};

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			const [result1, result2] = await Promise.all([
				manager.getItems(['item-1', 'item-2']),
				manager.getItems(['item-1', 'item-2']),
			]);

			// Only one batch API call should have been made
			expect(getItemsCallCount).to.equal(1);

			// Both callers should get the same data
			expect(result1.data).to.have.lengthOf(2);
			expect(result2.data).to.have.lengthOf(2);
			expect(result1.data?.map((i) => i.id)).to.include.members(['item-1', 'item-2']);
			expect(result2.data?.map((i) => i.id)).to.include.members(['item-1', 'item-2']);
		});

		it('cleans up inflight cache after requests complete', async () => {
			mockGetItems = async (ids: Array<string>) => {
				await new Promise((resolve) => setTimeout(resolve, 50));
				return { data: ids.map((id) => ({ id, name: `Item ${id}` })) };
			};

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			await Promise.all([
				manager.getItems(['item-1', 'item-2']),
				manager.getItems(['item-2', 'item-3']),
			]);

			// All inflight entries should be cleaned up
			expect(inflightRequestCache.has('item:item-1')).to.be.false;
			expect(inflightRequestCache.has('item:item-2')).to.be.false;
			expect(inflightRequestCache.has('item:item-3')).to.be.false;
		});

		it('combines data cache, inflight, and new requests correctly', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache with one item
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });

			const requestedIdBatches: Array<Array<string>> = [];
			mockGetItems = async (ids: Array<string>) => {
				requestedIdBatches.push([...ids]);
				await new Promise((resolve) => setTimeout(resolve, 50));
				return { data: ids.map((id) => ({ id, name: `Fresh Item ${id}` })) };
			};

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			const [result1, result2] = await Promise.all([
				manager.getItems(['item-1', 'item-2', 'item-3']),
				manager.getItems(['item-1', 'item-2', 'item-4']),
			]);

			// item-1 should come from data cache (not requested)
			// item-2 and item-3 should be requested by first call
			// item-4 should be requested by second call
			// item-2 should NOT be re-requested by second call (inflight from first)
			const allRequestedIds = requestedIdBatches.flat();
			expect(allRequestedIds).to.not.include('item-1'); // from cache
			expect(allRequestedIds.filter((id) => id === 'item-2')).to.have.lengthOf(1); // only once
			expect(allRequestedIds).to.include('item-3');
			expect(allRequestedIds).to.include('item-4');

			// Both results should have all their requested items
			expect(result1.data).to.have.lengthOf(3);
			expect(result2.data).to.have.lengthOf(3);

			// Verify cached item was returned from cache with its cached name
			const cachedItem = result1.data?.find((item) => item.id === 'item-1');
			expect(cachedItem?.name).to.equal('Cached Item 1');
		});
	});

	describe('Server Event Connection', () => {
		it('clears the cache when server connection is lost', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });
			dataCache.set('item-2', { id: 'item-2', name: 'Cached Item 2' });

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			// Verify items are in cache
			expect(dataCache.has('item-1')).to.be.true;
			expect(dataCache.has('item-2')).to.be.true;

			// Simulate losing connection
			mockServerEventContext.setIsConnected(false);

			// Wait for observation to process
			await new Promise((resolve) => setTimeout(resolve, 10));

			// Cache should be cleared
			expect(dataCache.has('item-1')).to.be.false;
			expect(dataCache.has('item-2')).to.be.false;
		});

		it('ignores undefined connection state', async () => {
			// Pre-populate cache
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				inflightRequestCache,
				getUniqueMethod: (item) => item.id,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			// Cache should not be affected by undefined state
			expect(dataCache.has('item-1')).to.be.true;
		});
	});

	describe('without inflight cache', () => {
		it('works without an inflight cache (backwards compatibility)', async () => {
			let requestedIds: Array<string> = [];
			mockGetItems = async (ids: Array<string>) => {
				requestedIds = ids;
				return { data: ids.map((id) => ({ id, name: `Item ${id}` })) };
			};

			manager = new UmbManagementApiItemDataRequestManager(hostElement, {
				getItems: mockGetItems,
				dataCache,
				getUniqueMethod: (item) => item.id,
			});

			const result = await manager.getItems(['item-1', 'item-2']);

			expect(requestedIds).to.deep.equal(['item-1', 'item-2']);
			expect(result.data).to.have.lengthOf(2);
		});
	});
});
