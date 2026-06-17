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
		if (value?.view) {
			this.observe(value.view.currentView, async (manifest) => {
				const element = manifest ? await createExtensionElement(manifest) : null;
				if (element && 'manifest' in element) {
					(element as HTMLElement & { manifest: unknown }).manifest = manifest;
				}
				this._viewElement = element;
			});
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

	/**
	 * When true the view-switcher toolbar is hidden.
	 * Defaults to true for backwards compatibility — existing trees stay toolbar-less
	 * until a consumer explicitly opts in with hide-toolbar="false".
	 * Note: hideTreeRoot and hideTreeItemActions default to false; this is the intentional exception.
	 */
	@property({ type: Boolean, attribute: 'hide-toolbar' })
	hideToolbar: boolean = true;

	/**
	 * When true the tree actions are hidden.
	 * Defaults to true — tree actions are not shown unless explicitly opted in with hide-tree-actions="false".
	 */
	@property({ type: Boolean, attribute: 'hide-tree-actions' })
	hideTreeActions: boolean = true;

	@property({ attribute: false })
	interactionMemories?: Array<UmbInteractionMemoryModel>;

	#lastDispatchedMemories: Array<UmbInteractionMemoryModel> = [];

	@state()
	private _viewElement?: HTMLElement | null;

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
			${!this.hideToolbar
				? html`<umb-tree-toolbar .hideTreeActions=${this.hideTreeActions}></umb-tree-toolbar>`
				: nothing}
			${this._viewElement ?? nothing}
		`;
	}

	static override styles = css`
		:host {
			display: contents;
		}
	`;
}

export default UmbDefaultTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree': UmbDefaultTreeElement;
	}
}
