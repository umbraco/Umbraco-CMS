import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import type { UUIButtonState } from '@umbraco-ui/uui';
import { UmbWorkspaceAction } from './';
import { UmbExecutedEvent } from '@umbraco-cms/backoffice/events';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-workspace-action')
export class UmbWorkspaceActionElement extends UmbLitElement {
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

	#api?: UmbWorkspaceAction;

	private async _onClick() {
		if (!this.#api) return;
		await this.#api.execute();
		this.dispatchEvent(new UmbExecutedEvent());
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

	static styles = [UUITextStyles, css``];
}

export default UmbWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action': UmbWorkspaceActionElement;
	}
}
