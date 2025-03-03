import type {
	UmbExamineFieldsSettingsModalData,
	UmbExamineFieldsSettingsModalValue,
	UmbExamineFieldSettingsType,
} from './examine-fields-settings-modal.token.js';
import { html, css, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-examine-fields-settings-modal')
export class UmbExamineFieldsSettingsModalElement extends UmbModalBaseElement<
	UmbExamineFieldsSettingsModalData,
	UmbExamineFieldsSettingsModalValue
> {
	override render() {
		return html`<umb-body-layout headline=${this.localize.term('examineManagement_fields')}>
			<uui-scroll-container id="field-settings"> ${this.#renderFields()} </uui-scroll-container>
			<div slot="actions">
				<uui-button
					look="primary"
					label=${this.localize.term('general_close')}
					@click="${this._submitModal}"></uui-button>
			</div>
		</umb-body-layout>`;
	}

	#setExposed(fieldSetting: UmbExamineFieldSettingsType) {
		const newField: UmbExamineFieldSettingsType = { ...fieldSetting, exposed: !fieldSetting.exposed };

		const updatedFields =
			this.modalContext?.getValue().fields.map((field) => {
				if (field.name === fieldSetting.name) return newField;
				else return field;
			}) ?? [];

		this.modalContext?.updateValue({ fields: updatedFields });
	}

	#renderFields() {
		if (!this.value.fields.length) return;
		return html`<span>
			${Object.values(this.value.fields).map((field) => {
				return html`<uui-toggle
						name="${field.name}"
						label="${field.name}"
						.checked="${field.exposed}"
						@change="${() => this.#setExposed(field)}"></uui-toggle>
					<br />`;
			})}
		</span>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: relative;
			}

			uui-scroll-container {
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
				flex: 1;
			}
		`,
	];
}

export default UmbExamineFieldsSettingsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-examine-fields-settings-modal': UmbExamineFieldsSettingsModalElement;
	}
}
