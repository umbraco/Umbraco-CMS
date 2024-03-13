import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import type { UmbDocumentScheduleModalData, UmbDocumentScheduleModalValue } from './document-schedule-modal.token.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

import '../shared/document-variant-language-picker.element.js';

@customElement('umb-document-schedule-modal')
export class UmbDocumentScheduleModalElement extends UmbModalBaseElement<
	UmbDocumentScheduleModalData,
	UmbDocumentScheduleModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);
	#schedule?: ScheduleRequestModel;

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		this._options =
			this.data?.options.filter(
				(option) => option.variant && option.variant.state !== UmbDocumentVariantState.NOT_CREATED,
			) ?? [];

		let selected = this.value?.selection ?? [];

		// Filter selection based on options:
		selected = selected.filter((s) => this._options.some((o) => o.unique === s));

		this.#selectionManager.setSelection(selected);

		// Additionally select mandatory languages:
		this._options.forEach((variant) => {
			if (variant.language?.isMandatory) {
				this.#selectionManager.select(variant.unique);
			}
		});
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection(), schedule: this.#schedule };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`<umb-body-layout headline=${this.localize.term('general_scheduledPublishing')}>
			<p id="subtitle">
				<umb-localize key="content_languagesToSchedule">Which languages would you like to schedule?</umb-localize>
			</p>
			<umb-document-variant-language-picker
				.selectionManager=${this.#selectionManager}
				.variantLanguageOptions=${this._options}></umb-document-variant-language-picker>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('buttons_schedulePublish')}"
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 400px;
				max-width: 90vw;
			}
		`,
	];
}

export default UmbDocumentScheduleModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-schedule-modal': UmbDocumentScheduleModalElement;
	}
}
