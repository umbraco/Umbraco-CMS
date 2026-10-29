import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import { UmbSchemaOperationAllowedController } from '@umbraco-cms/backoffice/schema-lockdown';

@customElement('umb-dictionary-collection')
export class UmbDictionaryCollectionElement extends UmbCollectionDefaultElement {
	@state()
	private _isReadOnly = false;

	constructor() {
		super();
		this.observe(
			new UmbSchemaOperationAllowedController(this, { entityType: UMB_DICTIONARY_ENTITY_TYPE, operation: 'update' })
				.allowed,
			(allowed) => (this._isReadOnly = !allowed),
			null,
		);
	}

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<div id="toolbar-content">
					<umb-collection-filter-field></umb-collection-filter-field>
					${this.#renderReadOnlyTag()}
				</div>
			</umb-collection-toolbar>
		`;
	}

	#renderReadOnlyTag() {
		if (!this._isReadOnly) return nothing;
		return html`<uui-tag look="secondary">${this.localize.term('general_readOnly')}</uui-tag>`;
	}

	static override styles = [
		...UmbCollectionDefaultElement.styles,
		css`
			#toolbar-content {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
			}

			umb-collection-filter-field {
				flex: 1 1 auto;
			}

			uui-tag {
				white-space: nowrap;
			}
		`,
	];
}

export { UmbDictionaryCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-collection': UmbDictionaryCollectionElement;
	}
}
