import {
	UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import { UmbDocumentBlueprintTreeServerDataSource } from './document-blueprint-tree.server.data-source.js';
import { useMockHandlers, resetMockHandlers } from '../../../../../../mocks/index.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

@customElement('test-document-blueprint-tree-data-source-host')
class UmbTestDocumentBlueprintTreeDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// From the default mock data set, which every test run loads.
const ROOT_BLUEPRINT_ID = 'the-simplest-document-id';
const OUTER_FOLDER_ID = 'document-blueprint-outer-folder-id';
const INNER_FOLDER_ID = 'document-blueprint-inner-folder-id';
const BLUEPRINT_IN_INNER_FOLDER_ID = 'document-blueprint-in-inner-folder-id';

describe('UmbDocumentBlueprintTreeServerDataSource', () => {
	let hostElement: UmbTestDocumentBlueprintTreeDataSourceHostElement;
	let dataSource: UmbDocumentBlueprintTreeServerDataSource;

	const rootItems = async () => {
		const { data } = await dataSource.getRootItems({ paging: { skip: 0, take: 100 } });
		expect(data).to.not.be.undefined;
		return data!.items;
	};

	const childrenOf = async (unique: string, entityType: string) => {
		const { data } = await dataSource.getChildrenOf({
			parent: { unique, entityType },
			paging: { skip: 0, take: 100 },
		});
		expect(data).to.not.be.undefined;
		return data!.items;
	};

	beforeEach(() => {
		hostElement = new UmbTestDocumentBlueprintTreeDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDocumentBlueprintTreeServerDataSource(hostElement);
	});

	afterEach(() => {
		resetMockHandlers();
		hostElement.remove();
	});

	describe('access', () => {
		it('reports no access for a folder the user may only browse through', async () => {
			const outerFolder = (await rootItems()).find((item) => item.unique === OUTER_FOLDER_ID);

			expect(outerFolder).to.not.be.undefined;
			expect(outerFolder!.noAccess).to.be.true;
		});

		it('reports access for the folder below it', async () => {
			const innerFolder = (
				await childrenOf(OUTER_FOLDER_ID, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE)
			).find((item) => item.unique === INNER_FOLDER_ID);

			expect(innerFolder).to.not.be.undefined;
			expect(innerFolder!.noAccess).to.be.false;
		});
	});

	describe('folders and blueprints', () => {
		it('maps a folder to the folder entity type', async () => {
			const outerFolder = (await rootItems()).find((item) => item.unique === OUTER_FOLDER_ID)!;

			expect(outerFolder.entityType).to.equal(UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE);
			expect(outerFolder.isFolder).to.be.true;
			expect(outerFolder.icon).to.equal('icon-folder');
		});

		it('maps a blueprint to the blueprint entity type', async () => {
			const blueprint = (await rootItems()).find((item) => item.unique === ROOT_BLUEPRINT_ID)!;

			expect(blueprint.entityType).to.equal(UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE);
			expect(blueprint.isFolder).to.be.false;
			expect(blueprint.icon).to.equal('icon-blueprint');
		});

		it('reports children for a folder that holds something, so it can be expanded', async () => {
			const innerFolder = (
				await childrenOf(OUTER_FOLDER_ID, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE)
			).find((item) => item.unique === INNER_FOLDER_ID)!;

			expect(innerFolder.hasChildren).to.be.true;
		});
	});

	describe('naming', () => {
		it('names a folder from the item, since a folder has no variants', async () => {
			const outerFolder = (await rootItems()).find((item) => item.unique === OUTER_FOLDER_ID)!;

			expect(outerFolder.name).to.equal('Outer Document Blueprint Folder Without Access');
		});

		it('prefers the first variant name over the item name', async () => {
			useMockHandlers(
				http.get(umbracoPath('/tree/document-blueprint/root'), () =>
					HttpResponse.json({
						items: [
							{
								id: 'a-blueprint-named-twice',
								name: 'Item Name',
								isFolder: false,
								hasChildren: false,
								noAccess: false,
								parent: null,
								documentType: { id: 'the-simplest-document-type-id', icon: 'icon-document' },
								variants: [{ name: 'Variant Name' }],
							},
						],
						total: 1,
					}),
				),
			);

			const item = (await rootItems())[0];

			expect(item.name).to.equal('Variant Name');
		});
	});

	describe('parent', () => {
		it('reports the root as the parent of a root level item', async () => {
			const blueprint = (await rootItems()).find((item) => item.unique === ROOT_BLUEPRINT_ID)!;

			expect(blueprint.parent.unique).to.be.null;
			expect(blueprint.parent.entityType).to.equal(UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE);
		});

		it('reports the holding folder as the parent of a nested blueprint', async () => {
			const blueprint = (
				await childrenOf(INNER_FOLDER_ID, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE)
			).find((item) => item.unique === BLUEPRINT_IN_INNER_FOLDER_ID)!;

			expect(blueprint.parent.unique).to.equal(INNER_FOLDER_ID);
		});
	});
});
