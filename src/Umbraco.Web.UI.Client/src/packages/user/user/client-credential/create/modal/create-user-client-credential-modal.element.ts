import type { UmbCreateUserClientCredentialRequestArgs } from '../../repository/index.js';
import { UmbUserClientCredentialRepository } from '../../repository/index.js';
import type {
	UmbCreateUserClientCredentialModalData,
	UmbCreateUserClientCredentialModalValue,
} from './create-user-client-credential-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

const elementName = 'umb-create-user-client-credential-modal';
@customElement(elementName)
export class UmbCreateUserModalElement extends UmbModalBaseElement<
	UmbCreateUserClientCredentialModalData,
	UmbCreateUserClientCredentialModalValue
> {
	@query('#CreateUserClientCredentialForm')
	_form?: HTMLFormElement;

	@query('#unique')
	_inputUniqueElement?: UUIInputElement;

	#userClientCredentialRepository = new UmbUserClientCredentialRepository(this);

	#uniquePrefix = 'umbraco-back-office-';

	protected override firstUpdated(): void {
		// For some reason the pattern attribute is not working with this specific regex. It complains about the regex is invalid.
		// TODO: investigate why this is happening.
		this._inputUniqueElement?.addValidator(
			'patternMismatch',
			() => 'Only alphanumeric characters and hyphens are allowed',
			() => {
				const value = (this._inputUniqueElement?.value as string) || '';
				// eslint-disable-next-line no-useless-escape
				return !new RegExp(/^[a-zA-Z0-9\-]+$/).test(value);
			},
		);
	}

	async #onSubmit(e: SubmitEvent) {
		e.preventDefault();

		if (this.data?.user?.unique === undefined) throw new Error('User unique is required');

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		const unique = formData.get('unique') as string;
		const secret = formData.get('secret') as string;

		const payload: UmbCreateUserClientCredentialRequestArgs = {
			user: { unique: this.data.user.unique },
			client: { unique, secret },
		};

		// TODO: figure out when to use email or username
		const { data } = await this.#userClientCredentialRepository.requestCreate(payload);

		if (data) {
			this.updateValue({ client: { unique: data.unique, secret } });
			this._submitModal();
		}
	}

	override render() {
		return html`<uui-dialog-layout headline="Create client credential">
			${this.#renderForm()}
			<uui-button @click=${this._rejectModal} slot="actions" label="Cancel" look="secondary"></uui-button>
			<uui-button
				form="CreateUserClientCredentialForm"
				slot="actions"
				type="submit"
				label="Create"
				look="primary"
				color="positive"></uui-button>
		</uui-dialog-layout>`;
	}

	#renderForm() {
		return html` <uui-form>
			<form id="CreateUserClientCredentialForm" name="form" @submit="${this.#onSubmit}">
				<uui-form-layout-item>
					<uui-label id="uniqueLabel" slot="label" for="unique" required>Id</uui-label>
					<uui-input id="unique" label="unique" type="text" name="unique" required>
						<div class="prepend" slot="prepend">${this.#uniquePrefix}</div>
					</uui-input>
				</uui-form-layout-item>

				<uui-form-layout-item>
					<div slot="description">The secret cannot be retrieved again.</div>
					<uui-label id="secretLabel" slot="label" for="secret" required>Secret</uui-label>
					<uui-input-password id="secret" label="secret" name="secret" required></uui-input-password>
				</uui-form-layout-item>
			</form>
		</uui-form>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-input,
			uui-input-password {
				width: 580px;
			}

			.prepend {
				user-select: none;
				height: 100%;
				padding: 0 var(--uui-size-3);
				border-right: 1px solid var(--uui-input-border-color, var(--uui-color-border));
				background: #f3f3f3;
				color: grey;
				display: flex;
				justify-content: center;
				align-items: center;
				white-space: nowrap;
			}
		`,
	];
}

export { UmbCreateUserModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCreateUserModalElement;
	}
}
