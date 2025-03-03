import type { UmbWorkspaceActionMenuItem } from '../types.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestWorkspaceActionMenuItemDefaultKind,
	MetaWorkspaceActionMenuItemDefaultKind,
} from '@umbraco-cms/backoffice/workspace';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-workspace-action-menu-item')
export class UmbWorkspaceActionMenuItemElement<
	MetaType extends MetaWorkspaceActionMenuItemDefaultKind = MetaWorkspaceActionMenuItemDefaultKind,
	ApiType extends UmbWorkspaceActionMenuItem<MetaType> = UmbWorkspaceActionMenuItem<MetaType>,
> extends UmbLitElement {
	#api?: ApiType;

	@state()
	_href?: string;

	@property({ attribute: false })
	public manifest?: ManifestWorkspaceActionMenuItemDefaultKind<MetaType>;

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
				label=${ifDefined(
					this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : this.manifest?.name,
				)}
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

export default UmbWorkspaceActionMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu-item': UmbWorkspaceActionMenuItemElement;
	}
}
