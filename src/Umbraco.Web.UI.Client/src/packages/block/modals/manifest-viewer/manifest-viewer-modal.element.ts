import type { UmbManifestViewerModalData, UmbManifestViewerModalValue } from './manifest-viewer-modal.token.js';
import { css, customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-manifest-viewer-modal')
export class UmbManifestViewerModalElement extends UmbModalBaseElement<
	UmbManifestViewerModalData,
	UmbManifestViewerModalValue
> {
	// Code adapted from https://stackoverflow.com/a/57668208/12787
	// Licensed under the permissions of the CC BY-SA 4.0 DEED
	#stringify(obj: any): string {
		let output = '{';
		for (const key in obj) {
			let value = obj[key];
			if (typeof value === 'function') {
				value = value.toString();
			} else if (value instanceof Array) {
				value = JSON.stringify(value);
			} else if (typeof value === 'object') {
				value = this.#stringify(value);
			} else {
				value = `"${value}"`;
			}
			output += `\n  ${key}: ${value},`;
		}
		return output + '\n}';
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('general_manifest')} main-no-padding>
				${this.data
					? html`<umb-code-block language="JSON" copy>${this.#stringify(this.data)}</umb-code-block>`
					: nothing}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			umb-code-block {
				border: none;
				height: 100%;
			}
		`,
	];
}

export default UmbManifestViewerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-manifest-viewer-modal': UmbManifestViewerModalElement;
	}
}
