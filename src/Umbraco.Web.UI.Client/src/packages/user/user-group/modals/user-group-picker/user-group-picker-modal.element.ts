import { UmbUserGroupCollectionRepository } from '../../collection/repository/index.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { html, customElement, state, ifDefined, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/user-group';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbUserGroupRefElement } from '../../components/user-group-ref';

@customElement('umb-user-group-picker-modal')
export class UmbUserGroupPickerModalElement extends UmbModalBaseElement<
	(typeof UMB_USER_GROUP_PICKER_MODAL)['DATA'],
	(typeof UMB_USER_GROUP_PICKER_MODAL)['VALUE']
> {
	@state()
	private _userGroups: Array<UmbUserGroupDetailModel> = [];

	#selectionManager = new UmbSelectionManager(this);
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	override connectedCallback(): void {
		super.connectedCallback();

		// TODO: in theory this config could change during the lifetime of the modal, so we could observe it
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
		this.observe(this.#selectionManager.selection, (selection) => this.updateValue({ selection }), 'selectionObserver');
	}

	protected override firstUpdated(): void {
		this.#observeUserGroups();
	}

	async #observeUserGroups() {
		const { error, asObservable } = await this.#userGroupCollectionRepository.requestCollection();
		if (error) return;
		this.observe(asObservable(), (items) => (this._userGroups = items), 'umbUserGroupsObserver');
	}

	#onSelected(event: UUIMenuItemEvent, item: UmbUserGroupDetailModel) {
		if (!item.unique) throw new Error('User group unique is required');
		event.stopPropagation();
		this.#selectionManager.select(item.unique);
		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(item.unique));
	}

	#onDeselected(event: UUIMenuItemEvent, item: UmbUserGroupDetailModel) {
		if (!item.unique) throw new Error('User group unique is required');
		event.stopPropagation();
		this.#selectionManager.deselect(item.unique);
		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(item.unique));
	}

	#onSubmit() {
		this.updateValue({ selection: this.#selectionManager.getSelection() });
		this._submitModal();
	}

	//TODO: The details should probably be defined in the umb-user-group-ref it self and subsequently imported instead of being defined here.

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('user_selectUserGroup', false)}>
				<uui-box>
					${this._userGroups.map((userGroup) => {
						const isSelected = this.#selectionManager.isSelected(userGroup.unique);
						return html`
							<umb-user-group-ref
								.name=${userGroup.name}
								select-only
								selectable
								@selected=${(event: UUIMenuItemEvent) => this.#onSelected(event, userGroup)}
								@deselected=${(event: UUIMenuItemEvent) => this.#onDeselected(event, userGroup)}
								?selected=${isSelected}
								.icon=${userGroup.icon || ''}
								.userPermissionAliases=${userGroup.sections}>
								<uui-icon .name=${userGroup.icon || undefined} slot="icon"></uui-icon>
								<div slot="detail" id="details">
									<div>
										<strong>Sections:</strong> ${userGroup.sections.length
											? userGroup.sections.map((section) => section.split('.').pop()).join(', ')
											: 'No sections selected'}
									</div>
									<div>
										<strong>Media Start Node:</strong> ${userGroup.mediaStartNode
											? userGroup.mediaStartNode.unique
											: 'No media start node selected'}
									</div>
									<div>
										<strong>Content Start Node:</strong> ${userGroup.documentStartNode
											? userGroup.documentStartNode.unique
											: 'No content start node selected'}
									</div>
								</div>
							</umb-user-group-ref>
						`;
					})}
				</uui-box>
				<div slot="actions">
					<uui-button label="Cancel" @click=${this._rejectModal}></uui-button>
					<uui-button label="Confirm" look="primary" color="positive" @click=${this.#onSubmit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			#details {
				color: var(--uui-color-text-alt);
				margin-top: var(--uui-size-space-2);
			}

			#details > div {
				margin-bottom: var(--uui-size-space-1);
			}
		`,
	];
}

export default UmbUserGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-picker-modal': UmbUserGroupPickerModalElement;
	}
}
