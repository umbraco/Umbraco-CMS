import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserExternalLoginProviderModel } from '../../types.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbConfirmModal, type UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { ProblemDetails } from '@umbraco-cms/backoffice/external/backend-api';

type UmbExternalLoginProviderOption = UmbCurrentUserExternalLoginProviderModel & {
	displayName: string;
	icon?: string;
	existsOnServer: boolean;
};

@customElement('umb-current-user-external-login-modal')
export class UmbCurrentUserExternalLoginModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	_items: Array<UmbExternalLoginProviderOption> = [];

	#currentUserRepository = new UmbCurrentUserRepository(this);
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor() {
		super();
		this.#loadProviders();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
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
						(serverLoginProvider) => serverLoginProvider.providerSchemeName === manifestLoginProvider.forProviderName,
					);
					return {
						existsOnServer: !!serverLoginProvider,
						hasManualLinkingEnabled: serverLoginProvider?.hasManualLinkingEnabled ?? false,
						isLinkedOnUser: serverLoginProvider?.isLinkedOnUser ?? false,
						providerKey: serverLoginProvider?.providerKey ?? '',
						providerSchemeName: manifestLoginProvider.forProviderName,
						icon: manifestLoginProvider.meta?.defaultView?.icon,
						displayName:
							manifestLoginProvider.meta?.label ?? manifestLoginProvider.forProviderName ?? manifestLoginProvider.name,
					} satisfies UmbExternalLoginProviderOption;
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

	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('defaultdialogs_externalLoginProviders')}">
				<div id="main">
					${repeat(
						this._items,
						(item) => item.providerSchemeName,
						(item) => this.#renderProvider(item),
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
	 * @param item
	 */
	#renderProvider(item: UmbExternalLoginProviderOption) {
		return html`
			<uui-box>
				<div class="header" slot="header">
					<uui-icon class="header-icon" name=${item.icon ?? 'icon-cloud'}></uui-icon>
					${this.localize.string(item.displayName)}
				</div>
				${when(
					item.existsOnServer,
					() => nothing,
					() =>
						html`<div style="position:relative" slot="header-actions">
							<uui-badge
								style="cursor:default"
								title="Warning: This provider is not configured on the server"
								color="danger"
								look="primary">
								!
							</uui-badge>
						</div>`,
				)}
				${when(
					item.isLinkedOnUser,
					() => html`
						<p style="margin-top:0">
							<umb-localize key="defaultdialogs_linkedToService">Your account is linked to this service</umb-localize>
						</p>
						<uui-button
							type="button"
							look="secondary"
							color="danger"
							.label=${this.localize.term('defaultdialogs_unLinkYour', this.localize.string(item.displayName))}
							@click=${() => this.#onProviderDisable(item)}>
							<umb-localize key="defaultdialogs_unLinkYour" .args=${[this.localize.string(item.displayName)]}>
								Unlink your ${this.localize.string(item.displayName)} account
							</umb-localize>
							<uui-icon name="icon-window-popout"></uui-icon>
						</uui-button>
					`,
					() => html`
						<uui-button
							type="button"
							look="secondary"
							.label=${this.localize.term('defaultdialogs_linkYour', item.displayName)}
							?disabled=${!item.existsOnServer || !item.hasManualLinkingEnabled}
							@click=${() => this.#onProviderEnable(item)}>
							<umb-localize key="defaultdialogs_linkYour" .args=${[this.localize.string(item.displayName)]}>
								Link your ${this.localize.string(item.displayName)} account
							</umb-localize>
							<uui-icon name="icon-window-popout"></uui-icon>
						</uui-button>
					`,
				)}
			</uui-box>
		`;
	}

	async #onProviderEnable(item: UmbExternalLoginProviderOption) {
		const providerDisplayName = this.localize.string(item.displayName);
		try {
			await umbConfirmModal(this, {
				headline: this.localize.term('defaultdialogs_linkYour', providerDisplayName),
				content: this.localize.term('defaultdialogs_linkYourConfirm', providerDisplayName),
				confirmLabel: this.localize.term('general_continue'),
				color: 'positive',
			});
			const authContext = await this.getContext(UMB_AUTH_CONTEXT);
			await authContext.linkLogin(item.providerSchemeName);
		} catch (error) {
			if (error instanceof Error) {
				this.#notificationContext?.peek('danger', {
					data: {
						headline: this.localize.term('defaultdialogs_linkYour', providerDisplayName),
						message: error.message,
					},
				});
			}
		}
	}

	async #onProviderDisable(item: UmbExternalLoginProviderOption) {
		if (!item.providerKey) {
			throw new Error('Provider key is missing');
		}

		const providerDisplayName = this.localize.string(item.displayName);
		try {
			await umbConfirmModal(this, {
				headline: this.localize.term('defaultdialogs_unLinkYour', providerDisplayName),
				content: this.localize.term('defaultdialogs_unLinkYourConfirm', providerDisplayName),
				confirmLabel: this.localize.term('general_continue'),
				color: 'danger',
			});
			const authContext = await this.getContext(UMB_AUTH_CONTEXT);
			await authContext.unlinkLogin(item.providerSchemeName, item.providerKey);
		} catch (error) {
			let message = this.localize.term('errors_receivedErrorFromServer');
			if (error instanceof Error) {
				message = error.message;
			} else if (typeof error === 'object' && (error as ProblemDetails).title) {
				message = (error as ProblemDetails).title ?? message;
			}
			console.error('[External Login] Error unlinking provider: ', error);
			this.#notificationContext?.peek('danger', {
				data: {
					headline: this.localize.term('defaultdialogs_unLinkYour', providerDisplayName),
					message,
				},
			});
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-box {
				margin-bottom: var(--uui-size-space-3);
			}

			.header {
				display: flex;
				align-items: center;
			}

			.header-icon {
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
