import { expect } from '@open-wc/testing';
import { fetchAllPages } from './fetch-all-pages.function.js';
import type { UmbDataSourceResponse } from '../data-source-response.interface.js';
import type { UmbPagedModel } from '../types.js';

interface TestItem {
	id: number;
}

const buildFakeFetcher = (allItems: Array<TestItem>) => {
	const calls: Array<{ skip: number; take: number }> = [];
	const fetchPage = async (skip: number, take: number) => {
		calls.push({ skip, take });
		return { data: { items: allItems.slice(skip, skip + take), total: allItems.length } };
	};
	return { fetchPage, calls };
};

describe('fetchAllPages', () => {
	it('returns all items in a single call when total <= take', async () => {
		const all: Array<TestItem> = [{ id: 1 }, { id: 2 }];
		const { fetchPage, calls } = buildFakeFetcher(all);

		const { data } = await fetchAllPages(fetchPage, 10);

		expect(data?.items).to.eql(all);
		expect(data?.total).to.equal(2);
		expect(calls).to.have.lengthOf(1);
		expect(calls[0]).to.eql({ skip: 0, take: 10 });
	});

	it('pages through and returns all items when total > take', async () => {
		const all: Array<TestItem> = [{ id: 1 }, { id: 2 }, { id: 3 }, { id: 4 }, { id: 5 }];
		const { fetchPage, calls } = buildFakeFetcher(all);

		const { data } = await fetchAllPages(fetchPage, 2);

		expect(data?.items).to.eql(all);
		expect(data?.total).to.equal(5);
		expect(calls.map((c) => c.skip)).to.eql([0, 2, 4]);
	});

	it('makes no extra call when the last page exactly fills `take`', async () => {
		const all: Array<TestItem> = [{ id: 1 }, { id: 2 }, { id: 3 }, { id: 4 }];
		const { fetchPage, calls } = buildFakeFetcher(all);

		const { data } = await fetchAllPages(fetchPage, 2);

		expect(data?.items).to.eql(all);
		expect(data?.total).to.equal(4);
		expect(calls).to.have.lengthOf(2);
	});

	it('returns an empty result when there are no items', async () => {
		const { fetchPage, calls } = buildFakeFetcher([]);

		const { data } = await fetchAllPages(fetchPage, 100);

		expect(data?.items).to.eql([]);
		expect(data?.total).to.equal(0);
		expect(calls).to.have.lengthOf(1);
	});

	it('returns the error and stops paging when a page fetch fails', async () => {
		let callCount = 0;
		const fetchPage = async (skip: number, take: number) => {
			callCount++;
			if (callCount === 1) {
				return { data: { items: [{ id: 1 }, { id: 2 }] as Array<TestItem>, total: 100 } };
			}
			return { error: new Error('boom') };
		};

		const { data, error } = await fetchAllPages<TestItem>(fetchPage, 2);

		expect(data).to.be.undefined;
		expect(error).to.exist;
		expect(callCount).to.equal(2);
	});

	it('returns a synthesised error when the fetcher returns neither data nor error', async () => {
		const fetchPage = async () => ({}) as UmbDataSourceResponse<UmbPagedModel<TestItem>>;

		const { data, error } = await fetchAllPages<TestItem>(fetchPage, 2);

		expect(data).to.be.undefined;
		expect(error).to.be.an.instanceOf(Error);
	});

	it('rejects when `take` is not a positive finite number', async () => {
		const { fetchPage } = buildFakeFetcher([{ id: 1 }, { id: 2 }]);

		for (const invalid of [0, -1, NaN, Number.POSITIVE_INFINITY]) {
			let thrown: unknown;
			try {
				await fetchAllPages(fetchPage, invalid);
			} catch (e) {
				thrown = e;
			}
			expect(thrown, `take=${invalid}`).to.be.an.instanceOf(RangeError);
		}
	});

	it('returns an error if the server delivers an empty page before reaching the reported total', async () => {
		// Surfaces (rather than masks) a server that reports more items than it returns. Also guards against
		// an infinite loop if `total` and the actual items disagree.
		let callCount = 0;
		const fetchPage = async (skip: number, take: number) => {
			callCount++;
			if (callCount === 1) {
				return { data: { items: [{ id: 1 }, { id: 2 }] as Array<TestItem>, total: 10 } };
			}
			return { data: { items: [] as Array<TestItem>, total: 10 } };
		};

		const { data, error } = await fetchAllPages<TestItem>(fetchPage, 2);

		expect(data).to.be.undefined;
		expect(error).to.be.an.instanceOf(Error);
		expect(callCount).to.equal(2);
	});
});
