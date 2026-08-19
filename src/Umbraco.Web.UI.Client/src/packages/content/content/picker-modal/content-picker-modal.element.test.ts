import { UmbContentPickerModalElement } from './content-picker-modal.element.js';
import type { UmbContentPickerModalData } from './types.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { umbMockManager, useMockSet } from '@umbraco-cms/internal/mock-manager';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbTreeItemOpenEvent } from '@umbraco-cms/backoffice/tree';
import type { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';

const TREE_ALIAS = 'Umb.Test.ContentPicker.Tree';
const TREE_REPOSITORY_ALIAS = 'Umb.Test.ContentPicker.TreeRepository';
const COLLECTION_ALIAS = 'Umb.Test.ContentPicker.Collection';
const OPENER_UNIQUE = 'the-opening-document';

const ROOT = { unique: null, entityType: 'test-root', name: 'Root' };

/**
 * A node whose content type configures a collection, and one that does not. `collectionDataTypeUnique` is filled from
 * the mock data set so the configuration actually resolves.
 */
let collectionDataTypeUnique: string;

function treeItem(unique: string, name: string, hasCollection: boolean) {
	return {
		unique,
		entityType: 'test-item',
		name,
		hasChildren: true,
		isFolder: false,
		contentType: {
			unique: `content-type-of-${unique}`,
			icon: 'icon-document',
			collection: hasCollection ? { unique: collectionDataTypeUnique } : null,
		},
	};
}

/** root → withCollection → deeper */
const ITEMS: Record<string, Array<ReturnType<typeof treeItem>>> = {};

class UmbTestTreeRepository {
	async requestTreeRoot() {
		return { data: ROOT };
	}

	async requestTreeItemAncestors({ treeItem: target }: { treeItem: { unique: string } }) {
		return { data: ITEMS[target.unique] ?? [] };
	}

	destroy() {}
}

/** Stands in for the element that opened the modal, so the modal's own entity context has something to shadow. */
@customElement('test-content-picker-opener')
class UmbTestOpenerElement extends UmbElementMixin(HTMLElement) {
	entityContext = new UmbEntityContext(this);

	constructor() {
		super();
		this.entityContext.setEntityType('test-item');
		this.entityContext.setUnique(OPENER_UNIQUE);
	}
}

describe('UmbContentPickerModalElement', () => {
	let element: UmbContentPickerModalElement<UmbTreeItemModelBase>;
	let opener: UmbTestOpenerElement;
	let restoreErrorHandler: () => void;

	before(async () => {
		await useMockSet('kitchenSink');

		const dataType = umbMockManager.getDataSet().dataType!.find((candidate) => !candidate.isFolder)!;
		collectionDataTypeUnique = dataType.id!;

		ITEMS['with-collection'] = [treeItem('with-collection', 'With Collection', true)];
		ITEMS['without-collection'] = [treeItem('without-collection', 'Without Collection', false)];
		ITEMS['deeper'] = [treeItem('with-collection', 'With Collection', true), treeItem('deeper', 'Deeper', false)];
	});

	beforeEach(async () => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();

		umbExtensionsRegistry.registerMany([
			{
				type: 'repository',
				alias: TREE_REPOSITORY_ALIAS,
				name: 'Test Tree Repository',
				api: UmbTestTreeRepository,
			},
			{
				type: 'tree',
				alias: TREE_ALIAS,
				name: 'Test Tree',
				meta: { repositoryAlias: TREE_REPOSITORY_ALIAS },
			},
		] as Array<never>);

		opener = await fixture(html`<test-content-picker-opener></test-content-picker-opener>`);
		element = new UmbContentPickerModalElement();
		element.data = {
			treeAlias: TREE_ALIAS,
			collection: { alias: COLLECTION_ALIAS },
			multiple: true,
		} as UmbContentPickerModalData<UmbTreeItemModelBase>;
		opener.appendChild(element);
		await element.updateComplete;
		await waitUntil(() => trail().length === 1, 'the root breadcrumb was never loaded');
	});

	afterEach(() => {
		element.remove();
		opener.remove();
		umbExtensionsRegistry.unregister(TREE_ALIAS);
		umbExtensionsRegistry.unregister(TREE_REPOSITORY_ALIAS);
		restoreErrorHandler();
	});

	const getBreadcrumb = () => element.shadowRoot?.querySelector('umb-tree-item-picker-breadcrumb');
	const getTree = () => element.shadowRoot?.querySelector('umb-tree');
	const getCollection = () => element.shadowRoot?.querySelector('umb-collection');
	const trail = () =>
		[...(getBreadcrumb()?.shadowRoot?.querySelectorAll('uui-breadcrumb-item') ?? [])].map((crumb) =>
			crumb.textContent!.trim(),
		);

	async function browseTo(unique: string) {
		element.dispatchEvent(new UmbTreeItemOpenEvent({ unique, entityType: 'test-item' }));
		await waitUntil(() => trail().length > 1, `never browsed to ${unique}`);
		await element.updateComplete;
	}

	async function clickBreadcrumb(index: number) {
		const items = getBreadcrumb()!.shadowRoot!.querySelectorAll<HTMLElement>('uui-breadcrumb-item');
		items[index].click();
		// Browsing the trail resolves asynchronously, so wait for it to land before asserting.
		await waitUntil(() => trail().length === index + 1, `never browsed back to step ${index}`);
		await element.updateComplete;
	}

	/**
	 * Whether the modal resolved the browsed node as having a collection. The `<umb-collection>` element itself only
	 * mounts once the configuring data type has loaded, which needs the data type store the app bootstraps, so these
	 * tests assert the decision and the absence of the tree instead.
	 */
	function hasCollection() {
		return (element as unknown as { _hasCollection: boolean })._hasCollection;
	}

	/**
	 * The modal's `value` is written through the modal context, which a standalone element has none of, so the
	 * selection is read from the picker context the modal owns.
	 */
	function selection() {
		return (
			element as unknown as { _pickerContext: { selection: { getSelection(): Array<string | null> } } }
		)._pickerContext.selection.getSelection();
	}

	async function readEntityContext() {
		const context = await element.getContext(UMB_ENTITY_CONTEXT);
		return { unique: context?.getUnique(), entityType: context?.getEntityType() };
	}

	describe('choosing a renderer per level', () => {
		it('renders the tree at the root', () => {
			expect(getTree()).to.exist;
			expect(getCollection()).to.not.exist;
		});

		it('renders the tree for a node whose content type has no collection', async () => {
			await browseTo('without-collection');

			expect(getTree()).to.exist;
			expect(getCollection()).to.not.exist;
		});

		it('takes the collection over the tree for a node whose content type has one', async () => {
			await browseTo('with-collection');

			expect(hasCollection()).to.be.true;
			// The tree must not be shown in the collection's place, not even while the data type is still loading.
			expect(getTree()).to.not.exist;
		});
	});

	// Opening straight into a collection must not mount the tree first: it would be torn down mid-initialisation, which
	// throws from the tree's own children and expansion managers.
	describe('opening into a remembered collection', () => {
		it('does not mount the tree before the location is known', async () => {
			const remembering = new UmbContentPickerModalElement<UmbTreeItemModelBase>();
			(
				remembering as unknown as { _pickerContext: { interactionMemory: UmbInteractionMemoryManager } }
			)._pickerContext.interactionMemory.setMemory({
				unique: 'UmbTreeItemPickerLocation',
				value: { entity: { unique: 'with-collection', entityType: 'test-item' } },
			});
			remembering.data = {
				treeAlias: TREE_ALIAS,
				collection: { alias: COLLECTION_ALIAS },
			} as UmbContentPickerModalData<UmbTreeItemModelBase>;

			opener.appendChild(remembering);
			await remembering.updateComplete;

			expect(remembering.shadowRoot?.querySelector('umb-tree')).to.not.exist;

			remembering.remove();
		});
	});

	describe('selection', () => {
		it('takes a selection made in the tree', async () => {
			getTree()!.dispatchEvent(new UmbSelectedEvent('picked-in-tree'));
			await element.updateComplete;

			expect(selection()).to.include('picked-in-tree');
		});

		// `UmbDefaultTreeContext.setStartNode` clears and reloads unconditionally, and Lit calls it whenever the property
		// changes identity. So a start node rebuilt per render would reload the whole tree on every selection.
		it('does not give the tree a new start node when the selection changes', async () => {
			await browseTo('without-collection');
			const before = getTree()!.props!.startNode;

			getTree()!.dispatchEvent(new UmbSelectedEvent('picked-in-tree'));
			await element.updateComplete;

			expect(getTree()!.props!.startNode).to.equal(before);
		});

		it('keeps the selection when the renderer changes', async () => {
			getTree()!.dispatchEvent(new UmbSelectedEvent('picked-in-tree'));
			await element.updateComplete;

			await browseTo('with-collection');

			expect(hasCollection()).to.be.true;
			expect(selection()).to.include('picked-in-tree');
		});
	});

	// A modal re-dispatches unanswered context requests onto the element that opened it, so without its own entity
	// context the collection would bind to the document being edited rather than the node being browsed.
	describe('the entity context', () => {
		it('reports the browsed node rather than the opener', async () => {
			await browseTo('with-collection');

			expect(await readEntityContext()).to.eql({ unique: 'with-collection', entityType: 'test-item' });
		});

		it('reports the root rather than the opener before anything is browsed', async () => {
			expect((await readEntityContext()).unique).to.not.equal(OPENER_UNIQUE);
		});
	});

	// Everything that browses must go through one resolution point, so the renderer, the entity context and the
	// collection configuration cannot describe different nodes.
	describe('browsing back through the breadcrumb', () => {
		it('returns to the tree when the root is clicked', async () => {
			await browseTo('with-collection');
			expect(hasCollection()).to.be.true;

			await clickBreadcrumb(0);

			expect(trail()).to.eql(['Root']);
			expect(hasCollection()).to.be.false;
			expect(getTree()).to.exist;
		});

		it('resets the entity context when the root is clicked', async () => {
			await browseTo('with-collection');
			await clickBreadcrumb(0);

			expect((await readEntityContext()).unique).to.be.null;
		});

		it('re-resolves the collection when browsing up into a collection node', async () => {
			await browseTo('deeper');
			expect(trail()).to.eql(['Root', 'With Collection', 'Deeper']);
			expect(hasCollection()).to.be.false;

			await clickBreadcrumb(1);

			expect(hasCollection()).to.be.true;
			expect(getTree()).to.not.exist;
			expect(await readEntityContext()).to.eql({ unique: 'with-collection', entityType: 'test-item' });
		});
	});
});
