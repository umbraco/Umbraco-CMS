import { UmbSortChildrenOfMediaServerDataSource } from './sort-children-of.server.data.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { ContentSortFieldModel, DirectionModel, MediaService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-sort-children-of-media-data-source-host')
class UmbTestSortChildrenOfMediaDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSortChildrenOfMediaServerDataSource', () => {
	let hostElement: UmbTestSortChildrenOfMediaDataSourceHostElement;
	let dataSource: UmbSortChildrenOfMediaServerDataSource;

	const original = {
		byId: MediaService.putMediaByIdSortChildren,
		root: MediaService.putMediaRootSortChildren,
	};

	let byIdOptions: any;
	let rootOptions: any;

	beforeEach(() => {
		hostElement = new UmbTestSortChildrenOfMediaDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbSortChildrenOfMediaServerDataSource(hostElement);

		byIdOptions = undefined;
		rootOptions = undefined;

		(MediaService as any).putMediaByIdSortChildren = (options: any) => {
			byIdOptions = options;
			return Promise.resolve({ data: undefined });
		};
		(MediaService as any).putMediaRootSortChildren = (options: any) => {
			rootOptions = options;
			return Promise.resolve({ data: undefined });
		};
	});

	afterEach(() => {
		(MediaService as any).putMediaByIdSortChildren = original.byId;
		(MediaService as any).putMediaRootSortChildren = original.root;
		document.body.innerHTML = '';
	});

	it('sorts a node via the by-id endpoint, without a culture', async () => {
		const { error } = await dataSource.sortChildrenOfByField({
			unique: 'media-id',
			field: ContentSortFieldModel.UPDATE_DATE,
			direction: DirectionModel.DESCENDING,
			culture: 'da',
		});

		expect(error).to.be.undefined;
		expect(rootOptions).to.be.undefined;
		expect(byIdOptions?.path?.id).to.equal('media-id');
		expect(byIdOptions?.body).to.eql({ field: 'UpdateDate', direction: 'Descending' });
		expect(byIdOptions?.body?.culture).to.be.undefined;
	});

	it('sorts the root via the root endpoint', async () => {
		const { error } = await dataSource.sortChildrenOfByField({
			unique: null,
			field: ContentSortFieldModel.NAME,
			direction: DirectionModel.ASCENDING,
		});

		expect(error).to.be.undefined;
		expect(byIdOptions).to.be.undefined;
		expect(rootOptions?.body).to.eql({ field: 'Name', direction: 'Ascending' });
	});
});
