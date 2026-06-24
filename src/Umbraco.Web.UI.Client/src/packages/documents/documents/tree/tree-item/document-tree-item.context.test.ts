import { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import type { UmbDocumentTreeItemModel } from '../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbDefaultTreeContext, UmbTreeItemOpenEvent } from '@umbraco-cms/backoffice/tree';
import { aTimeout, expect, oneEvent } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-test-document-tree-item-host')
class UmbTestDocumentTreeItemHostElement extends UmbElementMixin(HTMLElement) {}

function createTreeItem(hasCollection: boolean): UmbDocumentTreeItemModel {
	return {
		unique: 'document-unique-id',
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		name: 'Test Document',
		hasChildren: true,
		isFolder: false,
		parent: {
			unique: null,
			entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE,
		},
		ancestors: [],
		noAccess: false,
		isTrashed: false,
		isProtected: false,
		documentType: {
			unique: 'document-type-unique-id',
			icon: 'icon-document',
			collection: hasCollection ? { unique: 'collection-unique-id' } : null,
		},
		createDate: '2024-01-01T00:00:00Z',
		variants: [],
		flags: [],
	};
}

describe('UmbDocumentTreeItemContext', () => {
	let host: UmbTestDocumentTreeItemHostElement;
	let treeContext: UmbDefaultTreeContext<UmbDocumentTreeItemModel>;
	let context: UmbDocumentTreeItemContext;

	// Stubs/spies (no sinon in this project).
	let pushStateCalls: Array<{ url: string }>;
	const originalPushState = history.pushState;
	let expandCalls: number;
	let collapseCalls: number;

	beforeEach(async () => {
		host = new UmbTestDocumentTreeItemHostElement();
		document.body.appendChild(host);

		treeContext = new UmbDefaultTreeContext(host);

		expandCalls = 0;
		collapseCalls = 0;
		treeContext.expansion.expandItem = async () => {
			expandCalls++;
		};
		treeContext.expansion.collapseItem = async () => {
			collapseCalls++;
		};

		pushStateCalls = [];
		history.pushState = (_data: unknown, _unused: string, url?: string | URL | null) => {
			pushStateCalls.push({ url: String(url) });
		};

		context = new UmbDocumentTreeItemContext(host);

		// Wait for the tree context to be consumed by the item context.
		await aTimeout(0);
	});

	afterEach(() => {
		history.pushState = originalPushState;
		document.body.removeChild(host);
	});

	describe('collection item in a menu', () => {
		beforeEach(async () => {
			context.setIsMenu(true);
			context.setTreeItem(createTreeItem(true));
			// Let the children manager settle its expansion observer while the tree context is alive.
			await aTimeout(0);
		});

		it('navigates to the Collection view on showChildren instead of expanding', () => {
			context.showChildren();

			expect(pushStateCalls.length).to.equal(1);
			expect(pushStateCalls[0].url).to.contain('openCollection=true');
			expect(expandCalls).to.equal(0);
		});

		it('navigates to the Collection view on hideChildren instead of collapsing', () => {
			context.hideChildren();

			expect(pushStateCalls.length).to.equal(1);
			expect(pushStateCalls[0].url).to.contain('openCollection=true');
			expect(collapseCalls).to.equal(0);
		});
	});

	describe('collection item in a picker', () => {
		beforeEach(async () => {
			// A picker is not a menu.
			context.setTreeItem(createTreeItem(true));
			await aTimeout(0);
		});

		it('emits the open event on showChildren instead of expanding', async () => {
			const listener = oneEvent(host, UmbTreeItemOpenEvent.TYPE);

			context.showChildren();

			const event = (await listener) as UmbTreeItemOpenEvent;
			expect(event.unique).to.equal('document-unique-id');
			expect(event.entityType).to.equal(UMB_DOCUMENT_ENTITY_TYPE);
			expect(expandCalls).to.equal(0);
			expect(pushStateCalls.length).to.equal(0);
		});

		it('emits the open event on hideChildren instead of collapsing', async () => {
			const listener = oneEvent(host, UmbTreeItemOpenEvent.TYPE);

			context.hideChildren();

			const event = (await listener) as UmbTreeItemOpenEvent;
			expect(event.unique).to.equal('document-unique-id');
			expect(event.entityType).to.equal(UMB_DOCUMENT_ENTITY_TYPE);
			expect(collapseCalls).to.equal(0);
			expect(pushStateCalls.length).to.equal(0);
		});
	});

	describe('non-collection item', () => {
		beforeEach(async () => {
			context.setTreeItem(createTreeItem(false));
			await aTimeout(0);
		});

		it('expands its children on showChildren', () => {
			context.showChildren();

			expect(expandCalls).to.equal(1);
			expect(pushStateCalls.length).to.equal(0);
		});

		it('collapses its children on hideChildren', () => {
			context.hideChildren();

			expect(collapseCalls).to.equal(1);
			expect(pushStateCalls.length).to.equal(0);
		});
	});
});
