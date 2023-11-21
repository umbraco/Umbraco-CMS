import { UmbCollectionAction } from './collection-action-base.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	umbExtensionsRegistry,
	type ManifestWorkspaceAction as ManifestCollectionAction,
	UmbBackofficeManifestKind,
} from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.CollectionAction.Button',
	matchKind: 'button',
	matchType: 'collectionAction',
	manifest: {
		type: 'collectionAction',
		kind: 'button',
		elementName: 'umb-collection-action-button',
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-collection-action-button')
export class UmbCollectionActionElement extends UmbLitElement {
	@state()
	private _buttonState?: UUIButtonState;

	private _manifest?: ManifestCollectionAction;
	@property({ type: Object, attribute: false })
	public get manifest() {
		return this._manifest;
	}
	public set manifest(value: ManifestCollectionAction | undefined) {
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

	#api?: UmbCollectionAction;

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
				look="outline"
				color="default"
				label=${this.manifest?.meta.label || ''}
				.state=${this._buttonState}></uui-button>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-action': UmbCollectionActionElement;
	}
}
