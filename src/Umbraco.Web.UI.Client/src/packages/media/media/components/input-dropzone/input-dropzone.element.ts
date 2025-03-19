import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-input-dropzone')
export class UmbInputDropzoneElement extends UmbLitElement {
	override render() {
		return html`Dropzone`;
	}

	static override readonly styles = [UmbTextStyles, css``];
}

export { UmbInputDropzoneElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-dropzone': UmbInputDropzoneElement;
	}
}
