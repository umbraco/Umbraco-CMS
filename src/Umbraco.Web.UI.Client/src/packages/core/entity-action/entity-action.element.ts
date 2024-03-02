import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, nothing, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-entity-action')
export class UmbEntityActionElement extends UmbLitElement {
	#entityType?: string | null;

	@property({ type: String })
	public set entityType(value: string | undefined | null) {
		const oldValue = this.#entityType;
		this.#entityType = value;
		if (oldValue !== this.#entityType) {
			this.#createApi();
			this.requestUpdate('entityType', oldValue);
		}
	}
	public get entityType() {
		return this.#entityType;
	}

	#unique?: string | null;

	@property({ type: String })
	public set unique(value: string | undefined | null) {
		const oldValue = this.#unique;
		this.#unique = value;
		if (oldValue !== this.#unique) {
			this.#createApi();
			this.requestUpdate('unique', oldValue);
		}
	}
	public get unique() {
		return this.#unique;
	}

	#manifest?: ManifestEntityAction;

	@property({ type: Object, attribute: false })
	public set manifest(value: ManifestEntityAction | undefined) {
		if (!value) return;
		const oldValue = this.#manifest;
		this.#manifest = value;
		if (oldValue !== this.#manifest) {
			this.#createApi();
			this.requestUpdate('manifest', oldValue);
		}
	}
	public get manifest() {
		return this.#manifest;
	}

	async #createApi() {
		// only create the api if we have all the required properties
		if (!this.#manifest) return;
		if (this.#unique === undefined) return;
		if (!this.#entityType) return;

		this.#api = await createExtensionApi(this.#manifest, [
			this,
			{
				unique: this.#unique,
				entityType: this.#entityType,
				...this.#manifest.meta,
			},
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
				label=${ifDefined(this.manifest?.meta.label)}
				href=${ifDefined(this._href)}
				@click-label=${this.#onClickLabel}
				@click=${this.#onClick}>
				${this.manifest?.meta.icon
					? html`<uui-icon slot="icon" name="${this.manifest?.meta.icon}"></uui-icon>`
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
