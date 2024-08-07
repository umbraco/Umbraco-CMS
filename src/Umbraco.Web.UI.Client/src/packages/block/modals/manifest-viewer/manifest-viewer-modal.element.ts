import type { UmbManifestViewerModalData, UmbManifestViewerModalValue } from './manifest-viewer-modal.token.js';
import { css, html, customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

// JSON parser for the manifest viewer modal
// Enabling us to view JS code, but it is not optimal, but currently better than nothing [NL]
// Ideally we should have a JS code stringify that can print the manifest as JS. [NL]
function JsonParser(key: string, value: any) {
	if (typeof value === 'function' && value !== null && value.toString) {
		return Function.prototype.toString.call(value);
	}
	return value;
}

@customElement('umb-manifest-viewer-modal')
export class UmbManifestViewerModalElement extends UmbModalBaseElement<
	UmbManifestViewerModalData,
	UmbManifestViewerModalValue
> {
	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('general_manifest')}" main-no-padding>
				${this.data
					? html`<umb-code-block language="json" copy style="height:100%; border: none;"
							>${JSON.stringify(this.data, JsonParser, 2)}</umb-code-block
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
