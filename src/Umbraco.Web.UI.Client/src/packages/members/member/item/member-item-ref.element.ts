import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type { UmbMemberItemModel } from '../repository/index.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UMB_PICKER_INPUT_CONTEXT } from '@umbraco-cms/backoffice/picker-input';

@customElement('umb-member-item-ref')
export class UmbMemberItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbMemberItemModel;

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
			.addAdditionalPath(UMB_MEMBER_ENTITY_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_MEMBER_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentPath = routeBuilder({});
			});
	}

	#getHref(item: UmbMemberItemModel) {
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
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				${this.#renderIcon(this.item)} ${this.#renderActions()}
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

	#renderIcon(item: UmbMemberItemModel) {
		if (!item.memberType.icon) return;
		return html`<umb-icon slot="icon" name=${item.memberType.icon}></umb-icon>`;
	}
}

export { UmbMemberItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-item-ref': UmbMemberItemRefElement;
	}
}
