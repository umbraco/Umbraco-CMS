import type { UmbStructureItemModel } from './types.js';
import { UmbMenuListStructureWorkspaceContextBase } from './menu-list-structure-workspace-context-base.js';
import { expect } from '@open-wc/testing';
import { UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('umb-test-menu-list-structure-controller-host')
class UmbTestMenuListStructureControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class TestMenuListStructureWorkspaceContext extends UmbMenuListStructureWorkspaceContextBase {
	setStructure(items: Array<UmbStructureItemModel>) {
		this._setStructure(items);
	}
}

const ROOT_ITEM: UmbStructureItemModel = {
	unique: null,
	entityType: 'test-root',
	name: 'Root',
	isFolder: false,
};

const CURRENT_ITEM: UmbStructureItemModel = {
	unique: 'test-unique',
	entityType: 'test-entity',
	name: 'Current',
	isFolder: false,
};

describe('UmbMenuListStructureWorkspaceContextBase', () => {
	let host: UmbTestMenuListStructureControllerHostElement;
	let context: TestMenuListStructureWorkspaceContext;

	beforeEach(() => {
		host = new UmbTestMenuListStructureControllerHostElement();
		document.body.appendChild(host);

		context = new TestMenuListStructureWorkspaceContext(host);
	});

	afterEach(() => {
		context.destroy();
		document.body.removeChild(host);
	});

	it('sets the structure', async () => {
		context.setStructure([ROOT_ITEM, CURRENT_ITEM]);

		const structure = await firstValueFrom(context.structure);
		expect(structure).to.deep.equal([ROOT_ITEM, CURRENT_ITEM]);
	});

	it('sets UMB_PARENT_ENTITY_CONTEXT to the item preceding the current one', async () => {
		context.setStructure([ROOT_ITEM, CURRENT_ITEM]);

		const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
		expect(parentContext?.getParent()).to.deep.equal({ unique: null, entityType: 'test-root' });
	});

	it('sets UMB_PARENT_ENTITY_CONTEXT to the root when it is the only item (e.g. a new entity)', async () => {
		context.setStructure([ROOT_ITEM]);

		const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
		expect(parentContext?.getParent()).to.deep.equal({ unique: null, entityType: 'test-root' });
	});

	it('clears UMB_PARENT_ENTITY_CONTEXT when the structure is empty', async () => {
		context.setStructure([ROOT_ITEM]);
		context.setStructure([]);

		const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
		expect(parentContext?.getParent()).to.equal(undefined);
	});

	describe('destroy', () => {
		it('destroys the structure state', () => {
			context.destroy();
			expect(() => context.setStructure([])).to.throw();
		});
	});
});
