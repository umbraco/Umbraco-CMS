import { UmbElementSearchServerDataSource } from './element-search.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-element-search-data-source-host')
class UmbTestElementSearchDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbElementSearchServerDataSource', () => {
	let hostElement: UmbTestElementSearchDataSourceHostElement;
	let dataSource: UmbElementSearchServerDataSource;
	let searchQuery: Record<string, unknown> | undefined;

	const originalSearch = ElementService.getItemElementSearch;
	const originalAncestors = ElementService.getItemElementAncestors;

	beforeEach(() => {
		hostElement = new UmbTestElementSearchDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbElementSearchServerDataSource(hostElement);

		searchQuery = undefined;
		(ElementService as any).getItemElementSearch = (options: { query: Record<string, unknown> }) => {
			searchQuery = options.query;
			return Promise.resolve({ data: { items: [], total: 0 } });
		};
		(ElementService as any).getItemElementAncestors = () => Promise.resolve({ data: [] });
	});

	afterEach(() => {
		(ElementService as any).getItemElementSearch = originalSearch;
		(ElementService as any).getItemElementAncestors = originalAncestors;
		hostElement.remove();
	});

	it('forwards the search arguments to the server', async () => {
		await dataSource.search({
			query: 'teaser',
			culture: 'da-DK',
			includeTrashed: false,
			searchFrom: { unique: 'folder-1', entityType: 'element-folder' },
			allowedContentTypes: [{ unique: 'element-type-1', entityType: 'document-type' }],
			paging: { skip: 10, take: 20 },
		});

		expect(searchQuery).to.deep.equal({
			allowedElementTypes: ['element-type-1'],
			culture: 'da-DK',
			parentId: 'folder-1',
			query: 'teaser',
			trashed: false,
			skip: 10,
			take: 20,
		});
	});

	it('omits the culture when none is given', async () => {
		await dataSource.search({ query: 'teaser', culture: null });

		expect(searchQuery?.culture).to.be.undefined;
	});
});
