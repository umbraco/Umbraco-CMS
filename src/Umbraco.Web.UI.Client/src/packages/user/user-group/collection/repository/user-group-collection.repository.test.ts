import { useMockHandlers, resetMockHandlers } from '../../../../../../mocks/index.js';
import { UmbUserGroupDetailStore } from '../../repository/detail/user-group-detail.store.js';
import { UmbUserGroupCollectionRepository } from './user-group-collection.repository.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

const FILTER_PATH = '/filter/user-group';

const TOTAL_ITEMS = 150;

const allItems = Array.from({ length: TOTAL_ITEMS }, (_, index) => ({
	id: `user-group-${index}`,
	name: `User Group ${index}`,
	alias: `userGroup${index}`,
	aliasCanBeChanged: true,
	description: null,
	icon: null,
	documentRootAccess: true,
	documentStartNode: null,
	mediaRootAccess: true,
	mediaStartNode: null,
	hasAccessToAllLanguages: true,
	languages: [],
	isDeletable: true,
	fallbackPermissions: [],
	permissions: [],
	sections: [],
}));

type PageRequest = { skip: string | null; take: string | null };

@customElement('umb-test-user-group-collection-repository-host')
class UmbTestUserGroupCollectionRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbUserGroupCollectionRepository', () => {
	let host: UmbTestUserGroupCollectionRepositoryHostElement;
	let repository: UmbUserGroupCollectionRepository;
	let requests: Array<PageRequest>;

	// These handlers page for real, so the assertions below only hold if the repository pages.
	const pagingHandler = () =>
		http.get(umbracoPath(FILTER_PATH), ({ request }) => {
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
		host = new UmbTestUserGroupCollectionRepositoryHostElement();
		new UmbUserGroupDetailStore(host);
		document.body.appendChild(host);
		repository = new UmbUserGroupCollectionRepository(host);
	});

	afterEach(() => {
		repository.destroy();
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	describe('requestAllItems', () => {
		it('pages through every user group', async () => {
			useMockHandlers(pagingHandler());

			const { data } = await repository.requestAllItems();

			expect(data?.items).to.have.lengthOf(TOTAL_ITEMS);
			expect(data?.total).to.equal(TOTAL_ITEMS);
			expect(requests).to.eql([
				{ skip: '0', take: '100' },
				{ skip: '100', take: '100' },
			]);
		});

		it('appends every user group to the store', async () => {
			useMockHandlers(pagingHandler());

			const { asObservable } = await repository.requestAllItems();

			const items = await new Promise<Array<unknown>>((resolve) => {
				const subscription = asObservable().subscribe((value) => {
					resolve(value);
					queueMicrotask(() => subscription.unsubscribe());
				});
			});
			expect(items).to.have.lengthOf(TOTAL_ITEMS);
		});

		it('returns an error when a page fails', async () => {
			useMockHandlers(http.get(umbracoPath(FILTER_PATH), () => new HttpResponse(null, { status: 500 })));

			const { data, error } = await repository.requestAllItems();

			expect(error).to.exist;
			expect(data).to.be.undefined;
		});
	});

	describe('requestCollection', () => {
		it('requests only the given page', async () => {
			useMockHandlers(pagingHandler());

			const { data } = await repository.requestCollection({ skip: 100, take: 100 });

			expect(requests).to.eql([{ skip: '100', take: '100' }]);
			expect(data?.items).to.have.lengthOf(TOTAL_ITEMS - 100);
			expect(data?.total).to.equal(TOTAL_ITEMS);
		});
	});
});
