import type { UmbTreeContext } from '../tree.context.interface.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import { UmbTreeItemActiveManager } from '../active-manager/tree-active-manager.js';
import { UmbTreeExpansionManager } from '../expansion-manager/index.js';
import type { ManifestTreeItem } from '../extensions/types.js';
import { UmbTreeItemElement } from './tree-item.element.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-tree-item-host')
class UmbTestTreeItemHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbTreeItemElement', () => {
	let host: UmbTestTreeItemHostElement;

	beforeEach(() => {
		host = new UmbTestTreeItemHostElement();
		document.body.appendChild(host);

		const treeContext = {
			treeRoot: new UmbObjectState(undefined).asObservable(),
			selectOnly: new UmbBooleanState(false).asObservable(),
			selection: {
				selectable: new UmbBooleanState(false).asObservable(),
				selection: new UmbArrayState<string | null>([], (selected) => selected).asObservable(),
			},
			hideTreeItemActions: new UmbBooleanState(true).asObservable(),
			getHostElement: () => host,
			pagination: {
				currentPage: new UmbNumberState(1).asObservable(),
				totalPages: new UmbNumberState(1).asObservable(),
			},
			activeManager: new UmbTreeItemActiveManager(host),
			expansion: new UmbTreeExpansionManager(host),
			getRepository: () => undefined,
		} as unknown as UmbTreeContext;

		new UmbContextProviderController(host, UMB_TREE_CONTEXT, treeContext);
	});

	afterEach(() => {
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(document.createElement('umb-tree-item')).to.be.instanceOf(UmbTreeItemElement);
	});

	// A tree item manifest for another entity type: the test items never match it, so they keep rendering the default
	// tree item, but every tree item that observes the registry reads its `forEntityTypes` when the registry changes.
	let forEntityTypesReads = 0;
	const observedManifest = {
		type: 'treeItem',
		alias: 'Umb.Test.TreeItem.OtherEntityType',
		name: 'Test Tree Item For Another Entity Type',
		get forEntityTypes() {
			forEntityTypesReads++;
			return ['test-other-entity-type'];
		},
	};
	const registryChangeAlias = 'Umb.Test.Section.RegistryChange';

	beforeEach(() => {
		umbExtensionsRegistry.register(observedManifest as unknown as ManifestTreeItem);
	});

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany([observedManifest.alias, registryChangeAlias]);
	});

	async function render(index: number) {
		const element = document.createElement('umb-tree-item');
		element.entityType = 'test-entity-type';
		element.props = {
			item: {
				unique: `item-${index}`,
				entityType: 'test-entity-type',
				name: `Item ${index}`,
				hasChildren: false,
				isFolder: false,
				parent: { unique: null, entityType: 'test-root' },
			},
		};
		host.appendChild(element);
		// TODO: remove this wait once #24445 is merged; removing an item mid-load leaves subscriptions behind until then.
		await aTimeout(50);
		return element;
	}

	function changeRegistry() {
		forEntityTypesReads = 0;
		umbExtensionsRegistry.register({ type: 'section', alias: registryChangeAlias, name: 'Registry Change' } as never);
	}

	it('evaluates tree item manifests on registry changes while connected', async () => {
		const element = await render(0);

		changeRegistry();
		element.remove();

		expect(forEntityTypesReads).to.be.greaterThan(0);
	});

	it('stops evaluating tree item manifests once removed', async () => {
		for (let i = 0; i < 10; i++) {
			(await render(i)).remove();
		}
		await aTimeout(0);

		changeRegistry();

		expect(forEntityTypesReads).to.equal(0);
	});
});
