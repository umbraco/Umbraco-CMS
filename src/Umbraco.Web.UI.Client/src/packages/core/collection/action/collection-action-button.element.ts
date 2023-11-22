import { UmbCollectionAction } from './collection-action-base.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	umbExtensionsRegistry,
	type ManifestCollectionAction,
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
export class UmbCollectionActionButtonElement extends UmbLitElement {
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
		if (!this._manifest) throw new Error('No manifest defined');
		if (!this._manifest.api) return;
		this.#api = (await createExtensionApi(this._manifest, [this])) as unknown as UmbCollectionAction;
	}

	#api?: UmbCollectionAction;

	private async _onClick() {
		if (!this.#api) return;
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
				href="${ifDefined(this.manifest?.meta.href)}"
				.state=${this._buttonState}></uui-button>
		`;
	}
}

export default UmbCollectionActionButtonElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-action': UmbCollectionActionButtonElement;
	}
}
