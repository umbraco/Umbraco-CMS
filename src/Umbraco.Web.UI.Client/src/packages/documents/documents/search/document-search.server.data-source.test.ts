import { UmbDocumentSearchServerDataSource } from './document-search.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-document-search-data-source-host')
class UmbTestDocumentSearchDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const makeSearchItem = (id: string) => ({
	id,
	documentType: { id: `dt-${id}`, icon: 'icon-document', collection: null },
	hasChildren: false,
	isProtected: false,
	isTrashed: false,
	parent: null,
	variants: [{ culture: null, name: `Document ${id}`, state: 'Published', flags: [] }],
	flags: [],
});

const makeAncestorEntry = (id: string) => ({
	id,
	ancestors: [
		{
			id: `ancestor-of-${id}`,
			documentType: { id: `dt-ancestor-${id}`, icon: 'icon-folder', collection: null },
			hasChildren: true,
			isProtected: false,
			isTrashed: false,
			parent: null,
			variants: [{ culture: null, name: `Ancestor of ${id}`, state: 'Published', flags: [] }],
			flags: [],
		},
	],
});

describe('UmbDocumentSearchServerDataSource', () => {
	let hostElement: UmbTestDocumentSearchDataSourceHostElement;
	let dataSource: UmbDocumentSearchServerDataSource;

	const originalSearch = DocumentService.getItemDocumentSearch;
	const originalAncestors = DocumentService.getItemDocumentAncestors;

	let ancestorRequestBatches: Array<Array<string>>;

	const stubSearchReturning = (count: number) => {
		const items = Array.from({ length: count }, (_, i) => makeSearchItem(`doc-${i}`));
		(DocumentService as any).getItemDocumentSearch = () => Promise.resolve({ data: { items, total: count } });
	};

	beforeEach(() => {
		hostElement = new UmbTestDocumentSearchDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDocumentSearchServerDataSource(hostElement);

		ancestorRequestBatches = [];
		(DocumentService as any).getItemDocumentAncestors = (options: { query: { id: Array<string> } }) => {
			const ids = options.query.id;
			ancestorRequestBatches.push([...ids]);
			return Promise.resolve({ data: ids.map((id) => makeAncestorEntry(id)) });
		};
	});

	afterEach(() => {
		(DocumentService as any).getItemDocumentSearch = originalSearch;
		(DocumentService as any).getItemDocumentAncestors = originalAncestors;
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
		const lastItem = data?.items.find((item) => item.unique === 'doc-94');
		expect(lastItem?.ancestors?.[0]?.unique).to.equal('ancestor-of-doc-94');
	});

	it('sends a single ancestors request when results are within the batch size', async () => {
		stubSearchReturning(10);

		const { data } = await dataSource.search({ query: 'welcome' });

		expect(ancestorRequestBatches.length).to.equal(1);
		expect(ancestorRequestBatches[0].length).to.equal(10);
		expect(data?.items.length).to.equal(10);
	});
});
