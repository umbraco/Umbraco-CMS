import type { UmbManifestViewerModalData, UmbManifestViewerModalValue } from './manifest-viewer-modal.token.js';
import { css, html, customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-manifest-viewer-modal')
export class UmbManifestViewerModalElement extends UmbModalBaseElement<
	UmbManifestViewerModalData,
	UmbManifestViewerModalValue
> {
	override render() {
		console.log('data', this.data);
		return html`
			<umb-body-layout headline="${this.localize.term('general_manifest')}" main-no-padding>
				${this.data
					? html`<umb-code-block language="json" copy style="height:100%; border: none;"
							>${JSON.stringify(this.data, null, 2)}</umb-code-block
						>`
					: nothing}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [css``];
}

export default UmbManifestViewerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-manifest-viewer-modal': UmbManifestViewerModalElement;
	}
}
