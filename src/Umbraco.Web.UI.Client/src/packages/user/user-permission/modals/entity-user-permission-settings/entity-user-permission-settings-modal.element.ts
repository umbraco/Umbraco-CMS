import { html, customElement, css, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';
import { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-entity-user-permission-settings-modal')
export class UmbEntityUserPermissionSettingsModalElement extends UmbModalBaseElement<
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue
> {
	set data(data: UmbEntityUserPermissionSettingsModalData | undefined) {
		super.data = data;
		this._entityType = data?.entityType;
		this._allowedPermissions = data?.allowedPermissions ?? [];
		this._headline = data?.headline ?? this._headline;
	}

	@state()
	_headline: string = 'Set permissions';

	@state()
	_entityType?: string;

	@state()
	_allowedPermissions: Array<string> = [];

	private _handleConfirm() {
		this._value = { allowedPermissions: this._allowedPermissions };
		this.modalContext?.submit();
	}

	#onSelectedUserPermission(event: UmbSelectionChangeEvent) {
		const target = event.target as any;
		this._allowedPermissions = target.selectedPermissions;
	}

	render() {
		return html`
			<umb-body-layout headline=${this._headline}>
				<uui-box>
					${this._entityType
						? html` <umb-entity-user-permission-settings-list
								.entityType=${this._entityType}
								.selectedPermissions=${this._allowedPermissions}
								@selection-change=${this.#onSelectedUserPermission}></umb-entity-user-permission-settings-list>`
						: nothing}
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}">Cancel</uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label="Confirm"
					@click=${this._handleConfirm}></uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.permission-toggle {
				display: flex;
				align-items: center;
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-space-3) 0 var(--uui-size-space-4) 0;
			}

			.permission-meta {
				margin-left: var(--uui-size-space-4);
				line-height: 1.2em;
			}

			.permission-name {
				font-weight: bold;
			}
		`,
	];
}

export default UmbEntityUserPermissionSettingsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-user-permission-modal': UmbEntityUserPermissionSettingsModalElement;
	}
}
