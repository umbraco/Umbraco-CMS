import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-backoffice-header-logo')
export class UmbBackofficeHeaderLogoElement extends UmbLitElement {
	@state()
	private _version?: string;

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
		});
	}

	override render() {
		return html`
			<uui-button id="logo" look="primary" label="Umbraco" compact popovertarget="logo-popover">
				<img src="/umbraco/backoffice/assets/umbraco_logomark_white.svg" alt="Umbraco" />
			</uui-button>
			<uui-popover-container id="logo-popover" placement="bottom-start">
				<umb-popover-layout>
					<div id="modal">
						<img src="/umbraco/backoffice/assets/umbraco_logo_blue.svg" alt="Umbraco" loading="lazy" />
						<span>${this._version}</span>
						<a href="https://umbraco.com" target="_blank" rel="noopener">Umbraco.com</a>
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#logo {
				display: var(--umb-header-logo-display, inline);
				--uui-button-padding-top-factor: 1;
				--uui-button-padding-bottom-factor: 0.5;
				margin-right: var(--uui-size-space-2);
				--uui-button-background-color: transparent;
			}

			#logo > img {
				height: var(--uui-size-10);
				width: var(--uui-size-10);
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
