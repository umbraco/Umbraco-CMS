import { html, customElement, query, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCreateDictionaryModalData, UmbCreateDictionaryModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-create-dictionary-modal')
export class UmbCreateDictionaryModalElement extends UmbModalBaseElement<
	UmbCreateDictionaryModalData,
	UmbCreateDictionaryModalValue
> {
	@query('#form')
	private _form!: HTMLFormElement;

	#parentName?: string;

	connectedCallback() {
		super.connectedCallback();

		if (this.data?.parentName) {
			this.observe(this.data.parentName, (value) => (this.#parentName = value));
		}
	}

	#handleCancel() {
		this.modalContext?.reject();
	}

	#submitForm() {
		this._form?.requestSubmit();
	}

	async #handleSubmit(e: SubmitEvent) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form || !form.checkValidity()) return;

		const formData = new FormData(form);
		const name = formData.get('name') as string;

		this.modalContext?.submit({
			name,
			parentId: this.data?.parentId ?? null,
		});
	}

	render() {
		return html` <umb-body-layout headline="Create">
			${when(this.#parentName, () => html`<p>Create a dictionary item under <b>${this.#parentName}</b></p>`)}
			<uui-form>
				<form id="form" name="form" @submit=${this.#handleSubmit}>
					<uui-form-layout-item>
						<uui-label for="nameinput" slot="label" required>Name</uui-label>
						<div>
							<uui-input
								type="text"
								id="nameinput"
								name="name"
								label="name"
								required
								required-message="Name is required"></uui-input>
						</div>
					</uui-form-layout-item>
				</form>
			</uui-form>
			<uui-button slot="actions" type="button" label="Close" @click=${this.#handleCancel}></uui-button>
			<uui-button slot="actions" type="button" label="Create" look="primary" @click=${this.#submitForm}></uui-button>
		</umb-body-layout>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbCreateDictionaryModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-dictionary-modal': UmbCreateDictionaryModalElement;
	}
}
