import { UmbTreeItemContextBase } from './tree-item-context-base.js';
import { UmbDefaultTreeContext } from '../../default/default-tree.context.js';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';

class UmbTestTreeItemContext extends UmbTreeItemContextBase<UmbTreeItemModel, UmbTreeRootModel> {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	public get testChildrenManager() {
		return this._treeItemChildrenManager;
	}
}

@customElement('umb-test-tree-item-host')
class UmbTestTreeItemHostElement extends UmbElementMixin(HTMLElement) {}

@customElement('umb-test-tree-item-child')
class UmbTestTreeItemChildElement extends UmbElementMixin(HTMLElement) {}

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

describe('UmbTreeItemContextBase', () => {
	let context: UmbTestTreeItemContext;
	let host: UmbTestTreeItemHostElement;
	let child: UmbTestTreeItemChildElement;

	beforeEach(() => {
		host = new UmbTestTreeItemHostElement();
		child = new UmbTestTreeItemChildElement();
		host.appendChild(child);
		document.body.appendChild(host);
		context = new UmbTestTreeItemContext(host);
	});

	afterEach(() => {
		document.body.removeChild(host);
	});

	describe('Entity Context', () => {
		it('provides UMB_ENTITY_CONTEXT to descendants', async () => {
			context.setTreeItem(treeItem);
			const entityContext = await child.getContext(UMB_ENTITY_CONTEXT);
			expect(entityContext).to.not.be.undefined;
		});

		it('sets entityType on the entity context', async () => {
			context.setTreeItem(treeItem);
			const entityContext = await child.getContext(UMB_ENTITY_CONTEXT);
			expect(entityContext!.getEntityType()).to.equal('test-entity-type');
		});

		it('sets unique on the entity context', async () => {
			context.setTreeItem(treeItem);
			const entityContext = await child.getContext(UMB_ENTITY_CONTEXT);
			expect(entityContext!.getUnique()).to.equal('test-unique-id');
		});

		it('clears entity context when tree item is set to undefined', async () => {
			context.setTreeItem(treeItem);
			const entityContext = await child.getContext(UMB_ENTITY_CONTEXT);
			expect(entityContext!.getEntityType()).to.equal('test-entity-type');
			expect(entityContext!.getUnique()).to.equal('test-unique-id');

			context.setTreeItem(undefined);
			expect(entityContext!.getEntityType()).to.be.undefined;
			expect(entityContext!.getUnique()).to.be.null;
		});
	});

	describe('Additional Request Args', () => {
		it('forwards the tree context additional request args to the per-item children manager', async () => {
			const treeContext = new UmbDefaultTreeContext(host);
			treeContext.updateAdditionalRequestArgs({ dataType: { unique: 'test-data-type-id' } } as any);

			context.setTreeItem(treeItem);
			await aTimeout(0);

			expect(context.testChildrenManager.getAdditionalRequestArgs()).to.deep.equal({
				dataType: { unique: 'test-data-type-id' },
			});
		});

		it('propagates later updates to the tree context additional request args', async () => {
			const treeContext = new UmbDefaultTreeContext(host);

			context.setTreeItem(treeItem);
			await aTimeout(0);

			treeContext.updateAdditionalRequestArgs({ dataType: { unique: 'updated-data-type-id' } } as any);
			await aTimeout(0);

			expect(context.testChildrenManager.getAdditionalRequestArgs()).to.deep.equal({
				dataType: { unique: 'updated-data-type-id' },
			});
		});
	});
});
