import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';

@customElement('umb-dictionary-collection')
export class UmbDictionaryCollectionElement extends UmbCollectionDefaultElement {
	// Restricted until the server confirms it is not in production runtime mode (safe default).
	@state()
	private _isRestricted = true;

	constructor() {
		super();
		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.observe(context?.isProductionMode, (isProductionMode) => {
				this._isRestricted = isProductionMode !== false;
			});
		});
	}

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
			${this.#renderProductionModeNotice()}
		`;
	}

	#renderProductionModeNotice() {
		if (!this._isRestricted) return nothing;
		return html`
			<uui-box
				slot="header"
				style="width: 100%; --uui-box-default-padding: var(--uui-size-space-4) var(--uui-size-space-5); border-left: 4px solid var(--uui-color-warning-standalone, #f0ac00);">
				<div style="display: flex; gap: var(--uui-size-space-4); align-items: flex-start;">
					<umb-icon
						name="icon-info"
						style="flex: 0 0 auto; font-size: var(--uui-size-6); margin-top: 2px; color: var(--uui-color-warning-standalone, #f0ac00);"></umb-icon>
					<div>
						<strong><umb-localize key="general_productionMode">Production Mode</umb-localize></strong>
						<p style="margin: var(--uui-size-space-2) 0 0;">
							<umb-localize key="general_runtimeModeProductionSchema"></umb-localize>
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
