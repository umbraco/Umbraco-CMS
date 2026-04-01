import { batchImagingRequest } from './imaging-request-batcher.js';
import type { UmbImagingResizeModel } from './types.js';
import { expect } from '@open-wc/testing';

interface BatchedItem {
	unique: string;
	url?: string;
}

type FetchFn = (
	uniques: Array<string>,
	model?: UmbImagingResizeModel,
) => Promise<{ data?: Array<BatchedItem>; error?: unknown }>;

function createMockFetchFn(urlMap: Record<string, string>): { fetchFn: FetchFn; calls: Array<Array<string>> } {
	const calls: Array<Array<string>> = [];
	const fetchFn: FetchFn = async (uniques) => {
		calls.push([...uniques]);
		const data = uniques
			.filter((u) => u in urlMap)
			.map((u) => ({ unique: u, url: urlMap[u] }));
		return { data };
	};
	return { fetchFn, calls };
}

describe('batchImagingRequest', () => {
	const model: UmbImagingResizeModel = { width: 300, height: 300 };

	it('batches multiple requests in the same event loop turn into one fetch call', async () => {
		const { fetchFn, calls } = createMockFetchFn({
			'id-1': 'http://example.com/1.jpg',
			'id-2': 'http://example.com/2.jpg',
			'id-3': 'http://example.com/3.jpg',
		});

		const p1 = batchImagingRequest('id-1', model, fetchFn);
		const p2 = batchImagingRequest('id-2', model, fetchFn);
		const p3 = batchImagingRequest('id-3', model, fetchFn);

		const [r1, r2, r3] = await Promise.all([p1, p2, p3]);

		expect(calls.length).to.equal(1);
		expect(calls[0]).to.have.members(['id-1', 'id-2', 'id-3']);
		expect(r1).to.equal('http://example.com/1.jpg');
		expect(r2).to.equal('http://example.com/2.jpg');
		expect(r3).to.equal('http://example.com/3.jpg');
	});

	it('returns undefined for uniques not found in the response', async () => {
		const { fetchFn } = createMockFetchFn({
			'id-1': 'http://example.com/1.jpg',
		});

		const p1 = batchImagingRequest('id-1', model, fetchFn);
		const p2 = batchImagingRequest('id-missing', model, fetchFn);

		const [r1, r2] = await Promise.all([p1, p2]);

		expect(r1).to.equal('http://example.com/1.jpg');
		expect(r2).to.be.undefined;
	});

	it('separates batches by imaging model', async () => {
		const model1: UmbImagingResizeModel = { width: 100, height: 100 };
		const model2: UmbImagingResizeModel = { width: 200, height: 200 };

		const { fetchFn: fetchFn1, calls: calls1 } = createMockFetchFn({
			'id-1': 'http://example.com/1-small.jpg',
		});
		const { fetchFn: fetchFn2, calls: calls2 } = createMockFetchFn({
			'id-2': 'http://example.com/2-large.jpg',
		});

		const p1 = batchImagingRequest('id-1', model1, fetchFn1);
		const p2 = batchImagingRequest('id-2', model2, fetchFn2);

		const [r1, r2] = await Promise.all([p1, p2]);

		expect(calls1.length).to.equal(1);
		expect(calls2.length).to.equal(1);
		expect(r1).to.equal('http://example.com/1-small.jpg');
		expect(r2).to.equal('http://example.com/2-large.jpg');
	});

	it('deduplicates the same unique in a batch', async () => {
		const { fetchFn, calls } = createMockFetchFn({
			'id-1': 'http://example.com/1.jpg',
		});

		const p1 = batchImagingRequest('id-1', model, fetchFn);
		const p2 = batchImagingRequest('id-1', model, fetchFn);

		const [r1, r2] = await Promise.all([p1, p2]);

		expect(calls.length).to.equal(1);
		// Only one unique in the fetch call despite two callers
		expect(calls[0]).to.deep.equal(['id-1']);
		expect(r1).to.equal('http://example.com/1.jpg');
		expect(r2).to.equal('http://example.com/1.jpg');
	});

	it('propagates errors to all callers in a batch', async () => {
		const error = new Error('Network failure');
		const fetchFn: FetchFn = async () => ({ error });

		const p1 = batchImagingRequest('id-1', model, fetchFn);
		const p2 = batchImagingRequest('id-2', model, fetchFn);

		const results = await Promise.allSettled([p1, p2]);

		expect(results[0].status).to.equal('rejected');
		expect(results[1].status).to.equal('rejected');
	});

	it('creates separate batches across different event loop turns', async () => {
		const { fetchFn, calls } = createMockFetchFn({
			'id-1': 'http://example.com/1.jpg',
			'id-2': 'http://example.com/2.jpg',
		});

		// First microtask batch
		const p1 = batchImagingRequest('id-1', model, fetchFn);
		await p1;

		// Second microtask batch (after first resolved)
		const p2 = batchImagingRequest('id-2', model, fetchFn);
		await p2;

		expect(calls.length).to.equal(2);
		expect(calls[0]).to.deep.equal(['id-1']);
		expect(calls[1]).to.deep.equal(['id-2']);
	});

	it('resolves successful items and returns undefined for failed chunk items on partial failure', async () => {
		// Generate 50 IDs to force chunking (batch size is 40)
		const allIds = Array.from({ length: 50 }, (_, i) => `id-${i}`);
		const urlMap: Record<string, string> = {};
		for (const id of allIds) {
			urlMap[id] = `http://example.com/${id}.jpg`;
		}

		let callIndex = 0;
		const fetchFn: FetchFn = async (uniques) => {
			callIndex++;
			// First chunk succeeds, second chunk fails
			if (callIndex === 2) {
				throw new Error('Chunk 2 failed');
			}
			const data = uniques.map((u) => ({ unique: u, url: urlMap[u] }));
			return { data };
		};

		const promises = allIds.map((id) => batchImagingRequest(id, model, fetchFn));
		const results = await Promise.allSettled(promises);

		// First 40 items (successful chunk) should resolve with URLs
		for (let i = 0; i < 40; i++) {
			const result = results[i];
			expect(result.status).to.equal('fulfilled');
			if (result.status === 'fulfilled') {
				expect(result.value).to.equal(`http://example.com/id-${i}.jpg`);
			}
		}

		// Last 10 items (failed chunk) should resolve as undefined, not hang
		for (let i = 40; i < 50; i++) {
			const result = results[i];
			expect(result.status).to.equal('fulfilled');
			if (result.status === 'fulfilled') {
				expect(result.value).to.be.undefined;
			}
		}
	});

	it('handles undefined imaging model', async () => {
		const { fetchFn, calls } = createMockFetchFn({
			'id-1': 'http://example.com/1.jpg',
		});

		const result = await batchImagingRequest('id-1', undefined, fetchFn);

		expect(calls.length).to.equal(1);
		expect(result).to.equal('http://example.com/1.jpg');
	});
});
