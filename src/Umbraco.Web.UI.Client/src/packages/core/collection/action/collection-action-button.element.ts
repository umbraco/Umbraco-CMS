import type { UmbCollectionAction } from './collection-action-base.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestCollectionAction } from '../extensions/types.js';

@customElement('umb-collection-action-button')
export class UmbCollectionActionButtonElement extends UmbLitElement {
	#api?: UmbCollectionAction;

	@property({ type: Object, attribute: false })
	public get manifest() {
		return this._manifest;
	}
	public set manifest(value: ManifestCollectionAction | undefined) {
		const oldValue = this._manifest;
		if (oldValue !== value) {
			this._manifest = value;
			this._href = this.manifest?.meta.href;
			this._additionalOptions = this.manifest?.meta.additionalOptions;
			this.#createApi();
			this.requestUpdate('manifest', oldValue);
		}
	}
	private _manifest?: ManifestCollectionAction;

	async #createApi() {
		if (!this._manifest) throw new Error('No manifest defined');
		if (!this._manifest.api) return;
		this.#api = await createExtensionApi<UmbCollectionAction>(this, this._manifest);

		this._href = (await this.#api?.getHref?.()) ?? this.manifest?.meta.href;
		this._additionalOptions = (await this.#api?.hasAddionalOptions?.()) ?? this.manifest?.meta.additionalOptions;
	}

	@state()
	private _buttonState?: UUIButtonState;

	@state()
	private _additionalOptions?: boolean;

	@state()
	private _href?: string;

	private async _onClick() {
		// If its a link or has additional options, then we do not want to display state on the button. [NL]
		if (!this._href) {
			if (!this._additionalOptions) {
				this._buttonState = 'waiting';
			}

			try {
				if (!this.#api) throw new Error('No api defined');
				await this.#api.execute();
				if (!this._additionalOptions) {
					this._buttonState = 'success';
				}
			} catch {
				if (!this._additionalOptions) {
					this._buttonState = 'failed';
				}
			}
		}
		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	override render() {
		const label = this.manifest?.meta.label
			? this.localize.string(this.manifest.meta.label)
			: (this.manifest?.name ?? '');
		return html`
			<uui-button
				data-mark="collection-action:${this.manifest?.alias}"
				color="default"
				look="outline"
				.label=${this._additionalOptions ? label + '…' : label}
				.href=${this._href}
				.state=${this._buttonState}
				@click=${this._onClick}></uui-button>
		`;
	}
}

export default UmbCollectionActionButtonElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-action-button': UmbCollectionActionButtonElement;
	}
}
