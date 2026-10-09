import { UmbTableTreeViewElement } from './table-tree-view.element.js';
import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreeContext } from '../../tree.context.interface.js';
import { UMB_TREE_CONTEXT } from '../../tree.context.token.js';
import { UmbTreeItemActiveManager } from '../../active-manager/tree-active-manager.js';
import { expect, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState, UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-table-tree-view-host')
class UmbTestTableTreeViewHostElement extends UmbElementMixin(HTMLElement) {}

@customElement('umb-test-custom-expand-table-tree-view')
class UmbTestCustomExpandTableTreeViewElement extends UmbTableTreeViewElement {
	protected override _showExpandSymbol(item: UmbTreeItemModel): boolean {
		return item.unique === 'custom';
	}

	protected override _getExpandPath(item: UmbTreeItemModel): string | undefined {
		return `/custom/${item.unique}`;
	}

	protected override _renderExpandSymbol(): TemplateResult | undefined {
		return html`<span class="custom-symbol"></span>`;
	}
}

function treeItem(unique: string, overrides: Partial<UmbTreeItemModel> = {}): UmbTreeItemModel {
	return {
		unique,
		entityType: 'test-entity',
		name: unique,
		hasChildren: false,
		isFolder: false,
		parent: { unique: null, entityType: 'test-root' },
		...overrides,
	};
}

describe('UmbTableTreeViewElement', () => {
	let host: UmbTestTableTreeViewHostElement;
	let items: UmbArrayState<UmbTreeItemModel>;
	let selectOnly: UmbBooleanState<boolean>;
	let opened: Array<UmbTreeItemModel>;

	beforeEach(() => {
		host = new UmbTestTableTreeViewHostElement();
		document.body.appendChild(host);

		items = new UmbArrayState<UmbTreeItemModel>([], (item) => item.unique);
		selectOnly = new UmbBooleanState(false);
		opened = [];

		const treeContext = {
			treeRoot: new UmbObjectState(undefined).asObservable(),
			selectOnly: selectOnly.asObservable(),
			selection: {
				selectable: new UmbBooleanState(false).asObservable(),
				selection: new UmbArrayState<string | null>([], (selected) => selected).asObservable(),
			},
			currentPageItems: items.asObservable(),
			hideTreeItemActions: new UmbBooleanState(true).asObservable(),
			getHostElement: () => host,
			pagination: {
				currentPage: new UmbNumberState(1).asObservable(),
				totalPages: new UmbNumberState(1).asObservable(),
			},
			activeManager: new UmbTreeItemActiveManager(host),
			getRepository: () => undefined,
			open: (item: UmbTreeItemModel) => opened.push(item),
		} as unknown as UmbTreeContext;

		new UmbContextProviderController(host, UMB_TREE_CONTEXT, treeContext);
	});

	afterEach(() => {
		host.remove();
	});

	async function render(view: UmbTableTreeViewElement, treeItems: Array<UmbTreeItemModel>) {
		items.setValue(treeItems);
		host.appendChild(view);
		await waitUntil(() => getTable(view)?.shadowRoot?.querySelector('uui-table-row'), 'the table did not render');
	}

	function getTable(view: UmbTableTreeViewElement): HTMLElement {
		return view.shadowRoot!.querySelector('umb-table') as HTMLElement;
	}

	function getRow(view: UmbTableTreeViewElement, unique: string): HTMLElement {
		return getTable(view).shadowRoot!.querySelector(`uui-table-row[data-sortable-id="${unique}"]`) as HTMLElement;
	}

	function getIndicator(view: UmbTableTreeViewElement, unique: string): HTMLElement | null {
		return getRow(view, unique).querySelector('[data-mark="table-row:open"]');
	}

	describe('children indicator', () => {
		it('is shown for an item with children and not for one without', async () => {
			const view = new UmbTableTreeViewElement();
			await render(view, [treeItem('parent', { hasChildren: true }), treeItem('leaf')]);

			expect(getIndicator(view, 'parent')).to.exist;
			expect(getIndicator(view, 'leaf')).to.not.exist;
		});

		it('shows the default expand symbol', async () => {
			const view = new UmbTableTreeViewElement();
			await render(view, [treeItem('parent', { hasChildren: true })]);

			expect(getIndicator(view, 'parent')!.querySelector('uui-symbol-expand')).to.exist;
		});

		it('asks the tree to open the item when activated', async () => {
			const view = new UmbTableTreeViewElement();
			const parent = treeItem('parent', { hasChildren: true });
			await render(view, [parent]);

			getIndicator(view, 'parent')!.dispatchEvent(new MouseEvent('click', { bubbles: true }));

			expect(opened).to.deep.equal([parent]);
		});

		it('keeps a row with an indicator interactive while the tree is select-only', async () => {
			selectOnly.setValue(true);
			const view = new UmbTableTreeViewElement();
			await render(view, [treeItem('parent', { hasChildren: true }), treeItem('leaf')]);

			expect(getRow(view, 'parent').hasAttribute('select-only')).to.be.false;
			expect(getRow(view, 'leaf').hasAttribute('select-only')).to.be.true;
		});
	});

	describe('overriding the expand symbol', () => {
		it('lets a view decide which items show the indicator, regardless of children', async () => {
			const view = new UmbTestCustomExpandTableTreeViewElement();
			await render(view, [treeItem('custom'), treeItem('other', { hasChildren: true })]);

			expect(getIndicator(view, 'custom')).to.exist;
			expect(getIndicator(view, 'other')).to.not.exist;
		});

		it('renders the symbol the view provides in place of the default', async () => {
			const view = new UmbTestCustomExpandTableTreeViewElement();
			await render(view, [treeItem('custom')]);

			const indicator = getIndicator(view, 'custom')!;
			expect(indicator.querySelector('.custom-symbol')).to.exist;
			expect(indicator.querySelector('uui-symbol-expand')).to.not.exist;
		});

		it('leads to the path the view provides', async () => {
			const view = new UmbTestCustomExpandTableTreeViewElement();
			await render(view, [treeItem('custom')]);

			expect(getIndicator(view, 'custom')!.getAttribute('href')).to.equal('/custom/custom');
		});

		it('keeps a row with an indicator interactive while the tree is select-only', async () => {
			selectOnly.setValue(true);
			const view = new UmbTestCustomExpandTableTreeViewElement();
			await render(view, [treeItem('custom'), treeItem('other', { hasChildren: true })]);

			expect(getRow(view, 'custom').hasAttribute('select-only')).to.be.false;
			expect(getRow(view, 'other').hasAttribute('select-only')).to.be.true;
		});
	});
});
