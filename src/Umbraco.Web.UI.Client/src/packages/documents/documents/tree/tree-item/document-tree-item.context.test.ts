import { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import type { UmbDocumentTreeItemModel } from '../types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-test-document-tree-item-host')
class UmbTestDocumentTreeItemHostElement extends UmbElementMixin(HTMLElement) {}

function treeItem(overrides: Partial<UmbDocumentTreeItemModel>): UmbDocumentTreeItemModel {
	return {
		unique: 'test-unique-id',
		entityType: 'document',
		name: 'Test Item',
		hasChildren: true,
		isFolder: false,
		noAccess: false,
		isTrashed: false,
		isProtected: false,
		ancestors: [],
		createDate: '2024-01-01',
		variants: [],
		flags: [],
		parent: {
			unique: null,
			entityType: 'document-root',
		},
		documentType: {
			unique: 'document-type-unique',
			icon: 'icon-document',
			collection: null,
		},
		...overrides,
	} as UmbDocumentTreeItemModel;
}

describe('UmbDocumentTreeItemContext - collection start node access', () => {
	let host: UmbTestDocumentTreeItemHostElement;
	let context: UmbDocumentTreeItemContext;
	let originalPushState: typeof history.pushState;
	let pushedUrls: Array<string>;

	beforeEach(() => {
		host = new UmbTestDocumentTreeItemHostElement();
		document.body.appendChild(host);
		context = new UmbDocumentTreeItemContext(host);
		// Avoid depending on the section context for path construction.
		(context as unknown as { getPath: () => string }).getPath = () => '/test-path';

		pushedUrls = [];
		originalPushState = history.pushState;
		history.pushState = ((data: unknown, unused: string, url?: string | URL | null) => {
			if (url != null) pushedUrls.push(url.toString());
		}) as typeof history.pushState;
	});

	afterEach(() => {
		history.pushState = originalPushState;
		document.body.removeChild(host);
	});

	const collection = { unique: 'collection-unique' };

	it('opens the collection view instead of expanding for an accessible collection', () => {
		context.setIsMenu(true);
		context.setTreeItem(treeItem({ noAccess: false, documentType: { unique: 'dt', icon: 'icon', collection } }));

		context.showChildren();

		expect(pushedUrls.some((url) => url.includes('openCollection=true'))).to.be.true;
	});

	it('expands the tree (does not divert to the collection view) for a no-access collection ancestor', () => {
		// A "no access" collection node is an ancestor of the user's start node and must remain
		// expandable so the user can browse down to their start node.
		context.setIsMenu(true);
		context.setTreeItem(treeItem({ noAccess: true, documentType: { unique: 'dt', icon: 'icon', collection } }));

		context.showChildren();

		expect(pushedUrls.some((url) => url.includes('openCollection=true'))).to.be.false;
	});
});
