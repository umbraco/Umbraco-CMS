import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

@customElement('umb-entity-action')
class UmbEntityActionElement extends UmbLitElement {
	private _manifest?: ManifestEntityAction;
	@property({ type: Object, attribute: false })
	public get entityType() {
		return this._manifest;
	}
	public set manifest(value: ManifestEntityAction | undefined) {
		if (!value) return;
		const oldValue = this._manifest;
		this._manifest = value;
		if (oldValue !== this._manifest) {
			this.#api = new this._manifest.meta.api(this);
			this.requestUpdate('manifest', oldValue);
		}
	}

	#api: any;

	#onClickLabel() {
		this.#api.execute();
	}

	render() {
		return html`
			<uui-menu-item
				label="${ifDefined(this._manifest?.meta.label)}"
				@click-label=${this.#onClickLabel}></uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-action': UmbEntityActionElement;
	}
}
