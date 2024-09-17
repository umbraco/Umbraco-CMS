import { UmbModalBaseElement } from '../../component/modal-base.element.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-discard-changes-modal')
export class UmbDiscardChangesModalElement extends UmbModalBaseElement {
	override render() {
		return html` <uui-dialog-layout class="uui-text" headline="Discard changes?"> Hello Hello </uui-dialog-layout> `;
	}

	static override styles = [UmbTextStyles];
}

export { UmbDiscardChangesModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-discard-changes-modal': UmbDiscardChangesModalElement;
	}
}
