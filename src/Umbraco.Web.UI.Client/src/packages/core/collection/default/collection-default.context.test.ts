import { UmbDefaultCollectionContext } from './collection-default.context.js';
import {
	UMB_COLLECTION_FILTER_MEMORY_UNIQUE,
	UMB_COLLECTION_ORDER_MEMORY_UNIQUE,
	UMB_COLLECTION_PAGINATION_MEMORY_UNIQUE,
} from '../interaction-memory/constants.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbCollectionFilterModel } from '../collection-filter-model.interface.js';

@customElement('test-collection-default-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type UmbTestCollectionFilterModel = UmbCollectionFilterModel & {
	orderBy?: string;
	orderDirection?: string;
};

const PAGE_SIZE = 10;
const DEFAULT_VIEW_ALIAS = 'Umb.CollectionView.Test';

describe('UmbDefaultCollectionContext interaction memory', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbDefaultCollectionContext<any, UmbTestCollectionFilterModel>;

	const getFilter = () => context.getFilter() as Record<string, unknown>;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		context = new UmbDefaultCollectionContext(hostElement, DEFAULT_VIEW_ALIAS);
	});

	describe('writing', () => {
		beforeEach(() => {
			context.setConfig({ pageSize: PAGE_SIZE, orderBy: 'updateDate', orderDirection: 'desc' });
		});

		it('remembers the filter term', () => {
			context.setFilter({ filter: 'news' });
			expect(context.interactionMemory.getMemory(UMB_COLLECTION_FILTER_MEMORY_UNIQUE)?.value).to.eql({
				filter: 'news',
			});
		});

		it('forgets the filter term when it is cleared', () => {
			context.setFilter({ filter: 'news' });
			context.setFilter({ filter: '' });
			expect(context.interactionMemory.getMemory(UMB_COLLECTION_FILTER_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('remembers the ordering', () => {
			context.setFilter({ orderBy: 'name', orderDirection: 'asc' });
			expect(context.interactionMemory.getMemory(UMB_COLLECTION_ORDER_MEMORY_UNIQUE)?.value).to.eql({
				orderBy: 'name',
				orderDirection: 'asc',
			});
		});

		it('does not remember the ordering when it matches the configured ordering', () => {
			context.setFilter({ orderBy: 'updateDate', orderDirection: 'desc' });
			expect(context.interactionMemory.getMemory(UMB_COLLECTION_ORDER_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('does not remember the ordering when it matches the default filter of the collection', () => {
			const contextWithDefaultOrder = new UmbDefaultCollectionContext<any, UmbTestCollectionFilterModel>(
				hostElement,
				DEFAULT_VIEW_ALIAS,
				{ orderBy: 'name', orderDirection: 'asc' },
			);
			contextWithDefaultOrder.setConfig({ pageSize: PAGE_SIZE });

			contextWithDefaultOrder.setFilter({ orderBy: 'name', orderDirection: 'asc' });

			expect(contextWithDefaultOrder.interactionMemory.getMemory(UMB_COLLECTION_ORDER_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('remembers the page number', () => {
			context.setFilter({ skip: PAGE_SIZE * 2 });
			expect(context.interactionMemory.getMemory(UMB_COLLECTION_PAGINATION_MEMORY_UNIQUE)?.value).to.eql({
				pageNumber: 3,
			});
		});

		it('does not remember the first page', () => {
			context.setFilter({ skip: PAGE_SIZE * 2 });
			context.setFilter({ skip: 0 });
			expect(context.interactionMemory.getMemory(UMB_COLLECTION_PAGINATION_MEMORY_UNIQUE)).to.be.undefined;
		});
	});

	describe('reading', () => {
		it('applies the remembered filter term, ordering and page number', () => {
			context.interactionMemory.setMemory({
				unique: UMB_COLLECTION_FILTER_MEMORY_UNIQUE,
				value: { filter: 'news' },
			});
			context.interactionMemory.setMemory({
				unique: UMB_COLLECTION_ORDER_MEMORY_UNIQUE,
				value: { orderBy: 'name', orderDirection: 'asc' },
			});
			context.interactionMemory.setMemory({
				unique: UMB_COLLECTION_PAGINATION_MEMORY_UNIQUE,
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
				unique: UMB_COLLECTION_FILTER_MEMORY_UNIQUE,
				value: { filter: 'news' },
			});

			expect(getFilter()).to.include({ filter: 'news' });
		});

		it('leaves the configured ordering alone when nothing is remembered', () => {
			context.setConfig({ pageSize: PAGE_SIZE, orderBy: 'updateDate', orderDirection: 'desc' });

			expect(getFilter()).to.include({ orderBy: 'updateDate', orderDirection: 'desc', skip: 0 });
		});
	});
});
