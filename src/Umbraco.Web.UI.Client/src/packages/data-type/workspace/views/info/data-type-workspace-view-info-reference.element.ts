import { UmbDataTypeReferenceRepository } from '../../../reference/index.js';
import type { UmbDataTypeReferenceModel } from '../../../reference/index.js';
import { css, html, customElement, state, repeat, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

const elementName = 'umb-data-type-workspace-view-info-reference';

@customElement(elementName)
export class UmbDataTypeWorkspaceViewInfoReferenceElement extends UmbLitElement {
	#referenceRepository = new UmbDataTypeReferenceRepository(this);

	#routeBuilder?: UmbModalRouteBuilder;

	@property()
	dataTypeUnique = '';

	@state()
	private _loading = true;

	@state()
	private _items?: Array<UmbDataTypeReferenceModel> = [];

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(':entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#routeBuilder = routeBuilder;
			});
	}

	protected override firstUpdated() {
		this.#getReferences();
	}

	async #getReferences() {
		this._loading = true;

		const items = await this.#referenceRepository.requestReferencedBy(this.dataTypeUnique);
		if (!items) return;

		this._items = items;
		this._loading = false;
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('references_tabName')}>
				${when(
					this._loading,
					() => html`<uui-loader></uui-loader>`,
					() => this.#renderItems(),
				)}
			</uui-box>
		`;
	}

	#getEditPath(item: UmbDataTypeReferenceModel) {
		// TODO: [LK] Ask NL for a reminder on how the route constants work.
		return this.#routeBuilder && item.entityType
			? this.#routeBuilder({ entityType: item.entityType }) + `edit/${item.unique}`
			: '#';
	}

	#renderItems() {
		if (!this._items?.length) return html`<p>${this.localize.term('references_DataTypeNoReferences')}</p>`;
		return html`
			<uui-table>
				<uui-table-head>
					<uui-table-head-cell><umb-localize key="general_name">Name</umb-localize></uui-table-head-cell>
					<uui-table-head-cell><umb-localize key="general_type">Type</umb-localize></uui-table-head-cell>
					<uui-table-head-cell>
						<umb-localize key="references_usedByProperties">Referenced by</umb-localize>
					</uui-table-head-cell>
				</uui-table-head>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => html`
						<uui-table-row>
							<uui-table-cell>
								<uui-ref-node-document-type
									href=${this.#getEditPath(item)}
									name=${this.localize.string(item.name ?? item.unique)}>
									<umb-icon slot="icon" name=${item.icon ?? 'icon-document'}></umb-icon>
								</uui-ref-node-document-type>
							</uui-table-cell>
							<uui-table-cell>${item.entityType}</uui-table-cell>
							<uui-table-cell>${item.properties.map((prop) => prop.name).join(', ')}</uui-table-cell>
						</uui-table-row>
					`,
				)}
			</uui-table>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}
			uui-table-cell {
				color: var(--uui-color-text-alt);
			}
		`,
	];
}

export { UmbDataTypeWorkspaceViewInfoReferenceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDataTypeWorkspaceViewInfoReferenceElement;
	}
}
