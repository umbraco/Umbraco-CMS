import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbEntityBulkAction } from '@umbraco-cms/backoffice/entity-action';
import { UmbExecutedEvent } from '@umbraco-cms/backoffice/events';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extensions-registry';

@customElement('umb-entity-bulk-action')
class UmbEntityBulkActionElement extends UmbLitElement {
	private _selection: Array<string> = [];
	@property({ type: Array, attribute: false })
	public get selection() {
		return this._selection;
	}
	public set selection(value: Array<string>) {
		if (!value || value === this._selection) return;
		const oldValue = this._selection;
		this._selection = value;
		this.#api ? this.#api.setSelection(this._selection) : this.#createApi();
		this.requestUpdate('selection', oldValue);
	}

	private _manifest?: ManifestEntityBulkAction;
	@property({ type: Object, attribute: false })
	public get manifest() {
		return this._manifest;
	}
	public set manifest(value: ManifestEntityBulkAction | undefined) {
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
		this.#api = new this._manifest.meta.api(this, this._manifest.meta.repositoryAlias, this._selection);
	}

	#api?: UmbEntityBulkAction;

	async #onClick(event: PointerEvent) {
		if (!this.#api) return;
		event.stopPropagation();
		await this.#api.execute();
		this.dispatchEvent(new UmbExecutedEvent());
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
