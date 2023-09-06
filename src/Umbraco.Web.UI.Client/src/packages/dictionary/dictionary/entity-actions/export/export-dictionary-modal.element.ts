import { html, customElement, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbExportDictionaryModalData, UmbExportDictionaryModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-export-dictionary-modal')
export class UmbExportDictionaryModalElement extends UmbModalBaseElement<
	UmbExportDictionaryModalData,
	UmbExportDictionaryModalResult
> {
	@query('#form')
	private _form!: HTMLFormElement;

	#handleClose() {
		this.modalContext?.reject();
	}

	#submitForm() {
		this._form?.requestSubmit();
	}

	async #handleSubmit(e: SubmitEvent) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const formData = new FormData(form);

		this.modalContext?.submit({ includeChildren: (formData.get('includeDescendants') as string) === 'on' });
	}

	render() {
		return html` <umb-body-layout headline="Export">
			<uui-form>
				<form id="form" name="form" @submit=${this.#handleSubmit}>
					<uui-form-layout-item>
						<uui-label for="includeDescendants" slot="label">Include descendants</uui-label>
						<uui-toggle id="includeDescendants" name="includeDescendants"></uui-toggle>
					</uui-form-layout-item>
				</form>
			</uui-form>
			<uui-button slot="actions" type="button" label="Cancel" look="secondary" @click=${this.#handleClose}></uui-button>
			<uui-button slot="actions" type="button" label="Export" look="primary" @click=${this.#submitForm}></uui-button>
		</umb-body-layout>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbExportDictionaryModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-export-dictionary-modal': UmbExportDictionaryModalElement;
	}
}
