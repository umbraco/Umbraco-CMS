import type { UmbWorkspaceActionMenuItem } from '../types.js';
import type {
	ManifestWorkspaceActionMenuItemDefaultKind,
	MetaWorkspaceActionMenuItemDefaultKind,
} from '../../../extensions/types.js';
import { customElement, html, ifDefined, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-workspace-action-menu-item')
export class UmbWorkspaceActionMenuItemElement<
	MetaType extends MetaWorkspaceActionMenuItemDefaultKind = MetaWorkspaceActionMenuItemDefaultKind,
	ApiType extends UmbWorkspaceActionMenuItem<MetaType> = UmbWorkspaceActionMenuItem<MetaType>,
> extends UmbLitElement {
	#api?: ApiType;

	@state()
	private _href?: string;

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
			await this.#api?.execute().catch(() => {});
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
				label=${ifDefined(this.localize.string(this.manifest?.meta.label ?? this.manifest?.name))}
				href=${ifDefined(this._href)}
				@click=${this.#onClick}
				@click-label=${this.#onClickLabel}>
				${when(this.manifest?.meta.icon, (icon) => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
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
