import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UUIMenuItemElement, UUIMenuItemEvent } from '@umbraco-ui/uui';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbLanguageRepository } from '../../repository/language.repository';
import { UmbModalElementPickerBase } from '@umbraco-cms/internal/modal';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-language-picker-modal')
export class UmbLanguagePickerModalElement extends UmbModalElementPickerBase<LanguageResponseModel> {
	static styles = [UUITextStyles, css``];

	@state()
	private _languages: Array<LanguageResponseModel> = [];

	private _languageRepository = new UmbLanguageRepository(this);

	async firstUpdated() {
		const { data } = await this._languageRepository.requestLanguages();
		this._languages = data?.items ?? [];
	}

	#onSelection(event: UUIMenuItemEvent) {
		event?.stopPropagation();
		const language = event?.target as UUIMenuItemElement;
		const isoCode = language.dataset.isoCode;
		if (!isoCode) return;
		this.handleSelection(isoCode);
	}

	get #filteredLanguages() {
		if (this.data?.filter) {
			return this._languages.filter(this.data.filter);
		} else {
			return this._languages;
		}
	}

	render() {
		return html`<umb-body-layout headline="Select languages">
			<uui-box>
				${repeat(
					this.#filteredLanguages,
					(item) => item.isoCode,
					(item) => html`
						<uui-menu-item
							label=${item.name ?? ''}
							selectable="true"
							@selected=${this.#onSelection}
							@unselected=${this.#onSelection}
							?selected=${this.isSelected(item.isoCode!)}
							data-iso-code="${ifDefined(item.isoCode)}">
							<uui-icon slot="icon" name="umb:globe"></uui-icon>
						</uui-menu-item>
					`
				)}
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this.close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export default UmbLanguagePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-picker-modal': UmbLanguagePickerModalElement;
	}
}
