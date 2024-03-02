import type { UmbMediaCollectionItemModel } from '../../../types.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-media-table-column-name')
export class UmbMediaTableColumnNameElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	@state()
	private _editMediaPath = '';

	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbMediaCollectionItemModel;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media')
			.onSetup(() => {
				return { data: { entityType: 'media', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaPath = routeBuilder({});
			});
	}

	#onClick(event: Event) {
		// TODO: [LK] Review the `stopPropagation` usage, as it causes a page reload.
		// But we still need a say to prevent the `umb-table` from triggering a selection event.
		event.stopPropagation();
	}

	render() {
		return html`<uui-button
			look="default"
			color="default"
			compact
			href="${this._editMediaPath}edit/${this.value.unique}"
			label="${this.value.name}"
			@click=${this.#onClick}></uui-button>`;
	}

	static styles = [
		css`
			uui-button {
				text-align: left;
			}
		`,
	];
}

export default UmbMediaTableColumnNameElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-table-column-name': UmbMediaTableColumnNameElement;
	}
}
