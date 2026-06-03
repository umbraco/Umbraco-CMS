import { UmbMediaSearchServerDataSource } from './media-search.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-media-search-data-source-host')
class UmbTestMediaSearchDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const makeSearchItem = (id: string) => ({
	id,
	mediaType: { id: `mt-${id}`, icon: 'icon-picture', collection: null },
	hasChildren: false,
	isTrashed: false,
	parent: null,
	variants: [{ culture: null, name: `Media ${id}` }],
	flags: [],
});

const makeAncestorEntry = (id: string) => ({
	id,
	ancestors: [
		{
			id: `ancestor-of-${id}`,
			mediaType: { id: `mt-ancestor-${id}`, icon: 'icon-folder', collection: null },
			hasChildren: true,
			isTrashed: false,
			parent: null,
			variants: [{ culture: null, name: `Ancestor of ${id}` }],
			flags: [],
		},
	],
});

describe('UmbMediaSearchServerDataSource', () => {
	let hostElement: UmbTestMediaSearchDataSourceHostElement;
	let dataSource: UmbMediaSearchServerDataSource;

	const originalSearch = MediaService.getItemMediaSearch;
	const originalAncestors = MediaService.getItemMediaAncestors;

	let ancestorRequestBatches: Array<Array<string>>;

	const stubSearchReturning = (count: number) => {
		const items = Array.from({ length: count }, (_, i) => makeSearchItem(`media-${i}`));
		(MediaService as any).getItemMediaSearch = () => Promise.resolve({ data: { items, total: count } });
	};

	beforeEach(() => {
		hostElement = new UmbTestMediaSearchDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbMediaSearchServerDataSource(hostElement);

		ancestorRequestBatches = [];
		(MediaService as any).getItemMediaAncestors = (options: { query: { id: Array<string> } }) => {
			const ids = options.query.id;
			ancestorRequestBatches.push([...ids]);
			return Promise.resolve({ data: ids.map((id) => makeAncestorEntry(id)) });
		};
	});

	afterEach(() => {
		(MediaService as any).getItemMediaSearch = originalSearch;
		(MediaService as any).getItemMediaAncestors = originalAncestors;
		hostElement.remove();
	});

	it('batches the ancestors request into chunks of at most 40 ids', async () => {
		stubSearchReturning(95);

		const { data } = await dataSource.search({ query: 'pharmacy' });

		// 95 ids must be split across multiple requests, none exceeding the batch size of 40.
		expect(ancestorRequestBatches.length).to.equal(3);
		ancestorRequestBatches.forEach((batch) => expect(batch.length).to.be.at.most(40));

		// Every id is requested exactly once across the batches.
		const requestedIds = ancestorRequestBatches.flat();
		expect(requestedIds.length).to.equal(95);

		// Results are returned with their ancestors mapped from the amalgamated batches.
		expect(data?.items.length).to.equal(95);
		const lastItem = data?.items.find((item) => item.unique === 'media-94');
		expect(lastItem?.ancestors?.[0]?.unique).to.equal('ancestor-of-media-94');
	});

	it('sends a single ancestors request when results are within the batch size', async () => {
		stubSearchReturning(10);

		const { data } = await dataSource.search({ query: 'welcome' });

		expect(ancestorRequestBatches.length).to.equal(1);
		expect(ancestorRequestBatches[0].length).to.equal(10);
		expect(data?.items.length).to.equal(10);
	});

	it('returns an error instead of throwing when one of the batches fails', async () => {
		stubSearchReturning(95);

		// Fail the second of the three batches; the controller resolves it without rejecting, which
		// would otherwise leave an undefined hole in the amalgamated ancestors data.
		let callCount = 0;
		(MediaService as any).getItemMediaAncestors = (options: { query: { id: Array<string> } }) => {
			callCount++;
			if (callCount === 2) return Promise.reject(new Error('Simulated server error'));
			return Promise.resolve({ data: options.query.id.map((id) => makeAncestorEntry(id)) });
		};

		const result = await dataSource.search({ query: 'pharmacy' });

		expect(result.error).to.exist;
		expect(result.data).to.be.undefined;
	});
});
