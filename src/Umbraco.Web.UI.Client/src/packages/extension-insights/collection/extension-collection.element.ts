import type { UmbExtensionCollectionFilterModel, UmbExtensionDetailModel } from './types.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_COLLECTION_CONTEXT, UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-extension-collection')
export class UmbExtensionCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: UmbDefaultCollectionContext<UmbExtensionDetailModel, UmbExtensionCollectionFilterModel>;

	#options: Array<Option> = [];

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
		});

		this.observe(umbExtensionsRegistry.extensions, (extensions) => {
			const types = [...new Set(extensions.map((x) => x.type))];
			const options = types.sort().map((x) => ({ name: fromCamelCase(x), value: x }));
			this.#options = [{ name: 'All', value: '' }, ...options];
		});
	}

	#onChange(event: UUISelectEvent) {
		const extensionType = event.target.value as string;
		this.#collectionContext?.setFilter({ type: extensionType });
	}

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<div id="toolbar">
					<umb-collection-filter-field></umb-collection-filter-field>
					<uui-select
						label="Select type..."
						placeholder="Select type..."
						.options=${this.#options}
						@change=${this.#onChange}></uui-select>
				</div>
			</umb-collection-toolbar>
		`;
	}

	static override styles = [
		css`
			#toolbar {
				flex: 1;
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between;
				align-items: center;
			}

			umb-collection-filter-field {
				width: 100%;
			}

			uui-select {
				width: 100%;
			}
		`,
	];
}

/** @deprecated Should be exported as `element` only; to be removed in Umbraco 17. */
export default UmbExtensionCollectionElement;

export { UmbExtensionCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-collection': UmbExtensionCollectionElement;
	}
}
