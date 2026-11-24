import { UmbManagementApiTreeDataRequestManager } from './tree-data.request-manager.js';
import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from './types.js';
import { UmbManagementApiInFlightRequestCache } from '../inflight-request/cache.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbError, type UmbApiResponse } from '@umbraco-cms/backoffice/resources';

interface TestTreeItemModel {
	parent?: { id: string } | null;
}

@customElement('test-tree-request-manager-host')
class UmbTestTreeRequestManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbManagementApiTreeDataRequestManager', () => {
	let hostElement: UmbTestTreeRequestManagerHostElement;
	let inflightRequestCache: UmbManagementApiInFlightRequestCache<unknown>;
	let manager:
		| UmbManagementApiTreeDataRequestManager<
				TestTreeItemModel,
				UmbManagementApiTreeRootItemsRequestArgs,
				{ items: Array<TestTreeItemModel>; total: number },
				UmbManagementApiTreeChildrenOfRequestArgs,
				{ items: Array<TestTreeItemModel>; total: number },
				UmbManagementApiTreeAncestorsOfRequestArgs,
				Array<TestTreeItemModel>,
				UmbManagementApiTreeSiblingsFromRequestArgs,
				{ items: Array<TestTreeItemModel>; totalBefore: number; totalAfter: number }
		  >
		| undefined;

	const createManager = (
		getRootItems: (args: UmbManagementApiTreeRootItemsRequestArgs) => Promise<{
			data: { items: Array<TestTreeItemModel>; total: number };
		}>,
		options?: { withInflightCache?: boolean },
	) =>
		new UmbManagementApiTreeDataRequestManager(hostElement, {
			getRootItems,
			getChildrenOf: async () => ({ data: { items: [], total: 0 } }),
			getAncestorsOf: async () => ({ data: [] }),
			getSiblingsFrom: async () => ({ data: { items: [], totalBefore: 0, totalAfter: 0 } }),
			inflightRequestCache: options?.withInflightCache === false ? undefined : inflightRequestCache,
		});

	beforeEach(() => {
		hostElement = new UmbTestTreeRequestManagerHostElement();
		document.body.appendChild(hostElement);
		inflightRequestCache = new UmbManagementApiInFlightRequestCache<unknown>();
	});

	afterEach(() => {
		manager?.destroy();
		manager = undefined;
		document.body.innerHTML = '';
	});

	it('coalesces concurrent identical root requests into a single underlying call', async () => {
		let callCount = 0;
		manager = createManager(async () => {
			callCount++;
			await new Promise((resolve) => setTimeout(resolve, 50));
			return { data: { items: [], total: 0 } };
		});

		const [first, second] = await Promise.all([manager.getRootItems({}), manager.getRootItems({})]);

		expect(callCount).to.equal(1);
		expect(first.data).to.deep.equal(second.data);
	});

	it('removes the in-flight entry once settled so a later identical call fetches again', async () => {
		let callCount = 0;
		manager = createManager(async () => {
			callCount++;
			return { data: { items: [], total: 0 } };
		});

		await manager.getRootItems({});
		await manager.getRootItems({});

		expect(callCount).to.equal(2);
		expect(inflightRequestCache.has(`root:${JSON.stringify({ paging: { skip: 0, take: 50 } })}`)).to.be.false;
	});

	it('does not coalesce when no in-flight cache is provided', async () => {
		let callCount = 0;
		manager = createManager(
			async () => {
				callCount++;
				await new Promise((resolve) => setTimeout(resolve, 50));
				return { data: { items: [], total: 0 } };
			},
			{ withInflightCache: false },
		);

		await Promise.all([manager.getRootItems({}), manager.getRootItems({})]);

		expect(callCount).to.equal(2);
	});

	describe('getBatch', () => {
		interface TestBatchRequestArgs {
			uniques: Array<string>;
			dataType?: { unique: string };
		}

		const createBatchManager = (
			getBatch?: (
				args: TestBatchRequestArgs,
			) => Promise<UmbApiResponse<{ data?: { items: Array<TestTreeItemModel & { id: string }>; total: number } }>>,
		) =>
			new UmbManagementApiTreeDataRequestManager(hostElement, {
				getRootItems: async () => ({ data: { items: [], total: 0 } }),
				getChildrenOf: async () => ({ data: { items: [], total: 0 } }),
				getAncestorsOf: async () => ({ data: [] }),
				getSiblingsFrom: async () => ({ data: { items: [], totalBefore: 0, totalAfter: 0 } }),
				getBatch,
			});

		let batchManager: ReturnType<typeof createBatchManager> | undefined;

		afterEach(() => {
			batchManager?.destroy();
			batchManager = undefined;
		});

		it('requests at most 40 uniques at a time and returns the items in the requested order', async () => {
			const requestedChunks: Array<Array<string>> = [];
			const uniques = Array.from({ length: 85 }, (_, index) => `item-${index}`);
			batchManager = createBatchManager(async (args) => {
				requestedChunks.push(args.uniques);
				const items = args.uniques.map((unique) => ({ id: unique, parent: null }));
				return { data: { items, total: items.length } };
			});

			const { data, error } = await batchManager.getBatch({ uniques });

			expect(error).to.be.undefined;
			expect(requestedChunks.map((chunk) => chunk.length)).to.deep.equal([40, 40, 5]);
			expect((data as Array<{ id: string }>).map((item) => item.id)).to.deep.equal(uniques);
		});

		it('passes additional request arguments through to every request', async () => {
			const requests: Array<TestBatchRequestArgs> = [];
			batchManager = createBatchManager(async (args) => {
				requests.push(args);
				return { data: { items: [], total: 0 } };
			});

			await batchManager.getBatch({
				uniques: Array.from({ length: 41 }, (_, index) => `item-${index}`),
				dataType: { unique: 'data-type' },
			});

			expect(requests).to.have.length(2);
			expect(requests.every((request) => request.dataType?.unique === 'data-type')).to.be.true;
		});

		it('returns an error when the batch request resolves with an error', async () => {
			batchManager = createBatchManager(async () => ({ error: new UmbError('Request failed') }));

			const { data, error } = await batchManager.getBatch({ uniques: ['item-0'] });

			expect(data).to.be.undefined;
			expect(error).to.not.be.undefined;
		});

		it('returns an error when no batch request is configured', async () => {
			batchManager = createBatchManager();

			const { data, error } = await batchManager.getBatch({ uniques: ['item-0'] });

			expect(data).to.be.undefined;
			expect(error).to.not.be.undefined;
		});
	});
});
