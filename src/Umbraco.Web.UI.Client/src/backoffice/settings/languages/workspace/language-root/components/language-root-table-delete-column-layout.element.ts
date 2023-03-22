import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-language-root-table-delete-column-layout')
export class UmbLanguageRootTableDeleteColumnLayoutElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	value!: LanguageResponseModel;

	@state()
	_isOpen = false;

	#onActionExecuted() {
		this._isOpen = false;
	}

	#onClick() {
		this._isOpen = !this._isOpen;
	}

	#onClose() {
		this._isOpen = false;
	}

	render() {
		// TODO: we need to use conditionals on each action here. But until we have that in place
		// we'll just remove all actions on the default language.
		if (this.value.isDefault) return nothing;

		return html`
			<umb-dropdown .open="${this._isOpen}" @close=${this.#onClose}>
				<uui-button slot="trigger" compact @click=${this.#onClick}><uui-symbol-more></uui-symbol-more></uui-button>
				<umb-entity-action-list
					slot="dropdown"
					@executed=${this.#onActionExecuted}
					entity-type="language"
					unique=${ifDefined(this.value.isoCode)}></umb-entity-action-list>
			</umb-dropdown>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-table-delete-column-layout': UmbLanguageRootTableDeleteColumnLayoutElement;
	}
}
