import type { ManifestAuthProvider } from '../../extension-registry/models/index.js';
import type { UmbAuthProviderDefaultProps } from '../types.js';
import { UmbLitElement } from '../../lit-element/lit-element.element.js';
import { UmbTextStyles } from '../../style/index.js';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-auth-provider-default')
export class UmbAuthProviderDefaultElement extends UmbLitElement implements UmbAuthProviderDefaultProps {
	@property({ attribute: false })
	manifest!: ManifestAuthProvider;

	@property({ attribute: false })
	onSubmit!: (providerName: string, loginHint?: string) => void;

	connectedCallback(): void {
		super.connectedCallback();
		this.setAttribute('part', 'auth-provider-default');
	}

	get #label() {
		const label = this.manifest.meta?.label ?? this.manifest.forProviderName;
		const labelLocalized = this.localize.string(label);
		return this.localize.term('login_signInWith', labelLocalized);
	}

	render() {
		return html`
			<uui-button
				type="button"
				@click=${() => this.onSubmit(this.manifest.forProviderName)}
				id="auth-provider-button"
				.label=${this.#label}
				.look=${this.manifest.meta?.defaultView?.look ?? 'outline'}
				.color=${this.manifest.meta?.defaultView?.color ?? 'default'}>
				${this.manifest.meta?.defaultView?.icon
					? html`<uui-icon id="icon" .name=${this.manifest.meta?.defaultView?.icon}></uui-icon>`
					: nothing}
				${this.#label}
			</uui-button>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			#auth-provider-button {
				width: 100%;
			}

			#icon {
				margin-right: var(--uui-size-space-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth-provider-default': UmbAuthProviderDefaultElement;
	}
}
