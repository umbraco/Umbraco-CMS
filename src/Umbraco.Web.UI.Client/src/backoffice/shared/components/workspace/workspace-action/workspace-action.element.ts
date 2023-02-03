import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/element';
import type { ManifestWorkspaceAction } from '@umbraco-cms/models';

@customElement('umb-workspace-action')
export class UmbWorkspaceActionElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	private _buttonState?: UUIButtonState;

	private _manifest?: ManifestWorkspaceAction;
	@property({ type: Object, attribute: false })
	public get manifest() {
		return this._manifest;
	}
	public set manifest(value: ManifestWorkspaceAction | undefined) {
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
		this.#api = new this._manifest.meta.api(this);
	}

	#api: any;

	private async _onClick() {
		await this.#api.execute();
	}

	render() {
		return html`
			<uui-button
				@click=${this._onClick}
				look=${this.manifest?.meta.look || 'default'}
				color=${this.manifest?.meta.color || 'default'}
				label=${this.manifest?.meta.label || ''}
				.state="${this._buttonState}"></uui-button>
		`;
	}
}

export default UmbWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action': UmbWorkspaceActionElement;
	}
}
