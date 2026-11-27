import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentTreeServerDataSource } from './document-tree.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-test-document-tree-data-source-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const documentTreeItem = (unique: string) => ({ unique, entityType: UMB_DOCUMENT_ENTITY_TYPE });

const makeTreeItemResponse = (id: string, ancestorIds: Array<string>) => ({
	id,
	ancestors: ancestorIds.map((ancestorId) => ({ id: ancestorId })),
	parent: ancestorIds.length ? { id: ancestorIds[ancestorIds.length - 1] } : null,
	documentType: { id: 'document-type', icon: 'icon-folder', collection: null },
	noAccess: false,
	isTrashed: false,
	hasChildren: true,
	isProtected: false,
	flags: [],
	createDate: '',
	variants: [{ culture: null, name: `Name of ${id}`, state: 'Published', flags: [] }],
});

describe('UmbDocumentTreeServerDataSource', () => {
	let hostElement: UmbTestHostElement;
	let dataSource: UmbDocumentTreeServerDataSource;
	let requests: Array<Array<string>>;
	let requestDataTypeIds: Array<string | undefined>;
	let ancestorIdsOf: Record<string, Array<string>>;

	const originalAncestorsBatch = DocumentService.getTreeDocumentAncestorsBatch;

	beforeEach(() => {
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDocumentTreeServerDataSource(hostElement);

		requests = [];
		requestDataTypeIds = [];
		ancestorIdsOf = {
			'doc-1': ['root', 'folder-1'],
			'doc-2': ['root', 'folder-2'],
			root: [],
			'folder-1': ['root'],
			'folder-2': ['root'],
		};
		(DocumentService as any).getTreeDocumentAncestorsBatch = (options: {
			query: { id: Array<string>; dataTypeId?: string };
		}) => {
			requests.push([...options.query.id]);
			requestDataTypeIds.push(options.query.dataTypeId);
			const items = options.query.id
				.filter((id) => ancestorIdsOf[id])
				.map((id) => ({
					id,
					ancestors: ancestorIdsOf[id].map((ancestorId) => makeTreeItemResponse(ancestorId, ancestorIdsOf[ancestorId])),
				}));
			return Promise.resolve({ data: { total: items.length, items } });
		};
	});

	afterEach(() => {
		(DocumentService as any).getTreeDocumentAncestorsBatch = originalAncestorsBatch;
		document.body.innerHTML = '';
	});

	describe('getAncestorsOfItems', () => {
		const ancestorUniquesOf = (entry?: { ancestors: Array<{ unique: string | null }> }) =>
			entry?.ancestors.map((ancestor) => ancestor.unique);

		it('returns the ancestors of each item as tree items, ordered from the root down', async () => {
			const { data } = await dataSource.getAncestorsOfItems({
				treeItems: [documentTreeItem('doc-1'), documentTreeItem('doc-2')],
			});

			expect(data?.map((entry) => entry.treeItem)).to.deep.equal([
				documentTreeItem('doc-1'),
				documentTreeItem('doc-2'),
			]);
			expect(ancestorUniquesOf(data?.[0])).to.deep.equal(['root', 'folder-1']);
			expect(ancestorUniquesOf(data?.[1])).to.deep.equal(['root', 'folder-2']);
			expect(data?.[0].ancestors[1].variants[0].name).to.equal('Name of folder-1');
		});

		it('returns the requested tree items with their ancestors', async () => {
			const treeItems = [documentTreeItem('doc-1'), documentTreeItem('doc-2')];

			const { data } = await dataSource.getAncestorsOfItems({ treeItems });

			expect(data?.[0].treeItem).to.equal(treeItems[0]);
			expect(data?.[1].treeItem).to.equal(treeItems[1]);
		});

		it('requests the ancestors of all items in one request', async () => {
			await dataSource.getAncestorsOfItems({ treeItems: [documentTreeItem('doc-1'), documentTreeItem('doc-2')] });

			expect(requests).to.deep.equal([['doc-1', 'doc-2']]);
		});

		it('requests the ancestors for the given data type', async () => {
			await dataSource.getAncestorsOfItems({
				treeItems: [documentTreeItem('doc-1')],
				dataType: { unique: 'data-type-1' },
			});

			expect(requestDataTypeIds).to.deep.equal(['data-type-1']);
		});

		it('does not send a request when no items are given', async () => {
			const { data } = await dataSource.getAncestorsOfItems({ treeItems: [] });

			expect(requests).to.have.lengthOf(0);
			expect(data).to.deep.equal([]);
		});

		it('leaves out items the server does not return', async () => {
			const { data } = await dataSource.getAncestorsOfItems({
				treeItems: [documentTreeItem('unknown'), documentTreeItem('doc-1')],
			});

			expect(data?.map((entry) => entry.treeItem)).to.deep.equal([documentTreeItem('doc-1')]);
			expect(ancestorUniquesOf(data?.[0])).to.deep.equal(['root', 'folder-1']);
		});

		it('returns no ancestors for items at the root', async () => {
			const { data } = await dataSource.getAncestorsOfItems({ treeItems: [documentTreeItem('root')] });

			expect(data).to.deep.equal([{ treeItem: documentTreeItem('root'), ancestors: [] }]);
		});

		it('returns an error when the ancestors cannot be requested', async () => {
			(DocumentService as any).getTreeDocumentAncestorsBatch = () =>
				Promise.reject(new Error('Simulated server error'));

			const { data, error } = await dataSource.getAncestorsOfItems({ treeItems: [documentTreeItem('doc-1')] });

			expect(error).to.exist;
			expect(data).to.be.undefined;
		});
	});
});
