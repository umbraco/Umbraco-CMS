import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentTreeServerDataSource } from './document-tree.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-test-document-tree-data-source-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const documentTreeItem = (unique: string) => ({ unique, entityType: UMB_DOCUMENT_ENTITY_TYPE });

const makeAncestor = (id: string) => ({
	id,
	documentType: { id: 'document-type', icon: 'icon-folder', collection: null },
	hasChildren: true,
	isProtected: false,
	isTrashed: false,
	parent: null,
	variants: [{ culture: null, name: id, state: 'Published', flags: [] }],
	flags: [],
});

describe('UmbDocumentTreeServerDataSource', () => {
	let hostElement: UmbTestHostElement;
	let dataSource: UmbDocumentTreeServerDataSource;
	let requests: Array<Array<string>>;

	const originalAncestors = DocumentService.getItemDocumentAncestors;

	beforeEach(() => {
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDocumentTreeServerDataSource(hostElement);

		requests = [];
		(DocumentService as any).getItemDocumentAncestors = (options: { query: { id: Array<string> } }) => {
			requests.push([...options.query.id]);
			return Promise.resolve({
				data: options.query.id.map((id) => ({
					id,
					ancestors: [makeAncestor(`root-of-${id}`), makeAncestor(`parent-of-${id}`)],
				})),
			});
		};
	});

	afterEach(() => {
		(DocumentService as any).getItemDocumentAncestors = originalAncestors;
		document.body.innerHTML = '';
	});

	describe('getAncestorsOfItems', () => {
		it('returns the ancestors of each item as document references, ordered from the root down', async () => {
			const { data } = await dataSource.getAncestorsOfItems({
				treeItems: [documentTreeItem('doc-1'), documentTreeItem('doc-2')],
			});

			expect(requests).to.have.lengthOf(1);
			expect(data).to.deep.equal([
				{
					treeItem: documentTreeItem('doc-1'),
					ancestors: [
						{ unique: 'root-of-doc-1', entityType: UMB_DOCUMENT_ENTITY_TYPE },
						{ unique: 'parent-of-doc-1', entityType: UMB_DOCUMENT_ENTITY_TYPE },
					],
				},
				{
					treeItem: documentTreeItem('doc-2'),
					ancestors: [
						{ unique: 'root-of-doc-2', entityType: UMB_DOCUMENT_ENTITY_TYPE },
						{ unique: 'parent-of-doc-2', entityType: UMB_DOCUMENT_ENTITY_TYPE },
					],
				},
			]);
		});

		it('does not send a request when no items are given', async () => {
			const { data } = await dataSource.getAncestorsOfItems({ treeItems: [] });

			expect(requests).to.have.lengthOf(0);
			expect(data).to.deep.equal([]);
		});

		it('returns no ancestors for tree items that are not documents, without requesting them', async () => {
			const rootTreeItem = { unique: 'root', entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE };

			const { data } = await dataSource.getAncestorsOfItems({ treeItems: [rootTreeItem, documentTreeItem('doc-1')] });

			expect(requests).to.deep.equal([['doc-1']]);
			expect(data?.[0]).to.deep.equal({ treeItem: rootTreeItem, ancestors: [] });
			expect(data?.[1].ancestors).to.have.lengthOf(2);
		});

		it('returns an error instead of partial data when one of the batches fails', async () => {
			let callCount = 0;
			(DocumentService as any).getItemDocumentAncestors = (options: { query: { id: Array<string> } }) => {
				callCount++;
				if (callCount === 2) return Promise.reject(new Error('Simulated server error'));
				return Promise.resolve({ data: options.query.id.map((id) => ({ id, ancestors: [] })) });
			};

			const treeItems = Array.from({ length: 50 }, (_, i) => documentTreeItem(`doc-${i}`));
			const { data, error } = await dataSource.getAncestorsOfItems({ treeItems });

			expect(error).to.exist;
			expect(data).to.be.undefined;
		});
	});
});
