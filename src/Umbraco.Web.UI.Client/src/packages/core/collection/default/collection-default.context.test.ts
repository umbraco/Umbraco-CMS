import { UmbDefaultCollectionContext } from './collection-default.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbCollectionFilterModel } from '../collection-filter-model.interface.js';
import type { UmbCollectionRepository } from '../repository/collection-repository.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

const FILTER_MEMORY_UNIQUE = 'UmbCollectionFilter';
const ORDER_MEMORY_UNIQUE = 'UmbCollectionOrder';
const PAGINATION_MEMORY_UNIQUE = 'UmbCollectionPagination';

@customElement('test-collection-default-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type UmbTestCollectionFilterModel = UmbCollectionFilterModel & {
	orderBy?: string;
	orderDirection?: string;
};

type UmbTestCollectionItem = { unique: string };

type UmbTestCollectionResponse = UmbRepositoryResponse<UmbPagedModel<UmbTestCollectionItem>>;

/**
 * A collection repository whose requests stay open until the test answers them, identified by the order they were made in.
 * An answer may be given before the request reaches the repository.
 */
class UmbTestDeferredCollectionRepository {
	#responses: Array<{
		promise: Promise<UmbTestCollectionResponse>;
		resolve: (response: UmbTestCollectionResponse) => void;
	}> = [];
	#requestCount = 0;

	#getResponse(index: number) {
		if (!this.#responses[index]) {
			let resolve!: (response: UmbTestCollectionResponse) => void;
			const promise = new Promise<UmbTestCollectionResponse>((r) => (resolve = r));
			this.#responses[index] = { promise, resolve };
		}
		return this.#responses[index];
	}

	requestCollection() {
		return this.#getResponse(this.#requestCount++).promise;
	}

	answer(index: number, items: Array<UmbTestCollectionItem>) {
		this.#getResponse(index).resolve({ data: { items, total: items.length } });
	}
}

/**
 * Exposes the protected members a collection uses when it requests its items and when a request has been answered.
 */
class UmbTestCollectionContext extends UmbDefaultCollectionContext<any, UmbTestCollectionFilterModel> {
	receiveTotalItems(totalItems: number) {
		this._setTotalItems(totalItems);
	}

	useRepository(repository: UmbTestDeferredCollectionRepository) {
		this._repository = repository as unknown as UmbCollectionRepository;
		this._init = Promise.resolve();
	}

	request() {
		return this._requestCollection();
	}
}

const PAGE_SIZE = 10;
const DEFAULT_VIEW_ALIAS = 'Umb.CollectionView.Test';

const readOnce = <T>(observable: Observable<T>): T => {
	let value!: T;
	observable.subscribe((emitted) => (value = emitted)).unsubscribe();
	return value;
};

describe('UmbDefaultCollectionContext action affordances', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbTestCollectionContext;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		context = new UmbTestCollectionContext(hostElement, DEFAULT_VIEW_ALIAS);
	});

	it('shows actions when the configuration does not say otherwise', () => {
		context.setConfig({ pageSize: PAGE_SIZE });

		expect(readOnce(context.hideItemActions)).to.be.false;
		expect(readOnce(context.hideCollectionActions)).to.be.false;
	});

	it('hides the item actions when configured to', () => {
		context.setConfig({ pageSize: PAGE_SIZE, hideItemActions: true });

		expect(readOnce(context.hideItemActions)).to.be.true;
		expect(readOnce(context.hideCollectionActions)).to.be.false;
	});

	it('hides the collection actions when configured to', () => {
		context.setConfig({ pageSize: PAGE_SIZE, hideCollectionActions: true });

		expect(readOnce(context.hideCollectionActions)).to.be.true;
		expect(readOnce(context.hideItemActions)).to.be.false;
	});

	it('does not change what a click on an item does when actions are hidden', () => {
		// Whether actions render and what selecting does are separate axes, so hiding actions must not imply select-only.
		context.setConfig({ pageSize: PAGE_SIZE, hideItemActions: true, hideCollectionActions: true });

		expect(readOnce(context.selectOnly)).to.be.false;
	});
});

describe('UmbDefaultCollectionContext interaction memory', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbTestCollectionContext;

	const getFilter = () => context.getFilter() as Record<string, unknown>;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		context = new UmbTestCollectionContext(hostElement, DEFAULT_VIEW_ALIAS);
	});

	describe('writing', () => {
		beforeEach(() => {
			context.setConfig({ pageSize: PAGE_SIZE, orderBy: 'updateDate', orderDirection: 'desc' });
		});

		it('remembers the filter term', () => {
			context.setFilter({ filter: 'news' });
			expect(context.interactionMemory.getMemory(FILTER_MEMORY_UNIQUE)?.value).to.eql({
				filter: 'news',
			});
		});

		it('forgets the filter term when it is cleared', () => {
			context.setFilter({ filter: 'news' });
			context.setFilter({ filter: '' });
			expect(context.interactionMemory.getMemory(FILTER_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('remembers the ordering', () => {
			context.setFilter({ orderBy: 'name', orderDirection: 'asc' });
			expect(context.interactionMemory.getMemory(ORDER_MEMORY_UNIQUE)?.value).to.eql({
				orderBy: 'name',
				orderDirection: 'asc',
			});
		});

		it('does not remember the ordering when it matches the configured ordering', () => {
			context.setFilter({ orderBy: 'updateDate', orderDirection: 'desc' });
			expect(context.interactionMemory.getMemory(ORDER_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('does not remember the ordering when it matches the default filter of the collection', () => {
			const contextWithDefaultOrder = new UmbDefaultCollectionContext<any, UmbTestCollectionFilterModel>(
				hostElement,
				DEFAULT_VIEW_ALIAS,
				{ orderBy: 'name', orderDirection: 'asc' },
			);
			contextWithDefaultOrder.setConfig({ pageSize: PAGE_SIZE });

			contextWithDefaultOrder.setFilter({ orderBy: 'name', orderDirection: 'asc' });

			expect(contextWithDefaultOrder.interactionMemory.getMemory(ORDER_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('remembers the page number', () => {
			context.setFilter({ skip: PAGE_SIZE * 2 });
			expect(context.interactionMemory.getMemory(PAGINATION_MEMORY_UNIQUE)?.value).to.eql({
				pageNumber: 3,
			});
		});

		it('does not remember the first page', () => {
			context.setFilter({ skip: PAGE_SIZE * 2 });
			context.setFilter({ skip: 0 });
			expect(context.interactionMemory.getMemory(PAGINATION_MEMORY_UNIQUE)).to.be.undefined;
		});
	});

	describe('reading', () => {
		it('applies the remembered filter term, ordering and page number', () => {
			context.interactionMemory.setMemory({
				unique: FILTER_MEMORY_UNIQUE,
				value: { filter: 'news' },
			});
			context.interactionMemory.setMemory({
				unique: ORDER_MEMORY_UNIQUE,
				value: { orderBy: 'name', orderDirection: 'asc' },
			});
			context.interactionMemory.setMemory({
				unique: PAGINATION_MEMORY_UNIQUE,
				value: { pageNumber: 3 },
			});

			context.setConfig({ pageSize: PAGE_SIZE, orderBy: 'updateDate', orderDirection: 'desc' });

			expect(getFilter()).to.include({
				filter: 'news',
				orderBy: 'name',
				orderDirection: 'asc',
				skip: PAGE_SIZE * 2,
			});
		});

		it('applies memories that arrive after the collection has been configured', () => {
			context.setConfig({ pageSize: PAGE_SIZE });

			context.interactionMemory.setMemory({
				unique: FILTER_MEMORY_UNIQUE,
				value: { filter: 'news' },
			});

			expect(getFilter()).to.include({ filter: 'news' });
		});

		it('leaves the configured ordering alone when nothing is remembered', () => {
			context.setConfig({ pageSize: PAGE_SIZE, orderBy: 'updateDate', orderDirection: 'desc' });

			expect(getFilter()).to.include({ orderBy: 'updateDate', orderDirection: 'desc', skip: 0 });
		});
	});

	describe('restoring the page number', () => {
		beforeEach(() => {
			context.interactionMemory.setMemory({ unique: PAGINATION_MEMORY_UNIQUE, value: { pageNumber: 7 } });
			context.setConfig({ pageSize: PAGE_SIZE });
		});

		it('requests the remembered page before the total amount of items is known', () => {
			expect(getFilter()).to.include({ skip: PAGE_SIZE * 6 });
			expect(context.pagination.getCurrentPageNumber()).to.equal(1);
		});

		it('moves the pagination to the remembered page once the total amount of items is known', () => {
			context.receiveTotalItems(PAGE_SIZE * 100);

			expect(context.pagination.getCurrentPageNumber()).to.equal(7);
			expect(getFilter()).to.include({ skip: PAGE_SIZE * 6 });
		});

		it('corrects the remembered page when the collection no longer has that many pages', () => {
			context.receiveTotalItems(PAGE_SIZE * 3);

			expect(context.pagination.getCurrentPageNumber()).to.equal(3);
			expect(getFilter()).to.include({ skip: PAGE_SIZE * 2 });
		});
	});
});

describe('UmbDefaultCollectionContext requesting the collection', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbTestCollectionContext;
	let repository: UmbTestDeferredCollectionRepository;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		context = new UmbTestCollectionContext(hostElement, DEFAULT_VIEW_ALIAS);
		repository = new UmbTestDeferredCollectionRepository();
		context.setConfig({ pageSize: PAGE_SIZE });
		context.useRepository(repository);
	});

	it('applies the response of the latest request when an earlier request answers last', async () => {
		const earlier = context.request();
		const latest = context.request();

		repository.answer(1, [{ unique: 'latest' }]);
		await latest;
		repository.answer(0, [{ unique: 'earlier-1' }, { unique: 'earlier-2' }]);
		await earlier;

		expect(readOnce(context.items)).to.eql([{ unique: 'latest' }]);
		expect(readOnce(context.totalItems)).to.equal(1);
	});

	it('stays loading until the latest request has answered', async () => {
		const earlier = context.request();
		const latest = context.request();

		repository.answer(0, [{ unique: 'earlier' }]);
		await earlier;
		expect(readOnce(context.loading)).to.be.true;

		repository.answer(1, [{ unique: 'latest' }]);
		await latest;
		expect(readOnce(context.loading)).to.be.false;
	});

	it('does not hand a superseded response to a collection that applies the data itself', async () => {
		const appliedItems: Array<Array<string>> = [];
		const overridingContext = new (class extends UmbTestCollectionContext {
			protected override _setCollectionData(data: UmbPagedModel<UmbTestCollectionItem>) {
				appliedItems.push(data.items.map((item) => item.unique));
				super._setCollectionData(data);
			}
		})(hostElement, DEFAULT_VIEW_ALIAS);
		overridingContext.setConfig({ pageSize: PAGE_SIZE });
		overridingContext.useRepository(repository);

		const earlier = overridingContext.request();
		const latest = overridingContext.request();

		repository.answer(1, [{ unique: 'latest' }]);
		await latest;
		repository.answer(0, [{ unique: 'earlier' }]);
		await earlier;

		expect(appliedItems).to.eql([['latest']]);
	});
});
