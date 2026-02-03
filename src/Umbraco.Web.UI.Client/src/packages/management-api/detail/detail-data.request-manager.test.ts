import { UmbManagementApiDetailDataRequestManager } from './detail-data.request-manager.js';
import { UmbManagementApiDetailDataCache } from './cache.js';
import { UmbManagementApiInFlightRequestCache } from '../inflight-request/cache.js';
import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

interface TestDetailModel {
	id: string;
	name: string;
}

interface TestCreateModel {
	name: string;
}

interface TestUpdateModel {
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

@customElement('test-request-manager-host')
class UmbTestRequestManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbManagementApiDetailDataRequestManager', () => {
	let hostElement: UmbTestRequestManagerHostElement;
	let manager: UmbManagementApiDetailDataRequestManager<TestDetailModel, TestCreateModel, TestUpdateModel>;
	let dataCache: UmbManagementApiDetailDataCache<TestDetailModel>;
	let inflightRequestCache: UmbManagementApiInFlightRequestCache<TestDetailModel>;
	let mockServerEventContext: MockServerEventContext;

	// Mock API functions
	let mockCreate: (data: TestCreateModel) => Promise<{ data: string }>;
	let mockRead: (id: string) => Promise<{ data: TestDetailModel }>;
	let mockUpdate: (id: string, data: TestUpdateModel) => Promise<{ data: unknown }>;
	let mockDelete: (id: string) => Promise<{ data: unknown }>;
	let mockReadMany: ((ids: Array<string>) => Promise<{ data: { items: Array<TestDetailModel> } }>) | undefined;

	beforeEach(async () => {
		hostElement = new UmbTestRequestManagerHostElement();
		document.body.appendChild(hostElement);

		// Set up mock server event context
		mockServerEventContext = new MockServerEventContext();
		new UmbContextProviderController(
			hostElement,
			UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT,
			mockServerEventContext as unknown as typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE,
		);

		// Set up caches
		dataCache = new UmbManagementApiDetailDataCache<TestDetailModel>();
		inflightRequestCache = new UmbManagementApiInFlightRequestCache<TestDetailModel>();

		// Set up mock API functions
		mockCreate = async (data: TestCreateModel) => ({ data: 'new-id' });
		mockRead = async (id: string) => ({ data: { id, name: `Item ${id}` } });
		mockUpdate = async () => ({ data: {} });
		mockDelete = async () => ({ data: {} });
		mockReadMany = undefined;

		// Create manager
		manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
			create: mockCreate,
			read: mockRead,
			update: mockUpdate,
			delete: mockDelete,
			dataCache,
			inflightRequestCache,
		});

		// Allow context consumption to complete
		await Promise.resolve();
	});

	afterEach(() => {
		manager.destroy();
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a create method', () => {
				expect(manager).to.have.property('create').that.is.a('function');
			});

			it('has a read method', () => {
				expect(manager).to.have.property('read').that.is.a('function');
			});

			it('has a readMany method', () => {
				expect(manager).to.have.property('readMany').that.is.a('function');
			});

			it('has an update method', () => {
				expect(manager).to.have.property('update').that.is.a('function');
			});

			it('has a delete method', () => {
				expect(manager).to.have.property('delete').that.is.a('function');
			});
		});
	});

	describe('create', () => {
		it('calls the create API and returns the created item', async () => {
			let createCalled = false;
			mockCreate = async (data: TestCreateModel) => {
				createCalled = true;
				return { data: 'created-id' };
			};
			mockRead = async (id: string) => ({ data: { id, name: 'Created Item' } });

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.create({ name: 'New Item' });

			expect(createCalled).to.be.true;
			expect(result.data).to.deep.equal({ id: 'created-id', name: 'Created Item' });
			expect(result.error).to.be.undefined;
		});

		it('returns an error if the create API fails', async () => {
			const mockError = { name: 'ApiError', message: 'Create failed' };
			mockCreate = async () => {
				throw mockError;
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.create({ name: 'New Item' });

			expect(result.data).to.be.undefined;
			expect(result.error).to.exist;
		});
	});

	describe('read', () => {
		it('fetches from the server when not connected to server events', async () => {
			let readCalled = false;
			mockRead = async (id: string) => {
				readCalled = true;
				return { data: { id, name: `Item ${id}` } };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.read('item-1');

			expect(readCalled).to.be.true;
			expect(result.data).to.deep.equal({ id: 'item-1', name: 'Item item-1' });
		});

		it('fetches from the server when connected but item is not cached', async () => {
			mockServerEventContext.setIsConnected(true);
			await Promise.resolve();

			let readCalled = false;
			mockRead = async (id: string) => {
				readCalled = true;
				return { data: { id, name: `Item ${id}` } };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			const result = await manager.read('item-1');

			expect(readCalled).to.be.true;
			expect(result.data).to.deep.equal({ id: 'item-1', name: 'Item item-1' });
		});

		it('returns cached data when connected to server events and item is cached', async () => {
			mockServerEventContext.setIsConnected(true);

			let readCalled = false;
			mockRead = async (id: string) => {
				readCalled = true;
				return { data: { id, name: `Fresh Item ${id}` } };
			};

			// Pre-populate cache
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item' });

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			const result = await manager.read('item-1');

			expect(readCalled).to.be.false;
			expect(result.data).to.deep.equal({ id: 'item-1', name: 'Cached Item' });
		});

		it('caches data after fetching when connected to server events', async () => {
			mockServerEventContext.setIsConnected(true);

			mockRead = async (id: string) => {
				return { data: { id, name: `Item ${id}` } };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			await manager.read('item-1');

			expect(dataCache.has('item-1')).to.be.true;
			expect(dataCache.get('item-1')).to.deep.equal({ id: 'item-1', name: 'Item item-1' });
		});

		it('does not cache data after fetching when not connected to server events', async () => {
			mockServerEventContext.setIsConnected(false);

			mockRead = async (id: string) => {
				return { data: { id, name: `Item ${id}` } };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			await manager.read('item-1');

			expect(dataCache.has('item-1')).to.be.false;
		});

		it('deduplicates concurrent requests for the same ID', async () => {
			let readCallCount = 0;
			mockRead = async (id: string) => {
				readCallCount++;
				// Simulate network delay
				await new Promise((resolve) => setTimeout(resolve, 50));
				return { data: { id, name: `Item ${id}` } };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Make concurrent requests for the same ID
			const [result1, result2, result3] = await Promise.all([
				manager.read('item-1'),
				manager.read('item-1'),
				manager.read('item-1'),
			]);

			// Should only have made one actual API call
			expect(readCallCount).to.equal(1);

			// All results should be the same
			expect(result1.data).to.deep.equal(result2.data);
			expect(result2.data).to.deep.equal(result3.data);
		});

		it('cleans up inflight cache after request completes', async () => {
			mockRead = async (id: string) => {
				return { data: { id, name: `Item ${id}` } };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			await manager.read('item-1');

			expect(inflightRequestCache.has('read:item-1')).to.be.false;
		});

		it('returns an error if the read API fails', async () => {
			const mockError = { name: 'ApiError', message: 'Read failed' };
			mockRead = async () => {
				throw mockError;
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.read('item-1');

			expect(result.data).to.be.undefined;
			expect(result.error).to.exist;
		});
	});

	describe('readMany', () => {
		it('throws an error if readMany function was not provided', async () => {
			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			try {
				await manager.readMany(['item-1', 'item-2']);
				expect.fail('Should have thrown an error');
			} catch (error) {
				expect((error as Error).message).to.include('readMany is not available');
			}
		});

		it('fetches all items from the server when not connected to server events', async () => {
			let requestedIds: Array<string> = [];
			mockReadMany = async (ids: Array<string>) => {
				requestedIds = ids;
				return {
					data: {
						items: ids.map((id) => ({ id, name: `Item ${id}` })),
					},
				};
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				readMany: mockReadMany,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.readMany(['item-1', 'item-2']);

			expect(requestedIds).to.deep.equal(['item-1', 'item-2']);
			expect(result.data?.items).to.have.lengthOf(2);
		});

		it('uses cached items and only fetches non-cached items when connected', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache with one item
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });

			let requestedIds: Array<string> = [];
			mockReadMany = async (ids: Array<string>) => {
				requestedIds = ids;
				return {
					data: {
						items: ids.map((id) => ({ id, name: `Fresh Item ${id}` })),
					},
				};
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				readMany: mockReadMany,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			const result = await manager.readMany(['item-1', 'item-2', 'item-3']);

			// Should only request non-cached items
			expect(requestedIds).to.deep.equal(['item-2', 'item-3']);

			// Result should contain all items (cached + fetched)
			expect(result.data?.items).to.have.lengthOf(3);

			// Verify cached item was returned from cache
			const cachedItem = result.data?.items.find((item) => item.id === 'item-1');
			expect(cachedItem?.name).to.equal('Cached Item 1');
		});

		it('returns only cached items if all requested items are cached', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache with all items
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });
			dataCache.set('item-2', { id: 'item-2', name: 'Cached Item 2' });

			let readManyCalled = false;
			mockReadMany = async (ids: Array<string>) => {
				readManyCalled = true;
				return { data: { items: [] } };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				readMany: mockReadMany,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			const result = await manager.readMany(['item-1', 'item-2']);

			expect(readManyCalled).to.be.false;
			expect(result.data?.items).to.have.lengthOf(2);
		});

		it('caches fetched items when connected to server events', async () => {
			mockServerEventContext.setIsConnected(true);

			mockReadMany = async (ids: Array<string>) => {
				return {
					data: {
						items: ids.map((id) => ({ id, name: `Item ${id}` })),
					},
				};
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				readMany: mockReadMany,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			await manager.readMany(['item-1', 'item-2']);

			expect(dataCache.has('item-1')).to.be.true;
			expect(dataCache.has('item-2')).to.be.true;
		});
	});

	describe('update', () => {
		it('calls the update API and returns the updated item', async () => {
			let updateCalled = false;
			let updateId: string | undefined;
			let updateData: TestUpdateModel | undefined;

			mockUpdate = async (id: string, data: TestUpdateModel) => {
				updateCalled = true;
				updateId = id;
				updateData = data;
				return { data: {} };
			};
			mockRead = async (id: string) => ({ data: { id, name: 'Updated Item' } });

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.update('item-1', { name: 'Updated Name' });

			expect(updateCalled).to.be.true;
			expect(updateId).to.equal('item-1');
			expect(updateData).to.deep.equal({ name: 'Updated Name' });
			expect(result.data).to.deep.equal({ id: 'item-1', name: 'Updated Item' });
		});

		it('returns an error if the update API fails', async () => {
			const mockError = { name: 'ApiError', message: 'Update failed' };
			mockUpdate = async () => {
				throw mockError;
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.update('item-1', { name: 'Updated Name' });

			expect(result.data).to.be.undefined;
			expect(result.error).to.exist;
		});
	});

	describe('delete', () => {
		it('calls the delete API', async () => {
			let deleteCalled = false;
			let deleteId: string | undefined;

			mockDelete = async (id: string) => {
				deleteCalled = true;
				deleteId = id;
				return { data: {} };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.delete('item-1');

			expect(deleteCalled).to.be.true;
			expect(deleteId).to.equal('item-1');
			expect(result.error).to.be.undefined;
		});

		it('removes item from cache when connected to server events', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache
			dataCache.set('item-1', { id: 'item-1', name: 'Item to delete' });

			mockDelete = async () => ({ data: {} });

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			await manager.delete('item-1');

			expect(dataCache.has('item-1')).to.be.false;
		});

		it('does not remove item from cache when delete fails', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache
			dataCache.set('item-1', { id: 'item-1', name: 'Item to delete' });

			mockDelete = async () => {
				throw { name: 'ApiError', message: 'Delete failed' };
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			await manager.delete('item-1');

			// Item should still be in cache since delete failed
			expect(dataCache.has('item-1')).to.be.true;
		});

		it('returns an error if the delete API fails', async () => {
			const mockError = { name: 'ApiError', message: 'Delete failed' };
			mockDelete = async () => {
				throw mockError;
			};

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			const result = await manager.delete('item-1');

			expect(result.error).to.exist;
		});
	});

	describe('Server Event Connection', () => {
		it('clears the cache when server connection is lost', async () => {
			mockServerEventContext.setIsConnected(true);

			// Pre-populate cache
			dataCache.set('item-1', { id: 'item-1', name: 'Cached Item 1' });
			dataCache.set('item-2', { id: 'item-2', name: 'Cached Item 2' });

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
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

			manager = new UmbManagementApiDetailDataRequestManager(hostElement, {
				create: mockCreate,
				read: mockRead,
				update: mockUpdate,
				delete: mockDelete,
				dataCache,
				inflightRequestCache,
			});

			// Wait for context observation
			await new Promise((resolve) => setTimeout(resolve, 10));

			// Cache should not be affected by undefined state
			expect(dataCache.has('item-1')).to.be.true;
		});
	});
});
