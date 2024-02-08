import { UmbDocumentPublicAccessRepository } from '../repository/public-access.repository.js';
import { html, customElement, state, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import {
	UmbDocumentDetailRepository,
	type UmbInputDocumentElement,
	type UmbPublicAccessModalData,
	type UmbPublicAccessModalValue,
} from '@umbraco-cms/backoffice/document';
import type { UUIRadioEvent } from '@umbraco-cms/backoffice/external/uui';
import type { PublicAccessRequestModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbInputMemberTypeElement } from '@umbraco-cms/backoffice/member-type';
import type { UmbInputMemberElement } from '@umbraco-cms/backoffice/member';

@customElement('umb-public-access-modal')
export class UmbPublicAccessModalElement extends UmbModalBaseElement<
	UmbPublicAccessModalData,
	UmbPublicAccessModalValue
> {
	#publicAccessRepository = new UmbDocumentPublicAccessRepository(this);
	#unique?: string;
	#isNew: boolean = true;

	@state()
	private _documentName = '';

	@state()
	private _specific?: boolean;

	@state()
	private _startPage = true;

	@state()
	private _selectedIds: Array<string> = [];

	@state()
	private _loginPageId?: string;

	@state()
	private _errorPageId?: string;

	// Init

	firstUpdated() {
		this.#unique = this.data?.unique;
		this.#getDocumentName();
		this.#getPublicAccessModel();
	}

	async #getDocumentName() {
		if (!this.#unique) return;
		// Should this be done here or in the action file?
		const { data } = await new UmbDocumentDetailRepository(this).requestByUnique(this.#unique);
		if (!data) return;
		//TODO How do we ensure we get the correct variant?
		this._documentName = data.variants[0]?.name;
	}

	async #getPublicAccessModel() {
		if (!this.#unique) return;
		//const { data } = (await this.#publicAccessRepository.read(this.#unique));
		// TODO Currently returning "void". Remove mock data when API is ready. Will it be Response or Request model?
		const data: any = undefined;
		/*const data: PublicAccessResponseModel = {
			members: [{ name: 'Agent', id: '007' }],
			groups: [],
			loginPageId: '123',
			errorPageId: '456',
		};*/

		if (!data) return;
		this.#isNew = false;
		this._startPage = false;

		// Specific or Groups
		this._specific = data.members.length > 0 ? true : false;

		//SelectedIds
		if (data.members.length > 0) {
			this._selectedIds = data.members.map((m: any) => m.id);
		} else if (data.groups.length > 0) {
			this._selectedIds = data.groups.map((g: any) => g.id);
		}

		this._loginPageId = data.loginPageId;
		this._errorPageId = data.errorPageId;
	}

	// Modal events

	#handleNext() {
		this._startPage = false;
	}

	async #handleSave() {
		if (!this._loginPageId || !this._errorPageId || !this.#unique) return;

		const groups = this._specific ? [] : this._selectedIds;
		const members = this._specific ? this._selectedIds : [];

		const requestBody: PublicAccessRequestModel = {
			memberGroupNames: groups,
			memberUserNames: members,
			loginDocument: { id: this._loginPageId },
			errorDocument: { id: this._errorPageId },
		};

		if (this.#isNew) {
			this.#publicAccessRepository.create(this.#unique, requestBody);
		} else {
			this.#publicAccessRepository.update(this.#unique, requestBody);
		}

		this.modalContext?.submit();
	}

	#handleDelete() {
		if (!this.#unique) return;
		this.#publicAccessRepository.delete(this.#unique);
		this.modalContext?.submit();
	}

	#handleCancel() {
		this.modalContext?.reject();
	}

	// Change Events

	#onChangeLoginPage(e: CustomEvent) {
		this._loginPageId = (e.target as UmbInputDocumentElement).selectedIds[0];
	}

	#onChangeErrorPage(e: CustomEvent) {
		this._errorPageId = (e.target as UmbInputDocumentElement).selectedIds[0];
	}

	#onChangeGroup(e: CustomEvent) {
		this._selectedIds = (e.target as UmbInputMemberTypeElement).selectedIds;
	}

	#onChangeMember(e: CustomEvent) {
		this._selectedIds = (e.target as UmbInputMemberElement).selectedIds;
	}

	// Renders

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_protect')}>
				<uui-box>${this._startPage ? this.renderSelectGroup() : this.renderEditPage()}</uui-box> ${this.renderActions()}
			</umb-body-layout>
		`;
	}

	// First page when no Restricting Public Access is set.
	renderSelectGroup() {
		return html`<umb-localize key="publicAccess_paHowWould" .args=${[this._documentName]}>
				Choose how you want to restrict public access to the page '${this._documentName}'.
			</umb-localize>
			<uui-radio-group
				@change=${(e: UUIRadioEvent) =>
					e.target.value === 'members' ? (this._specific = true) : (this._specific = false)}>
				<uui-radio label=${this.localize.term('publicAccess_paMembers')} value="members">
					<strong>${this.localize.term('publicAccess_paMembers')}</strong><br />
					${this.localize.term('publicAccess_paMembersHelp')}
				</uui-radio>
				<uui-radio label=${this.localize.term('publicAccess_paGroups')} value="groups">
					<strong>${this.localize.term('publicAccess_paGroups')}</strong><br />
					${this.localize.term('publicAccess_paGroupsHelp')}
				</uui-radio>
			</uui-radio-group>`;
	}

	// Second page when editing Restricting Public Access
	renderEditPage() {
		return html`${this.renderMemberType()}
			<p>
				<umb-localize key="publicAccess_paSelectPages">
					Select the pages that contain login form and error messages
				</umb-localize>
			</p>
			<div class="select-item">
				<strong><umb-localize key="publicAccess_paLoginPage">Login Page</umb-localize></strong>
				<small>
					<umb-localize key="publicAccess_paLoginPageHelp"> Choose the page that contains the login form </umb-localize>
				</small>
				<umb-input-document max="1" @change=${this.#onChangeLoginPage}></umb-input-document>
			</div>
			<br />
			<div class="select-item">
				<strong><umb-localize key="publicAccess_paErrorPage">Error Page</umb-localize></strong>
				<small>
					<umb-localize key="publicAccess_paErrorPageHelp">
						Used when people are logged on, but do not have access
					</umb-localize>
				</small>
				<umb-input-document max="1" @change=${this.#onChangeErrorPage}></umb-input-document>
			</div>`;
	}

	renderMemberType() {
		return this._specific
			? html`<umb-localize key="publicAccess_paSelectMembers" .args=${[this._documentName]}>
						Select the members who have access to the page <strong>${this._documentName}</strong>
					</umb-localize>
					<umb-input-member .selectedIds=${this._selectedIds} @change=${this.#onChangeMember}></umb-input-member>`
			: html`<umb-localize key="publicAccess_paSelectGroups" .args=${[this._documentName]}>
						Select the groups who have access to the page <strong>${this._documentName}</strong>
					</umb-localize>
					<umb-input-member-type
						.selectedIds=${this._selectedIds}
						@change=${this.#onChangeGroup}></umb-input-member-type>`;
	}

	// Action buttons
	renderActions() {
		// Check for Save or Next button
		const confirm = !this._startPage
			? html`<uui-button
					slot="actions"
					id="save"
					look="primary"
					color="positive"
					label=${this.localize.term('buttons_save')}
					?disabled=${!this._loginPageId || !this._errorPageId || this._selectedIds.length === 0}
					@click="${this.#handleSave}"></uui-button>`
			: html`<uui-button
					slot="actions"
					id="save"
					look="primary"
					label=${this.localize.term('general_next')}
					?disabled=${this._specific === undefined}
					@click="${this.#handleNext}"></uui-button>`;
		// Check for Remove button
		const remove = !this.#isNew
			? html`<uui-button
					slot="actions"
					id="save"
					look="primary"
					color="warning"
					@click="${this.#handleDelete}"
					label=${this.localize.term('publicAccess_paRemoveProtection')}></uui-button>`
			: nothing;
		//Render the buttons
		return html` <uui-button
				slot="actions"
				id="cancel"
				label=${this.localize.term('buttons_confirmActionCancel')}
				@click="${this.#handleCancel}"></uui-button
			>${remove}${confirm}`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-box,
			uui-radio-group {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-4);
			}
			uui-radio-group {
				margin-top: var(--uui-size-4);
			}

			p {
				margin: var(--uui-size-6) 0 var(--uui-size-2);
			}
			small {
				display: block;
			}
		`,
	];
}

export default UmbPublicAccessModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-public-access-modal': UmbPublicAccessModalElement;
	}
}
