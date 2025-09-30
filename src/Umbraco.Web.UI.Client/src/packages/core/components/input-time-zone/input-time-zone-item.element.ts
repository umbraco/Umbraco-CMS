import { html, customElement, css, property, nothing, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { getTimeZoneName } from '@umbraco-cms/backoffice/utils';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-input-time-zone-item
 */
@customElement('umb-input-time-zone-item')
export class UmbInputTimeZoneItemElement extends UmbFormControlMixin<
	string | undefined,
	typeof UmbLitElement,
	undefined
>(UmbLitElement) {
	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled: boolean = false;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly: boolean = false;

	override render() {
		const label = (this.value && getTimeZoneName(this.value)) || '';
		return html`
			<div class="time-zone-item">
				${this.disabled || this.readonly ? nothing : html`<uui-icon name="icon-grip" class="handle"></uui-icon>`}

				<span>${label}</span>

				${when(
					!this.readonly,
					() => html`
						<uui-button
							class="time-zone-remove-button"
							compact
							label="${this.localize.term('general_remove')} ${label}"
							look="outline"
							?disabled=${this.disabled}
							@click=${() => this.dispatchEvent(new UmbDeleteEvent())}>
							<uui-icon name="icon-trash"></uui-icon>
						</uui-button>
					`,
				)}
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				align-items: center;
				margin-bottom: var(--uui-size-space-2);
				gap: var(--uui-size-space-3);
			}

			.time-zone-item {
				background-color: var(--uui-color-surface-alt);
				display: flex;
				align-items: center;
				gap: var(--uui-size-6);
				padding: var(--uui-size-2) var(--uui-size-6);
				width: 100%;
			}

			.time-zone-remove-button {
				margin-left: auto;
			}

			.handle {
				cursor: grab;
			}

			.handle:active {
				cursor: grabbing;
			}
		`,
	];
}

export default UmbInputTimeZoneItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-time-zone-item': UmbInputTimeZoneItemElement;
	}
}
