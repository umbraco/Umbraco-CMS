import type {
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue,
} from './entity-user-permission-settings-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-entity-user-permission-settings-modal')
export class UmbEntityUserPermissionSettingsModalElement extends UmbModalBaseElement<
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue
> {
	override set data(data: UmbEntityUserPermissionSettingsModalData | undefined) {
		super.data = data;
		this._entityType = data?.entityType;
		this._headline = data?.headline ?? this._headline;
		this._preset = data?.preset;
	}

	@state()
	_headline: string = 'Set permissions';

	@state()
	_entityType?: string;

	@state()
	_preset?: UmbEntityUserPermissionSettingsModalValue;

	override connectedCallback(): void {
		super.connectedCallback();

		if (this._preset?.allowedVerbs) {
			this.updateValue({ allowedVerbs: this._preset?.allowedVerbs });
		}
	}

	#onPermissionChange(event: UmbSelectionChangeEvent) {
		const target = event.target as any;
		this.updateValue({ allowedVerbs: target.allowedVerbs });
	}

	override render() {
		return html`
			<umb-body-layout headline=${this._headline}>
				<uui-box>
					${this._entityType
						? html` <umb-input-entity-user-permission
								.entityType=${this._entityType}
								.allowedVerbs=${this.value?.allowedVerbs ?? []}
								@change=${this.#onPermissionChange}></umb-input-entity-user-permission>`
						: nothing}
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}">Cancel</uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label="Confirm"
					@click=${this._submitModal}></uui-button>
			</umb-body-layout>
		`;
	}

	static override readonly styles = [
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
