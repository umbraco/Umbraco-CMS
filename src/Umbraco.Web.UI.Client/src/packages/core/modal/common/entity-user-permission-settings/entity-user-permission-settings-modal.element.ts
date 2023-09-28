import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalResult,
	UmbModalContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestUserPermission, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-entity-user-permission-settings-modal')
export class UmbEntityUserPermissionSettingsModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbEntityUserPermissionSettingsModalData, UmbEntityUserPermissionSettingsModalResult>;

	@property({ type: Object })
	data?: UmbEntityUserPermissionSettingsModalData;

	@state()
	private _userPermissionManifests: Array<ManifestUserPermission> = [];

	private _handleConfirm() {
		this.modalContext?.submit();
	}

	private _handleCancel() {
		this.modalContext?.reject();
	}

	constructor() {
		super();
		this.observe(
			umbExtensionsRegistry.extensionsOfType('userPermission'),
			(userPermissionManifests) => (this._userPermissionManifests = userPermissionManifests),
		);
	}

	render() {
		return html`
			<umb-body-layout headline="Hello">
				<uui-box>
					<umb-entity-user-permission-settings
						entity-type=${this.data?.entityType}></umb-entity-user-permission-settings>

					Render user permissions for ${this.data?.entityType} ${this.data?.unique}
					${this._userPermissionManifests.map((permission) => this.#renderPermission(permission))}

					<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
					<uui-button
						slot="actions"
						id="confirm"
						color="positive"
						look="primary"
						label="Confirm"
						@click=${this._handleConfirm}></uui-button>
				</uui-box>
			</umb-body-layout>
		`;
	}

	#onChangeUserPermission(event: UUIBooleanInputEvent, userPermissionManifest: ManifestUserPermission) {
		console.log(userPermissionManifest);
		console.log(event.target.checked);
	}

	#isAllowed(userPermissionManifest: ManifestUserPermission) {
		return true;
		//return this._userGroup?.permissions?.includes(userPermissionManifest.alias);
	}

	#renderPermission(userPermissionManifest: ManifestUserPermission) {
		return html`<div
			style="display: flex; align-items:center; border-bottom: 1px solid var(--uui-color-divider); padding: 9px 0 12px 0;">
			<uui-toggle
				label=${userPermissionManifest.meta.label}
				?checked=${this.#isAllowed(userPermissionManifest)}
				@change=${(event: UUIBooleanInputEvent) => this.#onChangeUserPermission(event, userPermissionManifest)}>
				<div class="permission-meta">
					<div class="permission-name">${userPermissionManifest.meta.label}</div>
					<small>${userPermissionManifest.meta.description}</small>
				</div>
			</uui-toggle>
		</div>`;
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
