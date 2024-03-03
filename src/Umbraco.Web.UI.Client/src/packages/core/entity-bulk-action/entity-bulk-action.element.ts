import type { UmbEntityBulkActionBase } from './entity-bulk-action-base.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, ifDefined, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestEntityBulkAction, MetaEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-bulk-action')
export class UmbEntityBulkActionElement<
	MetaType = MetaEntityBulkAction,
	ApiType extends UmbEntityBulkActionBase<MetaType> = UmbEntityBulkActionBase<MetaType>,
> extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestEntityBulkAction<MetaEntityBulkAction>;

	api?: ApiType;

	async #onClick(event: PointerEvent) {
		if (!this.api) return;
		event.stopPropagation();
		await this.api.execute();
		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	render() {
		return html`<uui-button
			@click=${this.#onClick}
			label=${ifDefined(this.manifest?.meta.label)}
			color="default"
			look="secondary"></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-bulk-action': UmbEntityBulkActionElement;
	}
}
