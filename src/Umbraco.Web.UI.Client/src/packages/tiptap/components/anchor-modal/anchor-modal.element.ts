import type { UmbAnchorModalData, UmbAnchorModalValue } from './anchor-modal.token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-anchor-modal')
export class UmbAnchorModalElement extends UmbModalBaseElement<UmbAnchorModalData, UmbAnchorModalValue> {
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
		return html`
			<umb-body-layout>
				<uui-form>
					<form id="form" @submit=${this.#onSubmit}>
						<uui-form-layout-item>
							<uui-label for="name" slot="label" required>Enter an anchor ID</uui-label>
							<uui-input
								type="text"
								id="name"
								name="name"
								label="Enter an anchor ID"
								.value=${this.data?.id || ''}
								required
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
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				--umb-body-layout-color-background: var(--uui-color-surface);
			}
		`,
	];
}

export { UmbAnchorModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-anchor-modal': UmbAnchorModalElement;
	}
}
