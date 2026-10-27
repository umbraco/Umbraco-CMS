import { UmbDocumentPickerModalElement } from './document-picker-modal.element.js';
import type { UmbDocumentPickerModalData } from './types.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { umbMockManager, useMockSet } from '@umbraco-cms/internal/mock-manager';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbTreeItemOpenEvent } from '@umbraco-cms/backoffice/tree';
import type { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

const TREE_ALIAS = 'Umb.Test.DocumentPicker.Tree';
const TREE_REPOSITORY_ALIAS = 'Umb.Test.DocumentPicker.TreeRepository';
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

/**
 * Stands in for `UMB_CURRENT_USER_CONTEXT`, exposing only the `allowedSections` part the modal reads. Defaults to
 * Content-section access so tests unrelated to the permission guard see the same collection/tree behaviour as before
 * it existed.
 */
class UmbTestCurrentUserContext {
	#host: UmbElement;
	#allowedSections = new UmbArrayState<string>([UMB_CONTENT_SECTION_ALIAS], (alias) => alias);
	readonly allowedSections = this.#allowedSections.asObservable();

	constructor(host: UmbElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	setAllowedSections(sections: Array<string>) {
		this.#allowedSections.setValue(sections);
	}
}

/** Stands in for the element that opened the modal, so the modal's own entity context has something to shadow. */
@customElement('test-document-picker-opener')
class UmbTestOpenerElement extends UmbElementMixin(HTMLElement) {
	entityContext = new UmbEntityContext(this);
	currentUserContext = new UmbTestCurrentUserContext(this);

	constructor() {
		super();
		this.entityContext.setEntityType('test-item');
		this.entityContext.setUnique(OPENER_UNIQUE);
		this.provideContext(UMB_CURRENT_USER_CONTEXT, this.currentUserContext as never);
	}
}

describe('UmbDocumentPickerModalElement', () => {
	let element: UmbDocumentPickerModalElement;
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

		opener = await fixture(html`<test-document-picker-opener></test-document-picker-opener>`);
		element = new UmbDocumentPickerModalElement();
		element.data = {
			treeAlias: TREE_ALIAS,
			multiple: true,
		} as UmbDocumentPickerModalData;
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
	const getNotFound = () => element.shadowRoot?.querySelector('#not-found');
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

	function hasContentSectionAccess() {
		return (element as unknown as { _hasContentSectionAccess: boolean })._hasContentSectionAccess;
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

	// The Collection endpoint authorizes against the Content section alone, while the Tree endpoint accepts any
	// backoffice section, so a user without Content-section access must be routed to the tree even for a
	// collection-configured node, rather than hitting a bare 403 from the collection.
	describe('guarding the collection by Content-section access', () => {
		it('falls back to the tree for a collection-configured node when the user lacks Content-section access', async () => {
			opener.currentUserContext.setAllowedSections(['Umb.Section.Media']);
			await browseTo('with-collection');

			expect(hasCollection()).to.be.true;
			expect(getTree()).to.exist;
			expect(getCollection()).to.not.exist;
		});

		it('still renders the collection for a collection-configured node when the user has Content-section access', async () => {
			await browseTo('with-collection');

			expect(hasCollection()).to.be.true;
			expect(getTree()).to.not.exist;
		});

		it('reacts to the current user context changing allowed sections', async () => {
			await browseTo('with-collection');
			expect(hasContentSectionAccess()).to.be.true;
			expect(getTree()).to.not.exist;

			opener.currentUserContext.setAllowedSections(['Umb.Section.Media']);
			await waitUntil(() => !!getTree(), 'never fell back to the tree once access was lost');
			expect(hasContentSectionAccess()).to.be.false;

			opener.currentUserContext.setAllowedSections([UMB_CONTENT_SECTION_ALIAS]);
			await waitUntil(() => hasContentSectionAccess(), 'never regained Content-section access');
			await element.updateComplete;
			expect(getTree()).to.not.exist;
		});
	});

	// Opening straight into a collection must not mount the tree first: it would be torn down mid-initialisation, which
	// throws from the tree's own children and expansion managers.
	describe('opening into a remembered collection', () => {
		it('does not mount the tree before the location is known', async () => {
			const remembering = new UmbDocumentPickerModalElement();
			(
				remembering as unknown as { _pickerContext: { interactionMemory: UmbInteractionMemoryManager } }
			)._pickerContext.interactionMemory.setMemory({
				unique: 'UmbTreeItemPickerLocation',
				value: { entity: { unique: 'with-collection', entityType: 'test-item' } },
			});
			remembering.data = {
				treeAlias: TREE_ALIAS,
			} as UmbDocumentPickerModalData;

			opener.appendChild(remembering);
			await remembering.updateComplete;

			expect(remembering.shadowRoot?.querySelector('umb-tree')).to.not.exist;

			remembering.remove();
		});
	});

	// A level the tree cannot describe has no renderer, so the modal has to say so rather than fall back to a level the
	// user did not browse to.
	describe('browsing to a node the tree does not have', () => {
		beforeEach(async () => {
			await browseTo('without-collection');

			element.dispatchEvent(new UmbTreeItemOpenEvent({ unique: 'gone', entityType: 'test-item' }));
			await waitUntil(() => !!getNotFound(), 'the not-found state was never rendered');
			await element.updateComplete;
		});

		it('renders neither the tree nor the collection', () => {
			expect(getTree()).to.not.exist;
			expect(getCollection()).to.not.exist;
		});

		// The trail is the way back out of a dead end, so it has to survive one.
		it('keeps the trail the user came from', () => {
			expect(trail()).to.eql(['Root', 'Without Collection']);
		});

		it('recovers when the user browses back through the breadcrumb', async () => {
			await clickBreadcrumb(0);

			expect(getNotFound()).to.not.exist;
			expect(getTree()).to.exist;
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
