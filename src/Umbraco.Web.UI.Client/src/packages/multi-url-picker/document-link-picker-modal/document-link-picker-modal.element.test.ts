import { UmbDocumentLinkPickerModalElement } from './document-link-picker-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '@umbraco-cms/backoffice/document';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbInteractionMemoryManager,
	UmbInteractionMemoryScopeContext,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

// Stands in for whatever opened this modal — the link picker modal at runtime, which provides its own
// manager as the scope so this modal's memories nest inside its entry.
@customElement('test-document-link-picker-scope-host')
class UmbTestScopeHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	public readonly scope: UmbInteractionMemoryManager;
	constructor() {
		super();
		this.scope = new UmbInteractionMemoryManager(this);
		new UmbInteractionMemoryScopeContext(this, this.scope);
	}
}

const MODAL_MEMORY_UNIQUE = 'UmbDocumentLinkPickerModal';
const EXPANSION = [{ unique: 'doc-1', entityType: 'document' }];

const tick = () => new Promise((resolve) => setTimeout(resolve, 0));

const nestedMemory = (memories: Array<UmbInteractionMemoryModel>) =>
	({ unique: MODAL_MEMORY_UNIQUE, memories }) as UmbInteractionMemoryModel;

// The picker context configures search on construction, which resolves this provider from the
// registry. Registering a stub keeps the run free of "Failed to get manifest by alias" noise.
const searchProviderManifest = {
	type: 'searchProvider' as const,
	alias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
	name: 'Test Document Search Provider',
	api: class {
		destroy() {}
		async search() {
			return { data: { items: [], total: 0 } };
		}
	},
	meta: { label: 'Documents' },
};

describe('UmbDocumentLinkPickerModalElement', () => {
	let host: UmbTestScopeHostElement;
	let restoreErrorHandler: () => void;

	before(() => {
		umbExtensionsRegistry.register(searchProviderManifest as never);
	});

	after(() => {
		umbExtensionsRegistry.unregister(searchProviderManifest.alias);
	});

	const mount = async () => {
		const element = document.createElement('umb-document-link-picker-modal');
		host.appendChild(element);
		await element.updateComplete;
		await tick();
		const tree = element.shadowRoot!.querySelector<UmbTreeElement>('umb-tree')!;
		return { element, tree };
	};

	beforeEach(() => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();
		host = new UmbTestScopeHostElement();
		document.body.appendChild(host);
	});

	afterEach(() => {
		host.remove();
		restoreErrorHandler();
	});

	it('is defined with its own instance', async () => {
		const { element } = await mount();
		expect(element).to.be.instanceOf(UmbDocumentLinkPickerModalElement);
	});

	describe('tabs', () => {
		const getTabs = (element: UmbDocumentLinkPickerModalElement) => element.shadowRoot?.querySelector('uui-tab-group');

		const getPane = (element: UmbDocumentLinkPickerModalElement, id: 'browse' | 'search') =>
			element.shadowRoot?.querySelector<HTMLElement>(`#${id}`);

		const clickTab = async (element: UmbDocumentLinkPickerModalElement, tab: 'browse' | 'search') => {
			element.shadowRoot?.querySelector<HTMLElement>(`[data-mark="picker:tab:${tab}"]`)?.click();
			await element.updateComplete;
		};

		const mountWithTabs = async () => {
			const { element, tree } = await mount();
			await waitUntil(() => !!getTabs(element), 'tab group was never rendered');
			return { element, tree };
		};

		it('renders a browse and a search tab', async () => {
			const { element } = await mountWithTabs();
			expect(getTabs(element)?.querySelectorAll('uui-tab')).to.have.lengthOf(2);
		});

		it('starts on the browse tab', async () => {
			const { element } = await mountWithTabs();
			expect(getPane(element, 'browse')?.hidden).to.be.false;
			expect(getPane(element, 'search')?.hidden).to.be.true;
		});

		it('shows the search pane when the search tab is clicked', async () => {
			const { element } = await mountWithTabs();

			await clickTab(element, 'search');

			expect(getPane(element, 'browse')?.hidden).to.be.true;
			expect(getPane(element, 'search')?.hidden).to.be.false;
		});

		it('keeps the tree mounted while the search tab is active', async () => {
			const { element, tree } = await mountWithTabs();

			await clickTab(element, 'search');

			expect(getPane(element, 'browse')?.querySelector('umb-tree')).to.equal(tree);
		});

		it('renders one language selector, shared by both tabs', async () => {
			const { element } = await mountWithTabs();
			element.data = { allowCultureSpecificLinks: true };

			// The selector only appears once more than one language has loaded.
			await waitUntil(
				() => !!element.shadowRoot?.querySelector('uui-combobox'),
				'language selector was never rendered',
			);

			await clickTab(element, 'search');

			// The culture applies to the picked document either way, so the selector sits above both panes.
			expect(element.shadowRoot?.querySelectorAll('uui-combobox')).to.have.lengthOf(1);
			expect(getPane(element, 'browse')?.querySelector('uui-combobox')).to.not.exist;
			expect(getPane(element, 'search')?.querySelector('uui-combobox')).to.not.exist;
		});
	});

	describe('interaction memory', () => {
		it('publishes the tree expansion to the scope, nested under its own key', async () => {
			const { tree } = await mount();

			// The tree reports its expansion through `getExpansion()` when it dispatches the change.
			tree.getExpansion = () => EXPANSION;
			tree.dispatchEvent(new CustomEvent('expansion-change', { bubbles: true }));
			await tick();

			expect(host.scope.getMemory(MODAL_MEMORY_UNIQUE)).to.deep.equal(
				nestedMemory([{ unique: 'UmbTreeItemPickerExpansion', value: { expansion: EXPANSION } }]),
			);
		});

		it('collects the tree memories under its own key, nested under its own key', async () => {
			const { tree } = await mount();

			const treeMemories = [{ unique: 'UmbTreeCurrentView', value: { alias: 'Umb.TreeView.Table' } }];
			Object.defineProperty(tree, 'interactionMemories', { value: treeMemories, configurable: true });
			tree.dispatchEvent(new CustomEvent('interaction-memories-change', { bubbles: true }));
			await tick();

			expect(host.scope.getMemory(MODAL_MEMORY_UNIQUE)).to.deep.equal(
				nestedMemory([{ unique: 'UmbTreeItemPickerTree', memories: treeMemories }]),
			);
		});

		it('removes its entry from the scope once it holds nothing', async () => {
			const { tree } = await mount();

			Object.defineProperty(tree, 'interactionMemories', { value: [{ unique: 'a' }], configurable: true });
			tree.dispatchEvent(new CustomEvent('interaction-memories-change', { bubbles: true }));
			await tick();
			expect(host.scope.getMemory(MODAL_MEMORY_UNIQUE)).to.not.equal(undefined);

			Object.defineProperty(tree, 'interactionMemories', { value: [], configurable: true });
			tree.dispatchEvent(new CustomEvent('interaction-memories-change', { bubbles: true }));
			await tick();

			expect(host.scope.getMemory(MODAL_MEMORY_UNIQUE)).to.equal(undefined);
		});

		it('restores the tree expansion from the scope and hands it to the tree', async () => {
			host.scope.setMemory(nestedMemory([{ unique: 'UmbTreeItemPickerExpansion', value: { expansion: EXPANSION } }]));

			const { tree } = await mount();

			expect(tree.props?.expansion).to.deep.equal(EXPANSION);
		});

		it('restores the tree memories from the scope and hands them to the tree', async () => {
			const treeMemories = [{ unique: 'UmbTreeCurrentView', value: { alias: 'Umb.TreeView.Table' } }];
			host.scope.setMemory(nestedMemory([{ unique: 'UmbTreeItemPickerTree', memories: treeMemories }]));

			const { tree } = await mount();

			expect(tree.props?.interactionMemories).to.deep.equal(treeMemories);
		});
	});
});
