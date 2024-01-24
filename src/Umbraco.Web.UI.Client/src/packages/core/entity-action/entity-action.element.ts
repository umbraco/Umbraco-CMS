import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, nothing, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-entity-action')
export class UmbEntityActionElement extends UmbLitElement {
	private _entityType?: string | null;
	@property({ type: String })
	public get entityType() {
		return this._entityType;
	}
	public set entityType(value: string | undefined | null) {
		const oldValue = this._entityType;
		this._entityType = value;
		if (oldValue !== this._entityType) {
			this.#createApi();
			this.requestUpdate('entityType', oldValue);
		}
	}

	private _unique?: string | null;
	@property({ type: String })
	public get unique() {
		return this._unique;
	}
	public set unique(value: string | undefined | null) {
		const oldValue = this._unique;
		this._unique = value;
		if (oldValue !== this._unique) {
			this.#createApi();
			this.requestUpdate('unique', oldValue);
		}
	}

	private _manifest?: ManifestEntityAction;
	@property({ type: Object, attribute: false })
	public get manifest() {
		return this._manifest;
	}
	public set manifest(value: ManifestEntityAction | undefined) {
		if (!value) return;
		const oldValue = this._manifest;
		this._manifest = value;
		if (oldValue !== this._manifest) {
			this.#createApi();
			this.requestUpdate('manifest', oldValue);
		}
	}

	async #createApi() {
		// only create the api if we have all the required properties
		if (!this._manifest) return;
		if (this._unique === undefined) return;
		if (!this._entityType) return;

		this.#api = await createExtensionApi(this._manifest, [
			this,
			this._manifest.meta.repositoryAlias,
			this.unique,
			this.entityType,
		]);

		// TODO: Fix so when we use a HREF it does not refresh the page?
		this._href = await this.#api.getHref?.();
	}

	#api: any;

	@state()
	_href?: string;

	async #onClickLabel(event: UUIMenuItemEvent) {
		if (!this._href) {
			event.stopPropagation();
			await this.#api.execute();
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
				label=${ifDefined(this._manifest?.meta.label)}
				href=${ifDefined(this._href)}
				@click-label=${this.#onClickLabel}
				@click=${this.#onClick}>
				${this._manifest?.meta.icon
					? html`<uui-icon slot="icon" name="${this._manifest?.meta.icon}"></uui-icon>`
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
