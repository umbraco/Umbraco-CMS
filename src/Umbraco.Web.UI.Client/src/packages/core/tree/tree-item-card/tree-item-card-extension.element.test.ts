import { UmbTreeItemCardExtensionElement } from './tree-item-card-extension.element.js';
import type { ManifestTreeItemCard } from './tree-item-card.extension.js';
import type { UmbTreeItemCardElement } from './types.js';
import type { UmbTreeItemModel } from '../types.js';
import { expect, waitUntil, aTimeout } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

let apiCreateCount = 0;

class UmbTestTreeItemCardApi extends UmbControllerBase implements UmbApi {
	#item?: UmbTreeItemModel;

	constructor(host: UmbControllerHost) {
		super(host);
		apiCreateCount++;
	}

	setTreeItem(item: UmbTreeItemModel | undefined): void {
		this.#item = item;
	}

	getTreeItem(): UmbTreeItemModel | undefined {
		return this.#item;
	}
}

@customElement('umb-test-tree-item-card')
class UmbTestTreeItemCardElement extends UmbControllerHostElementMixin(HTMLElement) {
	item: UmbTreeItemModel | undefined;
	api: UmbTestTreeItemCardApi | undefined;
}

const itemA1: UmbTreeItemModel = {
	unique: 'a-1',
	entityType: 'type-a',
	name: 'A1',
	hasChildren: false,
	isFolder: false,
	parent: { unique: null, entityType: 'type-a' },
};

const itemA2: UmbTreeItemModel = { ...itemA1, unique: 'a-2', name: 'A2' };

const itemB: UmbTreeItemModel = {
	unique: 'b-1',
	entityType: 'type-b',
	name: 'B1',
	hasChildren: false,
	isFolder: false,
	parent: { unique: null, entityType: 'type-b' },
};

const MANIFEST_ALIAS = 'Umb.Test.TreeItemCard';

describe('UmbTreeItemCardExtensionElement', () => {
	let element: UmbTreeItemCardExtensionElement;

	beforeEach(() => {
		apiCreateCount = 0;
		umbExtensionsRegistry.register({
			type: 'treeItemCard',
			alias: MANIFEST_ALIAS,
			name: 'Test Tree Item Card',
			elementName: 'umb-test-tree-item-card',
			api: UmbTestTreeItemCardApi,
			forEntityTypes: ['type-a', 'type-b'],
		} as unknown as ManifestTreeItemCard);

		element = new UmbTreeItemCardExtensionElement();
		document.body.appendChild(element);
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(MANIFEST_ALIAS);
	});

	// Reads the resolved card element off the protected `_component` state.
	function getComponent(): (UmbTreeItemCardElement & { api: UmbTestTreeItemCardApi }) | undefined {
		return (element as unknown as { _component?: UmbTreeItemCardElement & { api: UmbTestTreeItemCardApi } })._component;
	}

	it('creates the card component and api for the entity type', async () => {
		element.item = itemA1;

		await waitUntil(() => !!getComponent(), 'card component was not created');

		const component = getComponent()!;
		expect(apiCreateCount).to.equal(1);
		expect(component.item).to.equal(itemA1);
		expect(component.api.getTreeItem()).to.equal(itemA1);
	});

	it('reuses the component and updates the api when the item changes but the entity type is the same', async () => {
		element.item = itemA1;
		await waitUntil(() => !!getComponent(), 'card component was not created');

		const component = getComponent()!;
		const api = component.api;

		element.item = itemA2;

		// Same entity type: the controller must not be recreated.
		expect(getComponent()).to.equal(component);
		expect(apiCreateCount).to.equal(1);
		// Both the component and the api must hold the fresh item (no stale data).
		expect(component.item).to.equal(itemA2);
		expect(api).to.equal(component.api);
		expect(api.getTreeItem()).to.equal(itemA2);
	});

	it('recreates the controller when the entity type changes', async () => {
		element.item = itemA1;
		await waitUntil(() => !!getComponent(), 'card component was not created');

		const firstComponent = getComponent()!;

		element.item = itemB;
		await waitUntil(() => apiCreateCount === 2, 'controller was not recreated for the new entity type');

		const secondComponent = getComponent()!;
		expect(secondComponent).to.not.equal(firstComponent);
		expect(secondComponent.item).to.equal(itemB);
		expect(secondComponent.api.getTreeItem()).to.equal(itemB);
	});

	it('ignores an undefined item without recreating the controller', async () => {
		element.item = itemA1;
		await waitUntil(() => !!getComponent(), 'card component was not created');

		const component = getComponent()!;

		element.item = undefined;
		await aTimeout(50);

		expect(getComponent()).to.equal(component);
		expect(apiCreateCount).to.equal(1);
	});
});
