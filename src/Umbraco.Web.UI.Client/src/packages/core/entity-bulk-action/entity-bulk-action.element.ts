import type { UmbEntityBulkAction } from './entity-bulk-action.interface.js';
import type { UmbEntityBulkActionElement } from './entity-bulk-action-element.interface.js';
import { html, ifDefined, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestEntityBulkAction,
	MetaEntityBulkActionDefaultKind,
} from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-entity-bulk-action';

@customElement(elementName)
export class UmbEntityBulkActionDefaultElement<
		MetaType extends MetaEntityBulkActionDefaultKind = MetaEntityBulkActionDefaultKind,
		ApiType extends UmbEntityBulkAction<MetaType> = UmbEntityBulkAction<MetaType>,
	>
	extends UmbLitElement
	implements UmbEntityBulkActionElement
{
	@property({ attribute: false })
	manifest?: ManifestEntityBulkAction<MetaType>;

	api?: ApiType;

	async #onClick(event: PointerEvent) {
		if (!this.api) return;
		event.stopPropagation();
		await this.api.execute();
		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	override render() {
		return html`
			<uui-button
				color="default"
				label=${ifDefined(this.localize.string(this.manifest?.meta.label ?? ''))}
				look="secondary"
				@click=${this.#onClick}></uui-button>
		`;
	}
}

export default UmbEntityBulkActionDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbEntityBulkActionDefaultElement;
	}
}
