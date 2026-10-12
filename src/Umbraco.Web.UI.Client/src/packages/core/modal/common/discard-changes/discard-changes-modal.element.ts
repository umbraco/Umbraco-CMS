import { UmbModalBaseElement } from '../../component/modal-base.element.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-discard-changes-modal')
export class UmbDiscardChangesModalElement extends UmbModalBaseElement {
	override render() {
		return html`
			<uui-dialog-layout
				data-mark="discard-changes-modal"
				class="uui-text"
				headline=${this.localize.term('prompt_unsavedChanges')}>
				<umb-localize key="prompt_unsavedChangesWarning"></umb-localize>
				<uui-button
					slot="actions"
					id="cancel"
					data-mark="action:stay"
					label=${this.localize.term('prompt_stay')}
					@click=${this._rejectModal}></uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					data-mark="action:discard-changes"
					color="danger"
					look="primary"
					label=${this.localize.term('prompt_discardChanges')}
					@click=${this._submitModal}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [UmbTextStyles];
}

export { UmbDiscardChangesModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-discard-changes-modal': UmbDiscardChangesModalElement;
	}
}
