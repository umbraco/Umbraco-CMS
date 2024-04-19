import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserExternalLoginProviderModel } from '../../types.js';
import { css, customElement, html, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';

type UmbExternalLoginProviderOption = UmbCurrentUserExternalLoginProviderModel & {
	displayName: string;
	isEnabledOnUser: boolean;
};

@customElement('umb-current-user-external-login-modal')
export class UmbCurrentUserExternalLoginModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	_items: Array<UmbExternalLoginProviderOption> = [];

	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor() {
		super();
		this.#loadProviders();
	}

	async #loadProviders() {
		const serverLoginProviders$ = (await this.#currentUserRepository.requestExternalLoginProviders()).asObservable();
		const manifestLoginProviders$ = umbExtensionsRegistry.byTypeAndFilter(
			'authProvider',
			(ext) => !!ext.meta?.linking?.allowManualLinking,
		);

		// Merge the server and manifest providers to get the final list of providers
		const externalLoginProviders$ = mergeObservables(
			[serverLoginProviders$, manifestLoginProviders$],
			([serverLoginProviders, manifestLoginProviders]) => {
				const providers: UmbExternalLoginProviderOption[] = manifestLoginProviders.map((manifestLoginProvider) => {
					const serverLoginProvider = serverLoginProviders.find(
						(serverLoginProvider) => serverLoginProvider.providerName === manifestLoginProvider.forProviderName,
					);
					return {
						isEnabledOnUser: !!serverLoginProvider,
						providerKey: manifestLoginProvider.forProviderName,
						providerName: manifestLoginProvider.forProviderName,
						displayName:
							manifestLoginProvider.meta?.label ?? manifestLoginProvider.forProviderName ?? manifestLoginProvider.name,
					};
				});

				return providers;
			},
		);

		this.observe(
			externalLoginProviders$,
			(providers) => {
				this._items = providers;
			},
			'_externalLoginProviders',
		);
	}

	#close() {
		this.modalContext?.submit();
	}

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('defaultdialogs_externalLoginProviders')}">
				<div id="main">
					${when(
						this._items.length > 0,
						() => html`
							${repeat(
								this._items,
								(item) => item.providerName,
								(item) => this.#renderProvider(item),
							)}
						`,
					)}
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" .label=${this.localize.term('general_close')}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	/**
	 * Render a provider with a toggle to enable/disable it
	 */
	#renderProvider(item: UmbExternalLoginProviderOption) {
		return html`
			<uui-box headline=${item.displayName}>
				${when(
					item.isEnabledOnUser,
					() => html`
						<p style="margin-top:0">
							<umb-localize key="user_2faProviderIsEnabled">This provider is enabled</umb-localize>
							<uui-icon icon="check"></uui-icon>
						</p>
						<uui-button
							type="button"
							look="secondary"
							color="danger"
							.label=${this.localize.term('actions_disable')}
							@click=${() => this.#onProviderDisable(item)}></uui-button>
					`,
					() => html`
						<uui-button
							type="button"
							look="secondary"
							.label=${this.localize.term('actions_enable')}
							@click=${() => this.#onProviderEnable(item)}></uui-button>
					`,
				)}
			</uui-box>
		`;
	}

	async #onProviderEnable(item: UmbExternalLoginProviderOption) {
		alert('Enable provider ' + item.providerName);
	}

	async #onProviderDisable(item: UmbExternalLoginProviderOption) {
		alert('Disable provider ' + item.providerName);
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-box {
				margin-bottom: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbCurrentUserExternalLoginModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-external-login-modal': UmbCurrentUserExternalLoginModalElement;
	}
}
