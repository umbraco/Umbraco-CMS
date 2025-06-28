import { UMB_SECTION_SIDEBAR_MENU_CONTEXT } from '../../menu/section-sidebar-menu/context/section-sidebar-menu.context.token.js';
import type { UmbTreeElement } from '../tree.element.js';
import type { ManifestMenuItemTreeKind } from './types.js';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbMenuItemElement } from '@umbraco-cms/backoffice/menu';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbExpansionChangeEvent } from '@umbraco-cms/backoffice/utils';

@customElement('umb-menu-item-tree-default')
export class UmbMenuItemTreeDefaultElement extends UmbLitElement implements UmbMenuItemElement {
	@property({ type: Object })
	manifest?: ManifestMenuItemTreeKind;

	@state()
	_expansion: Array<UmbEntityModel> = [];

	#context?: typeof UMB_SECTION_SIDEBAR_MENU_CONTEXT.TYPE;

	constructor() {
		super();
		// TODO: make another context abstraction UMB_MENU_CONTEXT
		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_CONTEXT, (context) => {
			this.#context = context;
			this.#observeExpansion();
		});
	}

	#observeExpansion() {
		this.observe(this.#context?.expansion.expansion, (items) => {
			this._expansion = items || [];
		});
	}

	#onExpansionChange(event: UmbExpansionChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbTreeElement;
		const expansion = target.getExpansion();
		this.#context?.expansion.updateExpansion(expansion);
	}

	override render() {
		return this.manifest
			? html`
					<umb-tree
						alias=${this.manifest?.meta.treeAlias}
						.props=${{
							hideTreeRoot: this.manifest?.meta.hideTreeRoot === true,
							selectionConfiguration: {
								selectable: false,
								multiple: false,
							},
							expansion: this._expansion,
						}}
						@expansion-change=${this.#onExpansionChange}></umb-tree>
				`
			: nothing;
	}
}

export default UmbMenuItemTreeDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-tree-default': UmbMenuItemTreeDefaultElement;
	}
}
