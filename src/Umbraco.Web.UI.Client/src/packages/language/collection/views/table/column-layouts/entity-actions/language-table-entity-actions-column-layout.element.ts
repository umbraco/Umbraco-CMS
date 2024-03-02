import type { UmbLanguageDetailModel } from '../../../../../types.js';
import { html, nothing, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-language-table-entity-actions-column-layout')
export class UmbLanguageTableEntityActionsColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value!: UmbLanguageDetailModel;

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
					entity-type=${this.value.entityType}
					unique=${ifDefined(this.value.unique)}></umb-entity-action-list>
			</umb-dropdown>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-table-entity-actions-column-layout': UmbLanguageTableEntityActionsColumnLayoutElement;
	}
}
