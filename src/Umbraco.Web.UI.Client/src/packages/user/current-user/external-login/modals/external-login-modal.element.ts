import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserExternalLoginProviderModel } from '../../types.js';
import { css, customElement, html, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbConfirmModal, type UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';

type UmbExternalLoginProviderOption = UmbCurrentUserExternalLoginProviderModel & {
	displayName: string;
	icon?: string;
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
						icon: manifestLoginProvider.meta?.defaultView?.icon,
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
			<uui-box>
				<div class="header" slot="header">
					<uui-icon name=${item.icon ?? 'icon-cloud'}></uui-icon>
					${item.displayName}
				</div>
				${when(
					item.isEnabledOnUser,
					() => html`
						<p style="margin-top:0">
							<umb-localize key="defaultdialogs_linkedToService">Your account is linked to this service</umb-localize>
						</p>
						<uui-button
							type="button"
							look="secondary"
							color="danger"
							.label=${this.localize.term('defaultdialogs_unLinkYour', item.displayName)}
							@click=${() => this.#onProviderDisable(item)}>
							<umb-localize key="defaultdialogs_unLinkYour" .args=${[item.displayName]}>
								Unlink your ${item.displayName} account
							</umb-localize>
							<uui-icon name="icon-window-popout"></uui-icon>
						</uui-button>
					`,
					() => html`
						<uui-button
							type="button"
							look="secondary"
							.label=${this.localize.term('defaultdialogs_linkYour', item.displayName)}
							@click=${() => this.#onProviderEnable(item)}>
							<umb-localize key="defaultdialogs_linkYour" .args=${[item.displayName]}>
								Link your ${item.displayName} account
							</umb-localize>
							<uui-icon name="icon-window-popout"></uui-icon>
						</uui-button>
					`,
				)}
			</uui-box>
		`;
	}

	async #onProviderEnable(item: UmbExternalLoginProviderOption) {
		const authContext = await this.getContext(UMB_AUTH_CONTEXT);
		authContext.linkLogin(item.providerName);
	}

	async #onProviderDisable(item: UmbExternalLoginProviderOption) {
		try {
			await umbConfirmModal(this, {
				headline: this.localize.term('defaultdialogs_unLinkYour', item.displayName),
				content: this.localize.term('defaultdialogs_unLinkYourConfirm', item.displayName),
				confirmLabel: this.localize.term('general_unlink'),
				color: 'danger',
			});
			const authContext = await this.getContext(UMB_AUTH_CONTEXT);
			authContext.unlinkLogin(item.providerName, item.providerKey);
		} catch {
			// Do nothing
		}
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-box {
				margin-bottom: var(--uui-size-space-3);
			}

			.header {
				display: flex;
				align-items: center;
			}

			.header uui-icon {
				margin-right: var(--uui-size-space-4);
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
