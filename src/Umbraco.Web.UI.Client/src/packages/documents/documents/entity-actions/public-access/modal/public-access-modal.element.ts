import { html, customElement, state, css, nothing, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbPublicAccessModalData, UmbPublicAccessModalValue } from '@umbraco-cms/backoffice/document';
import type { UUIRadioEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-public-access-modal')
export class UmbPublicAccessModalElement extends UmbModalBaseElement<
	UmbPublicAccessModalData,
	UmbPublicAccessModalValue
> {
	@state()
	private _specific?: boolean;

	@state()
	private _startPage = true;

	@state()
	private _selectedIds: Array<string> = [];

	// Init

	firstUpdated() {
		const data = this.data?.publicAccessModel;
		if (!data) return;
		this._startPage = false;

		// Specific or Group
		this._specific = data.members.length > 0 ? true : false;

		//SelectedIds members
		if (data.members.length > 0) {
			this._selectedIds = data.members.map((m) => m.id);
		} else if (data.groups.length > 0) {
			this._selectedIds = data.groups.map((g) => g.id);
		}
	}

	// Modal

	#handleNext() {
		this._startPage = false;
	}

	async #handleSave() {
		if (this.data?.publicAccessModel) {
			this.modalContext?.submit({ action: 'update', publicAccessModel: this.value });
		} else {
			this.modalContext?.submit({ action: 'save', publicAccessModel: this.value });
		}
	}

	#handleDelete() {
		this.modalContext?.submit({ action: 'delete', publicAccessModel: this.value });
	}

	#handleCancel() {
		this.modalContext?.reject();
	}

	// Events

	#onChangeLoginPage(e: CustomEvent) {
		console.log('e', e);
		this.value = { ...this.value, loginPageId: e.detail.value[0] };
	}

	#onChangeErrorPage(e: CustomEvent) {
		console.log('e', e);
		this.value = { ...this.value, errorPageId: e.detail.value[0] };
	}

	#onChangeGroup() {
		// TODO: Finish when umb-input-member-type is done
		console.log('onChangeGroup');
	}

	#onChangeMember() {
		// TODO: Finish when umb-input-member is done
		console.log('onChangeMember');
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
		return html`<umb-localize key="publicAccess_paHowWould" .args=${['NameOfDocument']}>
				Choose how you want to restrict public access to the page 'NameOfDocument'.
			</umb-localize>
			<uui-radio-group
				@change=${(e: UUIRadioEvent) =>
					e.target.value === 'members' ? (this._specific = true) : (this._specific = false)}>
				<uui-radio label="Specific members protection" value="members">Test</uui-radio>
				<uui-radio label="Group based protection" value="groups"></uui-radio>
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
			? html`<umb-localize key="publicAccess_paSelectMembers" .args=${['NameOfDocument']}>
						Select the members who have access to the page <strong>NameOfDocument</strong>
					</umb-localize>
					<umb-input-member .selectedIds=${this._selectedIds} @change=${this.#onChangeMember}></umb-input-member>`
			: html`<umb-localize key="publicAccess_paSelectGroups" .args=${['NameOfDocument']}>
						Select the groups who have access to the page <strong>NameOfDocument</strong>
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
					@click="${this.#handleSave}"></uui-button>`
			: html`<uui-button
					slot="actions"
					id="save"
					look="primary"
					label=${this.localize.term('general_next')}
					?disabled=${this._specific === undefined}
					@click="${this.#handleNext}"></uui-button>`;
		// Check for Remove button
		const remove = this.data?.publicAccessModel
			? html`<uui-button
					slot="actions"
					id="save"
					look="primary"
					color="warning"
					@click="${this.#handleDelete}"
					label=${this.localize.term('publicAccess_paRemoveProtection')}></uui-button>`
			: nothing;
		//Render buttons
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
			uui-radio-group {
				display: block;
			}

			.select-item {
				display: flex;
				flex-direction: column;
			}

			p {
				margin: var(--uui-size-6) 0 var(--uui-size-2);
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
