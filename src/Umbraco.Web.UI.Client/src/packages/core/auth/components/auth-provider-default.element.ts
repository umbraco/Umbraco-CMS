import type { UmbAuthProviderDefaultProps, UmbUserLoginState } from '../types.js';
import type { ManifestAuthProvider } from '../auth-provider.extension.js';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-auth-provider-default')
export class UmbAuthProviderDefaultElement extends UmbLitElement implements UmbAuthProviderDefaultProps {
	@property({ attribute: false })
	userLoginState?: UmbUserLoginState | undefined;

	@property({ attribute: false })
	manifest!: ManifestAuthProvider;

	@property({ attribute: false })
	onSubmit!: (manifestOrProviderName: string | ManifestAuthProvider, loginHint?: string) => void;

	override connectedCallback(): void {
		super.connectedCallback();
		this.setAttribute('part', 'auth-provider-default');
	}

	get #label() {
		const label = this.manifest.meta?.label ?? this.manifest.forProviderName;
		const labelLocalized = this.localize.string(label);
		return this.localize.term('login_signInWith', labelLocalized);
	}

	override render() {
		return html`
			<uui-button
				type="button"
				@click=${() => this.onSubmit(this.manifest)}
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

	static override styles = [
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
