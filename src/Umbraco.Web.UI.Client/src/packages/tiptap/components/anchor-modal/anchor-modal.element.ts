import type { UmbTiptapAnchorModalData, UmbTiptapAnchorModalValue } from './anchor-modal.token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-tiptap-anchor-modal')
export class UmbTiptapAnchorModalElement extends UmbModalBaseElement<
	UmbTiptapAnchorModalData,
	UmbTiptapAnchorModalValue
> {
	async #onSubmit(event: SubmitEvent) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);
		const name = formData.get('name') as string;

		this.value = name;
		this._submitModal();
	}

	override render() {
		const label = this.localize.term('tiptap_anchor_input');
		return html`
			<uui-dialog-layout>
				<uui-form>
					<form id="form" @submit=${this.#onSubmit}>
						<uui-form-layout-item>
							<uui-label for="name" slot="label" required>${label}</uui-label>
							<uui-input
								type="text"
								required
								id="name"
								name="name"
								label=${label}
								.value=${this.data?.id || ''}
								${umbFocus()}></uui-input>
						</uui-form-layout-item>
					</form>
				</uui-form>
				<uui-button
					slot="actions"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click=${this._rejectModal}></uui-button>
				<uui-button
					type="submit"
					slot="actions"
					form="form"
					color="positive"
					look="primary"
					label=${this.localize.term('general_submit')}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
		css`
			:host {
				--umb-body-layout-color-background: var(--uui-color-surface);
			}

			uui-dialog-layout {
				width: var(--uui-size-100);
			}

			uui-input {
				width: 100%;
			}
		`,
	];
}

export { UmbTiptapAnchorModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-anchor-modal': UmbTiptapAnchorModalElement;
	}
}
