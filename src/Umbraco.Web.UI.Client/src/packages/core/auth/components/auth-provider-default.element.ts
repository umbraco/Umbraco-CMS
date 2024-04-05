import type { ManifestAuthProvider } from '../../index.js';
import { UmbLitElement } from '../../lit-element/lit-element.element.js';
import { UmbTextStyles } from '../../style/index.js';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-auth-provider-default')
export class UmbAuthProviderDefaultElement extends UmbLitElement {
	@property({ attribute: false })
	manifest!: ManifestAuthProvider;

	@property({ attribute: false })
	onSubmit!: (providerName: string) => void;

	render() {
		return html`
			<uui-button
				type="button"
				@click=${() => this.onSubmit(this.manifest.forProviderName)}
				id="auth-provider-button"
				.label=${this.manifest.meta?.label ?? this.manifest.forProviderName}
				.look=${this.manifest.meta?.defaultView?.look ?? 'outline'}
				.color=${this.manifest.meta?.defaultView?.color ?? 'default'}>
				${this.manifest.meta?.defaultView?.icon
					? html`<umb-icon .icon=${this.manifest.meta?.defaultView?.icon}></umb-icon>`
					: nothing}
				${this.manifest.meta?.label ?? this.manifest.forProviderName}
			</uui-button>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: 20px;
			}

			#auth-provider-button {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth-provider-default': UmbAuthProviderDefaultElement;
	}
}
