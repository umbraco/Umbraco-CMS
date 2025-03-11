import type { UmbServerUpgradeCheck } from '../types.js';
import type { UmbNewVersionModalData } from '../modals/new-version-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-new-version')
export class UmbNewVersionElement extends UmbModalBaseElement<UmbNewVersionModalData> {
	@state()
	private _serverUpgradeCheck: UmbServerUpgradeCheck | null = null;

	override render() {
		return html`
			<uui-dialog>
				<uui-dialog-layout headline=${this.localize.term('general_newVersionAvailable')}>
					${this.data?.comment}

					<uui-button
						@click=${this._submitModal}
						slot="actions"
						look="secondary"
						label=${this.localize.term('general_close')}></uui-button>

					${this.data?.downloadUrl
						? html` <uui-button
								.href=${this.data.downloadUrl}
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
