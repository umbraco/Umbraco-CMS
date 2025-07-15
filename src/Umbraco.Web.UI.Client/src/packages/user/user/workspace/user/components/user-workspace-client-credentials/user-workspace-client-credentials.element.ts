import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import type {
	UmbDeleteUserClientCredentialRequestArgs,
	UmbUserClientCredentialModel,
} from '../../../../client-credential/index.js';
import { UmbUserClientCredentialRepository } from '../../../../client-credential/index.js';
import { UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL } from '../../../../client-credential/create/modal/create-user-client-credential-modal.token.js';
import { UmbUserKind } from '../../../../utils/index.js';
import { html, customElement, state, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT, umbConfirmModal } from '@umbraco-cms/backoffice/modal';

const elementName = 'umb-user-workspace-client-credentials';
@customElement(elementName)
export class UmbUserWorkspaceClientCredentialsElement extends UmbLitElement {
	@state()
	private _userUnique?: string;

	@state()
	private _userKind?: string;

	@state()
	private _clientCredentials: UmbUserClientCredentialModel[] = [];

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;
	#modalManagerContext? = UMB_MODAL_MANAGER_CONTEXT.TYPE;
	#userClientCredentialRepository = new UmbUserClientCredentialRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;

			this.observe(this.#userWorkspaceContext?.kind, (kind) => (this._userKind = kind), 'umbUserKindObserver');

			this.observe(
				this.#userWorkspaceContext?.unique,
				async (unique) => {
					if (unique) {
						this.#onUserUniqueChange(unique);
					}
				},
				'umbUserUniqueObserver',
			);
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	#onUserUniqueChange(unique: string | undefined) {
		if (unique && this._userUnique !== unique) {
			this._userUnique = unique;
			this.#loadClientCredentials();
		}

		if (!unique) {
			this._userUnique = undefined;
			this._clientCredentials = [];
		}
	}

	async #loadClientCredentials() {
		if (!this._userUnique) throw new Error('User unique not available');

		const { data } = await this.#userClientCredentialRepository.requestClientCredentials({
			user: { unique: this._userUnique },
		});

		this._clientCredentials = data ?? [];
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

		modalContext.onSubmit().then(() => this.#loadClientCredentials());
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

		const { error } = await this.#userClientCredentialRepository.requestDelete(payload);

		if (!error) {
			this.#loadClientCredentials();
		}
	}

	override render() {
		if (this._userKind !== UmbUserKind.API) return nothing;

		return html`<uui-box>
			<div slot="headline">Client Credentials</div>
			<uui-ref-list>${this._clientCredentials.map((client) => html` ${this.#renderItem(client)} `)}</uui-ref-list>
			<uui-button
				id="add-button"
				look="placeholder"
				label=${this.localize.term('general_add')}
				@click=${this.#onAdd}></uui-button>
		</uui-box>`;
	}

	#renderItem(client: UmbUserClientCredentialModel) {
		return html`
			<uui-ref-node name=${client.unique} readonly>
				<uui-icon slot="icon" name="icon-key"></uui-icon>
				<uui-button
					slot="actions"
					@click=${(event: Event) => this.#onDelete(event, client)}
					label="Delete ${client.unique}"
					compact
					><uui-icon name="icon-trash" look="danger"></uui-icon
				></uui-button>
			</uui-ref-node>
		`;
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
