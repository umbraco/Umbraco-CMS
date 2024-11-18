import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-example-modal-step1')
export class UmbExampleModalStep1 extends UmbModalBaseElement {
	override render() {
		return html` <div>example modal step1</div> `;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbExampleModalStep1;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-modal-step1': UmbExampleModalStep1;
	}
}
