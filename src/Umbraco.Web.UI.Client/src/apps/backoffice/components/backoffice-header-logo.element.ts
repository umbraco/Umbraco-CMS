import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import { isCurrentUserAnAdmin } from '@umbraco-cms/backoffice/current-user';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_NEWVERSION_MODAL, UMB_SYSINFO_MODAL } from '@umbraco-cms/backoffice/sysinfo';
import type { UmbServerUpgradeCheck } from '@umbraco-cms/backoffice/sysinfo';

/**
 * The backoffice header logo element.
 * @cssprop --umb-header-logo-display - The display property of the header logo.
 * @cssprop --umb-header-logo-margin - The margin of the header logo.
 * @cssprop --umb-header-logo-width - The width of the header logo.
 * @cssprop --umb-header-logo-height - The height of the header logo.
 * @cssprop --umb-logo-display - The display property of the logo.
 * @cssprop --umb-logo-width - The width of the logo.
 * @cssprop --umb-logo-height - The height of the logo.
 */
@customElement('umb-backoffice-header-logo')
export class UmbBackofficeHeaderLogoElement extends UmbLitElement {
	@state()
	private _version?: string;

	@state()
	private _isUserAdmin = false;

	@state()
	private _serverUpgradeCheck: UmbServerUpgradeCheck | null = null;

	#backofficeContext?: typeof UMB_BACKOFFICE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT, (context) => {
			this.observe(
				context.version,
				(version) => {
					if (!version) return;
					this._version = version;
				},
				'_observeVersion',
			);

			this.#backofficeContext = context;
		});
	}

	protected override async firstUpdated() {
		this.#isAdmin();
	}

	async #isAdmin() {
		this._isUserAdmin = await isCurrentUserAnAdmin(this);

		if (this._isUserAdmin) {
			this._serverUpgradeCheck = this.#backofficeContext ? await this.#backofficeContext.serverUpgradeCheck() : null;
		}
	}

	override render() {
		return html`
			<uui-button id="header-logo-button" look="primary" label="Logo" compact popovertarget="logo-popover">
				<umb-app-logo id="header-logo" loading="eager"></umb-app-logo>
			</uui-button>
			<uui-popover-container id="logo-popover" placement="bottom-start">
				<umb-popover-layout>
					<div id="modal">
						<umb-app-logo id="logo" logo-type="logo" alternative></umb-app-logo>
						<span>${this._version}</span>

						${this._serverUpgradeCheck
							? html`<uui-button
									@click=${this.#openNewVersion}
									color="positive"
									label=${this.localize.term('general_newVersionAvailable')}></uui-button>`
							: ''}

						<a href="https://umbraco.com" target="_blank" rel="noopener">Umbraco.com</a>

						<uui-button @click=${this.#openSystemInformation} look="secondary" label="System information"></uui-button>
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	async #openSystemInformation() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager
			.open(this, UMB_SYSINFO_MODAL)
			.onSubmit()
			.catch(() => {});
	}

	async #openNewVersion() {
		if (!this._serverUpgradeCheck) return;
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager
			.open(this, UMB_NEWVERSION_MODAL, {
				data: {
					comment: this._serverUpgradeCheck.comment,
					downloadUrl: this._serverUpgradeCheck.url,
				},
			})
			.onSubmit()
			.catch(() => {});
	}

	static override styles = [
		UmbTextStyles,
		css`
			#header-logo-button {
				--uui-button-padding-top-factor: 1;
				--uui-button-padding-bottom-factor: 0.5;
				--uui-button-background-color: transparent;
				display: var(--umb-header-logo-display, inline);
				margin: var(--umb-header-logo-margin, 0 var(--uui-size-space-2) 0 0);
			}

			#header-logo > img {
				width: var(--umb-header-logo-width, auto);
				height: var(--umb-header-logo-height, 30px);
			}

			#logo {
				display: var(--umb-logo-display, block);
				> img {
					width: var(--umb-logo-width, auto);
					height: var(--umb-logo-height, 50px);
				}
			}

			#modal {
				padding: var(--uui-size-space-6);
				display: flex;
				flex-direction: column;
				align-items: center;
				gap: var(--uui-size-space-3);
				min-width: var(--uui-size-100);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-logo': UmbBackofficeHeaderLogoElement;
	}
}
