import { useMockHandlers, resetMockHandlers } from '../../../../../../mocks/index.js';
import { UmbDocumentTypeStructureRepository } from './document-type-structure.repository.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

const UMB_SLUG = '/document-type';

const TOTAL_ITEMS = 150;

const allItems = Array.from({ length: TOTAL_ITEMS }, (_, index) => ({
	id: `document-type-${index}`,
	name: `Document Type ${index}`,
	description: null,
	icon: 'icon-document',
}));

type PageRequest = { skip: string | null; take: string | null };

@customElement('umb-test-document-type-structure-repository-host')
class UmbTestDocumentTypeStructureRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDocumentTypeStructureRepository', () => {
	let host: UmbTestDocumentTypeStructureRepositoryHostElement;
	let repository: UmbDocumentTypeStructureRepository;
	let requests: Array<PageRequest>;

	// The global mock handlers ignore skip/take and always return everything, which would let an unpaged
	// repository pass. These handlers page for real, so the assertions below only hold if the repository pages.
	const pagingHandler = (path: string) =>
		http.get(umbracoPath(path), ({ request }) => {
			const url = new URL(request.url);
			const skip = url.searchParams.get('skip');
			const take = url.searchParams.get('take');
			requests.push({ skip, take });

			const from = Number(skip) || 0;
			const size = take === null ? 100 : Number(take);

			return HttpResponse.json({ items: allItems.slice(from, from + size), total: allItems.length });
		});

	beforeEach(() => {
		requests = [];
		host = new UmbTestDocumentTypeStructureRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbDocumentTypeStructureRepository(host);
	});

	afterEach(() => {
		repository.destroy();
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	describe('requestAllAllowedChildrenOf', () => {
		it('pages through every allowed child of a document type', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/:id/allowed-children`));

			const { data } = await repository.requestAllAllowedChildrenOf('parent-document-type', null);

			expect(data?.items).to.have.lengthOf(TOTAL_ITEMS);
			expect(data?.total).to.equal(TOTAL_ITEMS);
			expect(requests).to.eql([
				{ skip: '0', take: '100' },
				{ skip: '100', take: '100' },
			]);
		});

		it('pages through every document type allowed at root', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/allowed-at-root`));

			const { data } = await repository.requestAllAllowedChildrenOf(null, null);

			expect(data?.items).to.have.lengthOf(TOTAL_ITEMS);
			expect(requests).to.eql([
				{ skip: '0', take: '100' },
				{ skip: '100', take: '100' },
			]);
		});

		it('returns an error when a page fails', async () => {
			useMockHandlers(
				http.get(umbracoPath(`${UMB_SLUG}/:id/allowed-children`), () => new HttpResponse(null, { status: 500 })),
			);

			const { data, error } = await repository.requestAllAllowedChildrenOf('parent-document-type', null);

			expect(error).to.exist;
			expect(data).to.be.undefined;
		});
	});

	describe('requestAllowedChildrenOf', () => {
		it('makes a single unpaged request when no paging is given', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/:id/allowed-children`));

			const { data } = await repository.requestAllowedChildrenOf('parent-document-type', null);

			expect(requests).to.eql([{ skip: null, take: null }]);
			expect(data?.total).to.equal(TOTAL_ITEMS);
		});

		it('requests only the given page', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/:id/allowed-children`));

			const { data } = await repository.requestAllowedChildrenOf('parent-document-type', null, {
				skip: 100,
				take: 100,
			});

			expect(requests).to.eql([{ skip: '100', take: '100' }]);
			expect(data?.items).to.have.lengthOf(TOTAL_ITEMS - 100);
			expect(data?.total).to.equal(TOTAL_ITEMS);
		});
	});
});
