import type {
	UmbTreeItemModel,
	UmbTreeItemModelBase,
	UmbTreeRootModel,
	UmbTreeSelectionConfiguration,
	UmbTreeStartNode,
} from '../types.js';
import type { UmbTreeExpansionModel } from '../expansion-manager/types.js';
import type { UmbDefaultTreeContext } from './default-tree.context.js';

import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbInteractionMemoriesChangeEvent } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';

import '../components/tree-toolbar.element.js';

@customElement('umb-default-tree')
export class UmbDefaultTreeElement extends UmbLitElement {
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	private _api: UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel> | undefined;
	@property({ type: Object, attribute: false })
	public get api(): UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel> | undefined {
		return this._api;
	}
	public set api(value: UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel> | undefined) {
		this._api = value;
		if (value) {
			// Derive emptiness from the loaded children, not `hasChildren`: the latter is also written by the
			// concurrent tree-root load (with the root's own child count), which can clobber the start node's value
			// and intermittently hide the empty state.
			this.observe(value.rootItems, (items) => (this._hasItems = (items?.length ?? 0) > 0), 'umbTreeRootItemsObserver');
			// Track loading so the empty state isn't shown before the children have loaded, or while reloading.
			this.observe(
				value.isLoading,
				(isLoading) => {
					this._isLoading = isLoading ?? false;
					if (isLoading) {
						this.#hasBeenLoading = true;
					} else if (this.#hasBeenLoading) {
						this._initialLoadDone = true;
					}
				},
				'umbTreeIsLoadingObserver',
			);
		}
		if (value?.view) {
			this.observe(
				value.view.currentView,
				async (manifest) => {
					const element = manifest ? await createExtensionElement(manifest) : null;
					if (element && 'manifest' in element) {
						(element as HTMLElement & { manifest: unknown }).manifest = manifest;
					}
					this._viewElement = element;
				},
				'umbTreeCurrentViewObserver',
			);
		}
		if (value?.interactionMemory) {
			// Snapshot before forwarding so the first observer emission is not treated as a change.
			this.#lastDispatchedMemories = this.interactionMemories ?? [];
			this.interactionMemories?.forEach((m) => value.interactionMemory!.setMemory(m));
			this.observe(
				value.interactionMemory.memories,
				(memories) => {
					if (!jsonStringComparison(memories, this.#lastDispatchedMemories)) {
						this.#lastDispatchedMemories = memories;
						this.dispatchEvent(new UmbInteractionMemoriesChangeEvent());
					}
				},
				'umbTreeInteractionMemoryObserver',
			);
		}
	}

	@property({ type: Object, attribute: false })
	selectionConfiguration: UmbTreeSelectionConfiguration = this._selectionConfiguration;

	@property({ type: Boolean, attribute: false })
	hideTreeItemActions: boolean = false;

	@property({ type: Boolean, attribute: false })
	hideTreeRoot: boolean = false;

	@property({ type: Boolean, attribute: false })
	expandTreeRoot: boolean = false;

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: Boolean, attribute: false })
	foldersOnly?: boolean = false;

	@property({ type: Boolean, attribute: false })
	isMenu?: boolean = false;

	@property({ attribute: false })
	selectableFilter: (item: UmbTreeItemModelBase) => boolean = () => true;

	@property({ attribute: false })
	filter: (item: UmbTreeItemModelBase) => boolean = () => true;

	@property({ attribute: false })
	expansion: UmbTreeExpansionModel = [];

	@property({ type: Boolean, attribute: 'show-toolbar' })
	showToolbar: boolean = false;

	@property({ type: Boolean, attribute: 'show-tree-actions' })
	showTreeActions: boolean = false;

	@property({ attribute: false })
	interactionMemories?: Array<UmbInteractionMemoryModel>;

	#lastDispatchedMemories: Array<UmbInteractionMemoryModel> = [];

	@state()
	private _viewElement?: HTMLElement | null;

	@state()
	private _hasItems = false;

	@state()
	private _isLoading = false;

	@state()
	private _initialLoadDone = false;

	#hasBeenLoading = false;

	protected override async updated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.updated(_changedProperties);
		if (this._api === undefined) return;

		if (_changedProperties.has('api')) {
			this._api.loadTree();
		}

		if (_changedProperties.has('selectionConfiguration')) {
			this._selectionConfiguration = this.selectionConfiguration;
			this._api!.selection.setMultiple(this._selectionConfiguration.multiple ?? false);
			this._api!.selection.setSelectable(this._selectionConfiguration.selectable ?? true);
			this._api!.selection.setSelection(this._selectionConfiguration.selection ?? []);
			this._api!.setSelectOnly(this._selectionConfiguration.selectOnly);
		}

		if (_changedProperties.has('startNode')) {
			this._api!.setStartNode(this.startNode);
		}

		if (_changedProperties.has('hideTreeRoot')) {
			this._api!.setHideTreeRoot(this.hideTreeRoot);
		}

		if (_changedProperties.has('expandTreeRoot')) {
			this._api!.setExpandTreeRoot(this.expandTreeRoot);
		}

		if (_changedProperties.has('foldersOnly')) {
			this._api!.setFoldersOnly(this.foldersOnly ?? false);
		}

		if (_changedProperties.has('selectableFilter')) {
			this._api!.selectableFilter = this.selectableFilter;
		}

		if (_changedProperties.has('filter')) {
			this._api!.filter = this.filter;
		}

		if (_changedProperties.has('expansion')) {
			this._api!.setExpansion(this.expansion);
		}

		if (_changedProperties.has('hideTreeItemActions')) {
			this._api!.setHideTreeItemActions?.(this.hideTreeItemActions);
		}

		if (_changedProperties.has('isMenu')) {
			this._api!.setIsMenu?.(this.isMenu ?? false);
		}

		if (_changedProperties.has('interactionMemories') && this._api?.interactionMemory) {
			this.#lastDispatchedMemories = this.interactionMemories ?? [];
			this.interactionMemories?.forEach((m) => this._api!.interactionMemory!.setMemory(m));
		}
	}

	getSelection() {
		return this._api?.selection.getSelection();
	}

	getExpansion() {
		return this._api?.expansion.getExpansion();
	}

	override render() {
		return html`
			${this.showToolbar
				? html`<umb-tree-toolbar .showTreeActions=${this.showTreeActions}></umb-tree-toolbar>`
				: nothing}
			${this._viewElement ?? nothing} ${this.#renderEmptyState()}
		`;
	}

	#renderEmptyState() {
		// The empty state belongs to the children list, not a single view, so it is presented once here — mirroring
		// the collection pattern. It is only relevant when the children are shown on their own (root hidden or drilled
		// into a start node) and never in the sidebar menu.
		if (this.isMenu || !(this.hideTreeRoot || this.startNode)) return nothing;
		if (!this._initialLoadDone || this._isLoading || this._hasItems) return nothing;
		return html`<div id="empty-state" class="uui-text"><h4>${this.localize.term('tree_noItems')}</h4></div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			#empty-state {
				text-align: center;
				padding: var(--uui-size-layout-1);
				opacity: 0;
				animation: fadeIn 100ms 100ms forwards;
			}

			@keyframes fadeIn {
				100% {
					opacity: 1;
				}
			}
		`,
	];
}

export default UmbDefaultTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree': UmbDefaultTreeElement;
	}
}
