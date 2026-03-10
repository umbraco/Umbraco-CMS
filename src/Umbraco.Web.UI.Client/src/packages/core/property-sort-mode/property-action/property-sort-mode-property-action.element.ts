import { UMB_PROPERTY_SORT_MODE_CONTEXT } from '../property-context/property-sort-mode.context-token.js';
import type { ManifestPropertyActionSortModeKind } from './types.js';
import { customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyAction } from '@umbraco-cms/backoffice/property-action';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-property-sort-mode-property-action')
export class UmbPropertySortModePropertyActionElement extends UmbLitElement {
	@state()
	private _isSortMode = false;

	@property({ attribute: false })
	public manifest?: ManifestPropertyActionSortModeKind;

	@property({ attribute: false })
	public api?: UmbPropertyAction<ManifestPropertyActionSortModeKind> | undefined;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_SORT_MODE_CONTEXT, (context) => {
			this.observe(context?.isSortMode, (isSortMode) => (this._isSortMode = isSortMode ?? false));
		});
	}

	async #onClickLabel(event: UUIMenuItemEvent) {
		event.stopPropagation();
		await this.api?.execute().catch(() => {});
		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	// TODO: we need to stop the regular click event from bubbling up to the table so it doesn't select the row.
	// This should probably be handled in the UUI Menu item component. so we don't dispatch a label-click event and click event at the same time.
	#onClick(event: PointerEvent) {
		event.stopPropagation();
	}

	override render() {
		const label = this._isSortMode ? 'blockEditor_actionExitSortMode' : 'blockEditor_actionEnterSortMode';
		return html`
			<uui-menu-item label=${this.localize.term(label)} @click-label=${this.#onClickLabel} @click=${this.#onClick}>
				${when(this.manifest?.meta.icon, (icon) => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
			</uui-menu-item>
		`;
	}
}

export { UmbPropertySortModePropertyActionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-sort-mode-property-action': UmbPropertySortModePropertyActionElement;
	}
}
