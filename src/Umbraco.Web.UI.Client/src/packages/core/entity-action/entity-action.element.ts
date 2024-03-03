import type { UmbEntityAction } from './entity-action.interface.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, nothing, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-entity-action')
export class UmbEntityActionElement<
	ArgsMetaType extends object = object,
	ApiType extends UmbEntityAction<ArgsMetaType> = UmbEntityAction<ArgsMetaType>,
> extends UmbLitElement {
	#api?: ApiType;

	@property({ type: String })
	entityType?: string | null;

	@property({ type: String })
	public unique?: string | null;

	@property({ attribute: false })
	public manifest?: ManifestEntityAction<ArgsMetaType>;

	@property({ attribute: false })
	public set api(api: ApiType | undefined) {
		this.#api = api;

		// TODO: Fix so when we use a HREF it does not refresh the page?
		this.#api?.getHref?.().then((href) => {
			this._href = href;
			// TODO: Do we need to update the component here? [NL]
		});
	}

	@state()
	_href?: string;

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

	render() {
		return html`
			<uui-menu-item
				label=${ifDefined(this.manifest?.meta.label)}
				href=${ifDefined(this._href)}
				@click-label=${this.#onClickLabel}
				@click=${this.#onClick}>
				${this.manifest?.meta.icon
					? html`<umb-icon slot="icon" name="${this.manifest?.meta.icon}"></umb-icon>`
					: nothing}
			</uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-action': UmbEntityActionElement;
	}
}
