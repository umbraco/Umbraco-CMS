import { UmbSysinfoRepository } from '../repository/sysinfo.repository.js';
import type { UmbServerUpgradeCheck } from '../types.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-new-version')
export class UmbNewVersionElement extends UmbModalBaseElement {
	@state()
	private _serverUpgradeCheck: UmbServerUpgradeCheck | null = null;

	#sysinfoRepository = new UmbSysinfoRepository(this);

	override async connectedCallback() {
		super.connectedCallback();
		this._serverUpgradeCheck = await this.#sysinfoRepository.serverUpgradeCheck();
	}

	override render() {
		return html`
			<uui-dialog>
				<uui-dialog-layout headline=${this.localize.term('general_newVersionAvailable')}>
					${when(
						this._serverUpgradeCheck === null,
						() => html`<uui-loader-bar></uui-loader-bar>`,
						() => html` <div>${this._serverUpgradeCheck!.comment}</div> `,
					)}

					<uui-button
						@click=${this._submitModal}
						slot="actions"
						look="secondary"
						label=${this.localize.term('general_close')}></uui-button>

					${this._serverUpgradeCheck?.url
						? html` <uui-button
								.href=${this._serverUpgradeCheck.url}
								target="_blank"
								slot="actions"
								look="primary"
								color="positive"
								label=${this.localize.term('general_readMore')}>
								<umb-localize key="general_readMore">Read more</umb-localize>
								<umb-icon slot="extra" name="icon-out"></umb-icon>
							</uui-button>`
						: ''}
				</uui-dialog-layout>
			</uui-dialog>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			umb-icon {
				margin-left: var(--uui-size-2);
			}
		`,
	];
}

export default UmbNewVersionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-new-version': UmbNewVersionElement;
	}
}
