import type {
	ManifestPropertyActionDefaultKind,
	MetaPropertyActionDefaultKind,
} from '../../property-action.extension.js';
import type { UmbPropertyAction } from '../../property-action.interface.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, ifDefined, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-property-action')
export class UmbPropertyActionElement<
	MetaType extends MetaPropertyActionDefaultKind = MetaPropertyActionDefaultKind,
	ApiType extends UmbPropertyAction<MetaType> = UmbPropertyAction<MetaType>,
> extends UmbLitElement {
	#api?: ApiType;

	@state()
	_href?: string;

	@property({ attribute: false })
	public manifest?: ManifestPropertyActionDefaultKind<MetaType>;

	public set api(api: ApiType | undefined) {
		this.#api = api;

		// TODO: Fix so when we use a HREF it does not refresh the page?
		this.#api?.getHref?.().then((href) => {
			this._href = href;
			// TODO: Do we need to update the component here? [NL]
		});
	}

	async #onClickLabel(event: UUIMenuItemEvent) {
		if (!this._href) {
			event.stopPropagation();
			await this.#api?.execute();
		}
		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	// TODO: we need to stop the regular click event from bubbling up to the table so it doesn't select the row.
	// This should probably be handled in the UUI Menu item component. so we don't dispatch a label-click event and click event at the same time.
	#onClick(event: PointerEvent) {
		event.stopPropagation();
	}

	override render() {
		return html`
			<uui-menu-item
				label=${this.localize.string(this.manifest?.meta.label)}
				href=${ifDefined(this._href)}
				@click-label=${this.#onClickLabel}
				@click=${this.#onClick}>
				${when(this.manifest?.meta.icon, (icon) => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
			</uui-menu-item>
		`;
	}
}

export default UmbPropertyActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-action': UmbPropertyActionElement;
	}
}
