import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import type {
	UmbDeleteUserClientCredentialRequestArgs,
	UmbUserClientCredentialModel,
} from '../../../client-credential/index.js';
import { UmbUserClientCredentialRepository } from '../../../client-credential/index.js';
import { UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL } from '../../../client-credential/create/modal/create-user-client-credential-modal.token.js';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT, umbConfirmModal } from '@umbraco-cms/backoffice/modal';

const elementName = 'umb-user-workspace-client-credentials';
@customElement(elementName)
export class UmbUserWorkspaceClientCredentialsElement extends UmbLitElement {
	@state()
	private _userUnique?: string;

	@state()
	private _clientCredentials: UmbUserClientCredentialModel[] = [];

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;
	#modalManagerContext? = UMB_MODAL_MANAGER_CONTEXT.TYPE;
	#userClientCredentialRepository = new UmbUserClientCredentialRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(
				this.#userWorkspaceContext.unique,
				async (unique) => this.#onUserUniqueChange(unique),
				'umbUserUniqueObserver',
			);
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async #onUserUniqueChange(unique: string | undefined) {
		if (unique && this._userUnique !== unique) {
			const { data } = await this.#userClientCredentialRepository.requestClientCredentials({ user: { unique } });
			if (data) {
				this._clientCredentials = data;
			}
		}

		this._userUnique = unique;
	}

	#onAdd(event: Event) {
		event.stopPropagation();
		if (!this.#modalManagerContext) throw new Error('Modal Manager Context not available');
		if (!this._userUnique) throw new Error('User unique not available');

		const modalContext = this.#modalManagerContext.open(this, UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL, {
			data: {
				user: {
					unique: this._userUnique,
				},
			},
		});

		modalContext.onSubmit().then((result) => {
			console.log('submit', result);
		});
	}

	async #onDelete(event: Event, client: UmbUserClientCredentialModel) {
		event.stopPropagation();
		if (!this._userUnique) throw new Error('User unique not available');

		await umbConfirmModal(this, {
			headline: `Delete ${client.unique}`,
			content: `Are you sure you want to delete ${client.unique}?`,
			confirmLabel: 'Delete',
			color: 'danger',
		});

		const payload: UmbDeleteUserClientCredentialRequestArgs = {
			user: { unique: this._userUnique },
			client: { unique: client.unique },
		};

		this.#userClientCredentialRepository.requestDelete(payload);
	}

	override render() {
		return html`<uui-box>
			<div slot="headline">Client Credentials</div>
			${this._clientCredentials.map(
				(client) =>
					html`<div>
						${client.unique} <uui-button @click=${(event: Event) => this.#onDelete(event, client)}>Delete</uui-button>
					</div>`,
			)}
			<uui-button
				id="add-button"
				look="placeholder"
				label=${this.localize.term('general_add')}
				@click=${this.#onAdd}></uui-button>
		</uui-box>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			uui-input {
				width: 100%;
			}

			#add-button {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserWorkspaceClientCredentialsElement;
	}
}
