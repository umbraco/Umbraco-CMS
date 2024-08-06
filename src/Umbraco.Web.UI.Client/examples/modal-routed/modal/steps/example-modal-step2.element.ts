import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-example-modal-step2')
export class UmbExampleModalStep2 extends UmbModalBaseElement {
	override render() {
		return html` <div>example modal step2</div> `;
	}

	static override styles = [UmbTextStyles];
}

export default UmbExampleModalStep2;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-modal-step2': UmbExampleModalStep2;
	}
}
