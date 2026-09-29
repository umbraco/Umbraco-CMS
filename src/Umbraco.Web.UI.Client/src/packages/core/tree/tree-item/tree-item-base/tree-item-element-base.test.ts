import { UmbTreeItemElementBase } from './tree-item-element-base.js';
import { UmbTreeItemContextBase } from './tree-item-context-base.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';

@customElement('umb-test-tree-item-element-base')
class UmbTestTreeItemElement extends UmbTreeItemElementBase<UmbTreeItemModel> {}

class UmbTestTreeItemContext extends UmbTreeItemContextBase<UmbTreeItemModel, UmbTreeRootModel> {
	public setTreeItemCallCount = 0;

	constructor(host: UmbControllerHost) {
		super(host);
	}

	public override setTreeItem(treeItem: UmbTreeItemModel | undefined) {
		this.setTreeItemCallCount++;
		super.setTreeItem(treeItem);
	}
}

const treeItem: UmbTreeItemModel = {
	unique: 'test-unique-id',
	entityType: 'test-entity-type',
	name: 'Test Item',
	hasChildren: false,
	isFolder: false,
	parent: {
		unique: null,
		entityType: 'test-root-entity-type',
	},
};

describe('UmbTreeItemElementBase', () => {
	let element: UmbTestTreeItemElement;
	let context: UmbTestTreeItemContext;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-tree-item-element-base></umb-test-tree-item-element-base>`);
		context = new UmbTestTreeItemContext(element);
		element.api = context;
	});

	describe('item', () => {
		it('initializes the api when an item is set', () => {
			element.item = treeItem;
			expect(context.setTreeItemCallCount).to.equal(1);
		});

		it('ignores the item object it already holds', () => {
			element.item = treeItem;
			element.item = treeItem;
			expect(context.setTreeItemCallCount).to.equal(1);
		});

		it('initializes the api again when a different item object is set', () => {
			element.item = treeItem;
			element.item = { ...treeItem, name: 'Renamed Item' };
			expect(context.setTreeItemCallCount).to.equal(2);
		});
	});

	describe('rendered child items', () => {
		const childA: UmbTreeItemModel = { ...treeItem, unique: 'child-a', name: 'Child A' };
		const childB: UmbTreeItemModel = { ...treeItem, unique: 'child-b', name: 'Child B' };
		const childC: UmbTreeItemModel = { ...treeItem, unique: 'child-c', name: 'Child C' };

		const childElements = () => Array.from(element.shadowRoot!.querySelectorAll('umb-tree-item'));

		const setChildItems = async (items: Array<UmbTreeItemModel>) => {
			(element as unknown as { _childItems: Array<UmbTreeItemModel> })._childItems = items;
			element.requestUpdate();
			await element.updateComplete;
		};

		it('keeps the same elements when the items are reordered', async () => {
			await setChildItems([childA, childB, childC]);
			const [a, b, c] = childElements();

			await setChildItems([childC, childA, childB]);

			expect(childElements()).to.deep.equal([c, a, b]);
		});

		it('keeps the same element when an item is renamed', async () => {
			await setChildItems([childA]);
			const [before] = childElements();

			await setChildItems([{ ...childA, name: 'Renamed Child' }]);

			expect(childElements()[0]).to.equal(before);
		});

		it('keeps the existing elements when items are prepended', async () => {
			await setChildItems([childB, childC]);
			const [b, c] = childElements();

			await setChildItems([childA, childB, childC]);

			const after = childElements();
			expect(after).to.have.lengthOf(3);
			expect(after[1]).to.equal(b);
			expect(after[2]).to.equal(c);
		});

		it('keeps the existing elements when items are appended', async () => {
			await setChildItems([childA]);
			const [a] = childElements();

			await setChildItems([childA, childB]);

			expect(childElements()[0]).to.equal(a);
		});

		it('removes the element of an item that is no longer in the list', async () => {
			await setChildItems([childA, childB]);

			await setChildItems([childA]);

			expect(childElements()).to.have.lengthOf(1);
		});
	});
});
