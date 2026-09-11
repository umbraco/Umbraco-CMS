import { useMockHandlers, resetMockHandlers } from '../../../../../../mocks/index.js';
import { UmbMediaTypeStructureRepository } from './media-type-structure.repository.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

const UMB_SLUG = '/item/media-type';

const TOTAL_ITEMS = 150;

const allItems = Array.from({ length: TOTAL_ITEMS }, (_, index) => ({
	id: `media-type-${index}`,
	name: `Media Type ${index}`,
	description: null,
	icon: 'icon-picture',
	matchedFileExtension: true,
}));

type PageRequest = { skip: string | null; take: string | null };

@customElement('umb-test-media-type-structure-repository-host')
class UmbTestMediaTypeStructureRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaTypeStructureRepository', () => {
	let host: UmbTestMediaTypeStructureRepositoryHostElement;
	let repository: UmbMediaTypeStructureRepository;
	let requests: Array<PageRequest>;

	// The default handlers for these endpoints page only for `/folders`, and a handler that ignores skip/take
	// would let an unpaged repository pass. These page for real so the assertions below mean something.
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
		host = new UmbTestMediaTypeStructureRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbMediaTypeStructureRepository(host);
	});

	afterEach(() => {
		repository.destroy();
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	describe('requestMediaTypesOfFolders', () => {
		it('pages through every folder media type when no paging is given', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/folders`));

			const items = await repository.requestMediaTypesOfFolders();

			expect(items).to.have.lengthOf(TOTAL_ITEMS);
			expect(requests).to.eql([
				{ skip: '0', take: '100' },
				{ skip: '100', take: '100' },
			]);
		});

		it('requests only the given page when paging is given', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/folders`));

			const items = await repository.requestMediaTypesOfFolders({ skip: 100, take: 100 });

			expect(items).to.have.lengthOf(TOTAL_ITEMS - 100);
			expect(requests).to.eql([{ skip: '100', take: '100' }]);
		});

		it('returns an empty array when the request fails', async () => {
			useMockHandlers(http.get(umbracoPath(`${UMB_SLUG}/folders`), () => new HttpResponse(null, { status: 500 })));

			const items = await repository.requestMediaTypesOfFolders();

			expect(items).to.have.lengthOf(0);
		});
	});

	describe('requestMediaTypesOf', () => {
		it('pages through every media type of a file extension when no paging is given', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/allowed`));

			const items = await repository.requestMediaTypesOf({ fileExtension: 'jpg' });

			expect(items).to.have.lengthOf(TOTAL_ITEMS);
			expect(requests).to.eql([
				{ skip: '0', take: '100' },
				{ skip: '100', take: '100' },
			]);
		});

		it('requests only the given page when paging is given', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/allowed`));

			const items = await repository.requestMediaTypesOf({ fileExtension: 'jpg', skip: 0, take: 5 });

			expect(items).to.have.lengthOf(5);
			expect(requests).to.eql([{ skip: '0', take: '5' }]);
		});

		it('retains the matched file extension flag', async () => {
			useMockHandlers(pagingHandler(`${UMB_SLUG}/allowed`));

			const items = await repository.requestMediaTypesOf({ fileExtension: 'jpg', take: 1 });

			expect(items[0].matchedFileExtension).to.be.true;
		});
	});
});
