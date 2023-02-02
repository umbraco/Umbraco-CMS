import { html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

@customElement('umb-entity-action')
class UmbEntityActionElement extends UmbLitElement {
	private _unique?: string;
	@property({ type: String })
	public get unique() {
		return this._unique;
	}
	public set unique(value: string | undefined) {
		if (!value) return;
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

	#createApi() {
		if (!this._manifest?.meta.api) return;
		this.#api = new this._manifest.meta.api(this, this.unique);
	}

	#api: any;

	async #onClickLabel() {
		await this.#api.execute();
	}

	render() {
		return html`
			<uui-menu-item label="${ifDefined(this._manifest?.meta.label)}" @click-label=${this.#onClickLabel}>
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
