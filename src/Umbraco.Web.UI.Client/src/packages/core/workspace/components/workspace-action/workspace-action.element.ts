import type { UmbWorkspaceAction } from './index.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

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

	async #createApi() {
		if (!this._manifest) return;
		this.#api = await createExtensionApi(this._manifest, [this]);
	}

	#api?: UmbWorkspaceAction;

	private async _onClick() {
		this._buttonState = 'waiting';

		try {
			if (!this.#api) throw new Error('No api defined');
			await this.#api.execute();
			this._buttonState = 'success';
		} catch (error) {
			this._buttonState = 'failed';
		}

		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	render() {
		return html`
			<uui-button
				id="action-button"
				@click=${this._onClick}
				look=${this.manifest?.meta.look || 'default'}
				color=${this.manifest?.meta.color || 'default'}
				label=${this.manifest?.meta.label || ''}
				.state=${this._buttonState}></uui-button>
		`;
	}
}

export default UmbWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action': UmbWorkspaceActionElement;
	}
}
