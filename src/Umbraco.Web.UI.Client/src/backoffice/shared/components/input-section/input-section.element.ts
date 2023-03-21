import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbInputListBase } from '../input-list-base/input-list-base';
import { UMB_SECTION_PICKER_MODAL_TOKEN } from '../../modals/section-picker';
import type { ManifestSection } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

@customElement('umb-input-section')
export class UmbInputPickerSectionElement extends UmbInputListBase {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			#user-group-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			.user-group {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
			.user-group div {
				display: flex;
				align-items: center;
				gap: var(--uui-size-4);
			}
			.user-group uui-button {
				margin-left: auto;
			}
		`,
	];

	@state()
	private _sections: Array<ManifestSection> = [];

	connectedCallback(): void {
		super.connectedCallback();
		this.pickerToken = UMB_SECTION_PICKER_MODAL_TOKEN;
		this._observeSections();
	}

	private _observeSections() {
		if (this.value.length > 0) {
			umbExtensionsRegistry.extensionsOfType('section').subscribe((sections: Array<ManifestSection>) => {
				this._sections = sections.filter((section) => this.value.includes(section.alias));
			});
		} else {
			this._sections = [];
		}
	}

	selectionUpdated() {
		this._observeSections();
		// TODO: Use proper event class:
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	renderContent() {
		if (this._sections.length === 0) return html`${nothing}`;

		return html`
			<div id="user-list">
				${this._sections.map(
					(section) => html`
						<div class="user-group">
							<div>
								<span>${section.meta.label}</span>
							</div>
							<uui-button
								@click=${() => this.removeFromSelection(section.alias)}
								label="remove"
								color="danger"></uui-button>
						</div>
					`
				)}
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-section': UmbInputPickerSectionElement;
	}
}
