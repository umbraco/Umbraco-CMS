import type { UmbTreeContext } from '../tree.context.interface.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import { UmbTreeItemActiveManager } from '../active-manager/tree-active-manager.js';
import { UmbTreeExpansionManager } from '../expansion-manager/index.js';
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

	const registrySubscriberCount = () =>
		(umbExtensionsRegistry as unknown as { _extensions: { _subject: { observers: Array<unknown> } } })._extensions
			._subject.observers.length;

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

	async function renderAndRemove(index: number) {
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
		element.remove();
		await aTimeout(0);
	}

	it('holds no subscriptions on the extension registry once removed', async () => {
		// The first tree item sets up shared, session-wide observations.
		await renderAndRemove(0);
		const before = registrySubscriberCount();

		for (let i = 1; i <= 10; i++) {
			await renderAndRemove(i);
		}

		expect(registrySubscriberCount()).to.equal(before);
	});
});
