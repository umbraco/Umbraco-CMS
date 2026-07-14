import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { ContentSortFieldModel, DirectionModel, DocumentService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-sort-children-of-document-data-source-host')
class UmbTestSortChildrenOfDocumentDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSortChildrenOfDocumentServerDataSource', () => {
	let hostElement: UmbTestSortChildrenOfDocumentDataSourceHostElement;
	let dataSource: UmbSortChildrenOfDocumentServerDataSource;

	const original = {
		byId: DocumentService.putDocumentByIdSortChildren,
		root: DocumentService.putDocumentRootSortChildren,
	};

	let byIdOptions: any;
	let rootOptions: any;

	beforeEach(() => {
		hostElement = new UmbTestSortChildrenOfDocumentDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbSortChildrenOfDocumentServerDataSource(hostElement);

		byIdOptions = undefined;
		rootOptions = undefined;

		(DocumentService as any).putDocumentByIdSortChildren = (options: any) => {
			byIdOptions = options;
			return Promise.resolve({ data: undefined });
		};
		(DocumentService as any).putDocumentRootSortChildren = (options: any) => {
			rootOptions = options;
			return Promise.resolve({ data: undefined });
		};
	});

	afterEach(() => {
		(DocumentService as any).putDocumentByIdSortChildren = original.byId;
		(DocumentService as any).putDocumentRootSortChildren = original.root;
		document.body.innerHTML = '';
	});

	it('sorts a node via the by-id endpoint, passing field, direction and culture', async () => {
		const { error } = await dataSource.sortChildrenOfByField({
			unique: 'document-id',
			field: ContentSortFieldModel.NAME,
			direction: DirectionModel.DESCENDING,
			culture: 'da',
		});

		expect(error).to.be.undefined;
		expect(rootOptions).to.be.undefined;
		expect(byIdOptions?.path?.id).to.equal('document-id');
		expect(byIdOptions?.body).to.eql({ field: 'Name', direction: 'Descending', culture: 'da' });
	});

	it('sorts the root via the root endpoint, defaulting culture to null when none is given', async () => {
		const { error } = await dataSource.sortChildrenOfByField({
			unique: null,
			field: ContentSortFieldModel.CREATE_DATE,
			direction: DirectionModel.ASCENDING,
		});

		expect(error).to.be.undefined;
		expect(byIdOptions).to.be.undefined;
		expect(rootOptions?.body).to.eql({ field: 'CreateDate', direction: 'Ascending', culture: null });
	});
});
