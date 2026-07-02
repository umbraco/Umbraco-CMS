import { UmbTableTreeViewRowController } from './table-tree-view-row.controller.js';
import type { UmbTreeItemModel } from '../../types.js';
import { UMB_TREE_ITEM_CONTEXT } from '../../tree-item/tree-item.context.token.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';

@customElement('umb-test-table-tree-view-row-child')
class UmbTestTableTreeViewRowChildElement extends UmbElementMixin(HTMLElement) {}

type Spy = (() => void) & { callCount: number };

function createSpy(): Spy {
	const spy = (() => {
		spy.callCount++;
	}) as Spy;
	spy.callCount = 0;
	return spy;
}

function createCallbacks() {
	return {
		onNoAccessChange: createSpy(),
		onPathChange: createSpy(),
		onActiveChange: createSpy(),
	};
}

function resolvesWithin(promise: Promise<unknown>, ms: number): Promise<boolean> {
	return Promise.race([
		promise.then(
			() => true,
			() => false,
		),
		new Promise<boolean>((resolve) => {
			setTimeout(() => resolve(false), ms);
		}),
	]);
}

const treeItem: UmbTreeItemModel = {
	unique: 'unique-1',
	entityType: 'test-entity',
	name: 'Test Item',
	hasChildren: false,
	isFolder: false,
	parent: { unique: null, entityType: 'test-entity' },
};

describe('UmbTableTreeViewRowController', () => {
	let host: HTMLElement;
	let child: UmbTestTableTreeViewRowChildElement;
	let controller: UmbTableTreeViewRowController | undefined;

	beforeEach(() => {
		host = document.createElement('div');
		child = new UmbTestTableTreeViewRowChildElement();
		host.appendChild(child);
		document.body.appendChild(host);
	});

	afterEach(() => {
		controller?.destroy();
		controller = undefined;
		host.remove();
	});

	describe('context provision', () => {
		it('provides the tree item api context on the row element', async () => {
			controller = new UmbTableTreeViewRowController(host, treeItem.entityType, treeItem.unique, treeItem);

			const api = await child.getContext(UMB_TREE_ITEM_CONTEXT);

			expect(api).to.exist;
		});

		it('provides an entity context with the row entity type and unique', async () => {
			controller = new UmbTableTreeViewRowController(host, treeItem.entityType, treeItem.unique, treeItem);

			const entityContext = await child.getContext(UMB_ENTITY_CONTEXT);

			expect(entityContext?.getEntityType()).to.equal('test-entity');
			expect(entityContext?.getUnique()).to.equal('unique-1');
		});
	});

	describe('observeApi', () => {
		it('emits the initial values synchronously on subscribe', () => {
			controller = new UmbTableTreeViewRowController(host, treeItem.entityType, treeItem.unique, treeItem);
			const callbacks = createCallbacks();

			controller.observeApi(callbacks);

			expect(callbacks.onNoAccessChange.callCount).to.equal(1);
			expect(callbacks.onPathChange.callCount).to.equal(1);
			expect(callbacks.onActiveChange.callCount).to.equal(1);
		});

		it('defaults currentNoAccess to false for an accessible item', () => {
			controller = new UmbTableTreeViewRowController(host, treeItem.entityType, treeItem.unique, treeItem);

			controller.observeApi(createCallbacks());

			expect(controller.currentNoAccess).to.be.false;
		});

		it('reflects noAccess from the tree item', () => {
			const noAccessItem: UmbTreeItemModel = { ...treeItem, noAccess: true };
			controller = new UmbTableTreeViewRowController(host, noAccessItem.entityType, noAccessItem.unique, noAccessItem);

			controller.observeApi(createCallbacks());

			expect(controller.currentNoAccess).to.be.true;
		});
	});

	describe('setItem', () => {
		it('updates currentNoAccess and notifies when access changes', () => {
			controller = new UmbTableTreeViewRowController(host, treeItem.entityType, treeItem.unique, treeItem);
			const callbacks = createCallbacks();
			controller.observeApi(callbacks);

			const callsBefore = callbacks.onNoAccessChange.callCount;
			controller.setItem(treeItem.entityType, treeItem.unique, { ...treeItem, noAccess: true });

			expect(controller.currentNoAccess).to.be.true;
			expect(callbacks.onNoAccessChange.callCount).to.be.greaterThan(callsBefore);
		});
	});

	describe('destroy', () => {
		it('removes the provided contexts', async () => {
			controller = new UmbTableTreeViewRowController(host, treeItem.entityType, treeItem.unique, treeItem);
			expect(await resolvesWithin(child.getContext(UMB_TREE_ITEM_CONTEXT), 100)).to.be.true;

			controller.destroy();
			controller = undefined;

			expect(await resolvesWithin(child.getContext(UMB_TREE_ITEM_CONTEXT), 50)).to.be.false;
		});

		it('can be called multiple times without throwing', () => {
			controller = new UmbTableTreeViewRowController(host, treeItem.entityType, treeItem.unique, treeItem);
			controller.observeApi(createCallbacks());

			expect(() => {
				controller!.destroy();
				controller!.destroy();
			}).to.not.throw();
			controller = undefined;
		});
	});
});
