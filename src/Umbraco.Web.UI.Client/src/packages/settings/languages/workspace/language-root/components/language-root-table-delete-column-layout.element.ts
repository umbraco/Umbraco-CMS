import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-language-root-table-delete-column-layout')
export class UmbLanguageRootTableDeleteColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value!: LanguageResponseModel;

	@state()
	_isOpen = false;

	#onActionExecuted() {
		this._isOpen = false;
	}

	render() {
		// TODO: we need to use conditionals on each action here. But until we have that in place
		// we'll just remove all actions on the default language.
		if (this.value.isDefault) return nothing;

		return html`
			<umb-dropdown .open=${this._isOpen} compact hide-expand>
				<uui-symbol-more slot="label"></uui-symbol-more>
				<umb-entity-action-list
					@action-executed=${this.#onActionExecuted}
					entity-type="language"
					unique=${ifDefined(this.value.isoCode)}></umb-entity-action-list>
			</umb-dropdown>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-table-delete-column-layout': UmbLanguageRootTableDeleteColumnLayoutElement;
	}
}
