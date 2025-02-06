import type { UmbDocumentItemModel } from '../repository/types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import {
	classMap,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UMB_PICKER_INPUT_CONTEXT } from '@umbraco-cms/backoffice/picker-input';

@customElement('umb-document-item-ref')
export class UmbDocumentItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDocumentItemModel;

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_isPickerInput = false;

	_editDocumentPath = '';

	#pickerInputContext?: typeof UMB_PICKER_INPUT_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_INPUT_CONTEXT, (context) => {
			this.#pickerInputContext = context;
			this._isPickerInput = context !== undefined;
		});

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_DOCUMENT_ENTITY_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_DOCUMENT_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentPath = routeBuilder({});
			});
	}

	#isDraft(item: UmbDocumentItemModel) {
		return item.variants[0]?.state === 'Draft';
	}

	#getHref(item: UmbDocumentItemModel) {
		return `${this._editDocumentPath}/edit/${item.unique}`;
	}

	#onRemove() {
		if (!this.item) return;
		this.#pickerInputContext?.requestRemoveItem(this.item.unique);
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				id=${this.item.unique}
				class=${classMap({ draft: this.#isDraft(this.item) })}
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				${this.#renderIcon(this.item)} ${this.#renderIsTrashed(this.item)} ${this.#renderActions()}
			</uui-ref-node>
		`;
	}

	#renderActions() {
		if (this.readonly) return;
		if (!this.item) return;
		if (!this._isPickerInput) return;

		return html`
			<uui-action-bar slot="actions">
				<uui-button label=${this.localize.term('general_remove')} @click=${this.#onRemove}></uui-button>
			</uui-action-bar>
		`;
	}

	#renderIcon(item: UmbDocumentItemModel) {
		if (!item.documentType.icon) return;
		return html`<umb-icon slot="icon" name=${item.documentType.icon}></umb-icon>`;
	}

	#renderIsTrashed(item: UmbDocumentItemModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}
}

export { UmbDocumentItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-item-ref': UmbDocumentItemRefElement;
	}
}
