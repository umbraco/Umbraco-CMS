import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

@customElement('umb-extension-collection')
export class UmbExtensionCollectionElement extends UmbCollectionDefaultElement {
	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<div id="toolbar">
					<umb-collection-filter-field></umb-collection-filter-field>
				</div>
			</umb-collection-toolbar>
		`;
	}

	static override styles = [
		css`
			#toolbar {
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between;
				align-items: center;

				umb-collection-filter-field {
					flex: 1;
				}

				uui-select {
					flex: 1;
				}
			}
		`,
	];
}

export default UmbExtensionCollectionElement;

export { UmbExtensionCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-collection': UmbExtensionCollectionElement;
	}
}
