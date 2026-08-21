import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '@umbraco-cms/backoffice/schema-lockdown';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

// The base collection state we need to reproduce its layout while injecting the notice.
// These mirror private members of UmbCollectionDefaultElement, hence the underscore-prefixed names.
/* eslint-disable @typescript-eslint/naming-convention */
type UmbCollectionRenderState = {
	_routes?: Array<UmbRoute>;
	_hasItems: boolean;
	_initialLoadDone: boolean;
	_emptyLabel?: string;
};
/* eslint-enable @typescript-eslint/naming-convention */

@customElement('umb-dictionary-collection')
export class UmbDictionaryCollectionElement extends UmbCollectionDefaultElement {
	// Restricted until the schema lockdown matrix confirms the operation is allowed (safe default).
	@state()
	private _isRestricted = true;

	constructor() {
		super();
		this.consumeContext(UMB_SCHEMA_LOCKDOWN_CONTEXT, (context) => {
			this.observe(context?.state, () => {
				this._isRestricted = context?.isAllowed(UMB_DICTIONARY_ENTITY_TYPE, 'create') !== true;
			});
		});
	}

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
		`;
	}

	// Mirrors UmbCollectionDefaultElement.render(), but places the schema lockdown notice inside the
	// body-layout right before the router slot so it sits full-width at the top of the content area.
	override render() {
		const base = this as unknown as UmbCollectionRenderState;
		if (!base._routes) return nothing;
		return html`
			<umb-body-layout header-transparent class=${base._hasItems ? 'has-items' : ''}>
				${this.#renderSchemaLockdownNotice()}
				<umb-router-slot id="router" .routes=${base._routes}></umb-router-slot>
				${this.renderToolbar()} ${base._hasItems ? this.#renderContent() : this.#renderEmptyState(base)}
			</umb-body-layout>
		`;
	}

	#renderContent() {
		return html`${this.renderPagination()} ${this.renderSelectionActions()}`;
	}

	#renderEmptyState(base: UmbCollectionRenderState) {
		if (!base._initialLoadDone) return nothing;
		return html`
			<div id="empty-state" class="uui-text">
				<h4>${this.localize.string(base._emptyLabel)}</h4>
			</div>
		`;
	}

	#renderSchemaLockdownNotice() {
		if (!this._isRestricted) return nothing;
		return html`
			<uui-box
				style="display: block; --uui-box-default-padding: var(--uui-size-space-4) var(--uui-size-space-5); border-left: 4px solid var(--uui-color-warning-standalone, #f0ac00);">
				<div style="display: flex; gap: var(--uui-size-space-4); align-items: flex-start;">
					<umb-icon
						name="icon-info"
						style="flex: 0 0 auto; font-size: var(--uui-size-6); margin-top: 2px; color: var(--uui-color-warning-standalone, #f0ac00);"></umb-icon>
					<div>
						<strong><umb-localize key="schemaLockdown_headline">Schema Locked</umb-localize></strong>
						<p style="margin: var(--uui-size-space-2) 0 0;">
							<umb-localize key="schemaLockdown_notice"></umb-localize>
						</p>
					</div>
				</div>
			</uui-box>
		`;
	}
}

export { UmbDictionaryCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-collection': UmbDictionaryCollectionElement;
	}
}
