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
});
