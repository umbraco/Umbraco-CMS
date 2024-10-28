import type { UmbEntityCreateOptionAction } from '../entity-create-option-action.interface.js';
import type { UmbEntityCreateOptionActionElement } from '../entity-create-option-action-element.interface.js';
import type { ManifestEntityCreateOptionAction } from '../entity-create-option-action.extension.js';
import type { MetaEntityCreateOptionActionDefaultKind } from './types.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, nothing, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-entity-create-option-action';
@customElement(elementName)
export class UmbEntityCreateOptionActionDefaultElement<
		MetaType extends MetaEntityCreateOptionActionDefaultKind = MetaEntityCreateOptionActionDefaultKind,
		ApiType extends UmbEntityCreateOptionAction<MetaType> = UmbEntityCreateOptionAction<MetaType>,
	>
	extends UmbLitElement
	implements UmbEntityCreateOptionActionElement
{
	#api?: ApiType;

	// TODO: Do these need to be properties? [NL]
	@property({ type: String })
	entityType?: string | null;

	// TODO: Do these need to be properties? [NL]
	@property({ type: String })
	public unique?: string | null;

	@property({ attribute: false })
	public manifest?: ManifestEntityCreateOptionAction<MetaType>;

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

	override async focus() {
		await this.updateComplete;
		this.shadowRoot?.querySelector('uui-menu-item')?.focus();
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
		const label = this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : this.manifest?.name;

		return html`
			<uui-menu-item
				label=${ifDefined(this.manifest?.meta.additionalOptions ? label + '...' : label)}
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

export { UmbEntityCreateOptionActionDefaultElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbEntityCreateOptionActionDefaultElement;
	}
}
