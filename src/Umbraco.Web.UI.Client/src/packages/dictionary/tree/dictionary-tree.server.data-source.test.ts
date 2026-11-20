import { UmbDictionaryTreeServerDataSource } from './dictionary-tree.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

@customElement('test-dictionary-tree-data-source-host')
class UmbTestDictionaryTreeDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbNotificationContext(this);
	}
}

const makeTreeItem = (id: string) => ({ id, name: `Item ${id}`, hasChildren: false, parent: null, flags: [] });

describe('UmbDictionaryTreeServerDataSource', () => {
	let hostElement: UmbTestDictionaryTreeDataSourceHostElement;
	let dataSource: UmbDictionaryTreeServerDataSource;

	const originalGetBatch = DictionaryService.getTreeDictionaryBatch;

	beforeEach(() => {
		hostElement = new UmbTestDictionaryTreeDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDictionaryTreeServerDataSource(hostElement);
	});

	afterEach(() => {
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		(DictionaryService as any).getTreeDictionaryBatch = originalGetBatch;
		dataSource.destroy();
		document.body.innerHTML = '';
	});

	describe('getItems', () => {
		it('returns the mapped items of every chunk in the requested order', async () => {
			const uniques = Array.from({ length: 45 }, (_, index) => `item-${index}`);
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			(DictionaryService as any).getTreeDictionaryBatch = (options: { query: { id: Array<string> } }) =>
				Promise.resolve({ data: { total: options.query.id.length, items: options.query.id.map(makeTreeItem) } });

			const { data, error } = await dataSource.getItems({ uniques });

			expect(error).to.be.undefined;
			expect(data?.map((item) => item.unique)).to.deep.equal(uniques);
		});

		it('returns an error when the batch request fails', async () => {
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			(DictionaryService as any).getTreeDictionaryBatch = () => Promise.reject(new Error('Request failed'));

			const { data, error } = await dataSource.getItems({ uniques: ['item-0'] });

			expect(data).to.be.undefined;
			expect(error).to.not.be.undefined;
		});
	});
});
