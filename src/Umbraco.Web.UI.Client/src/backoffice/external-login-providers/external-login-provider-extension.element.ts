import { CSSResultGroup, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import type { ManifestExternalLoginProvider } from '@umbraco-cms/models';

@customElement('umb-external-login-provider-extension')
export class UmbExternalLoginProviderExtensionElement extends LitElement {
	static styles: CSSResultGroup = [UUITextStyles];

	private _externalLoginProvider?: ManifestExternalLoginProvider;

	@property({ type: Object })
	public get externalLoginProvider(): ManifestExternalLoginProvider | undefined {
		return this._externalLoginProvider;
	}
	public set externalLoginProvider(value: ManifestExternalLoginProvider | undefined) {
		this._externalLoginProvider = value;
		this._createElement();
	}

	@state()
	private _element?: any;

	private async _createElement() {
		if (!this.externalLoginProvider) return;

		try {
			this._element = (await createExtensionElement(this.externalLoginProvider)) as any | undefined;
		} catch (error) {
			// TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
		}
	}

	render() {
		return html`${this._element}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-external-login-provider-extension': UmbExternalLoginProviderExtensionElement;
	}
}
