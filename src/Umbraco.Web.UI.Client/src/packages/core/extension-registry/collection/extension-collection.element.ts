import { umbExtensionsRegistry } from '../registry.js';
import type { UmbExtensionCollectionFilterModel } from './types.js';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { UMB_COLLECTION_CONTEXT, UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-extension-collection')
export class UmbExtensionCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: UmbDefaultCollectionContext<any, UmbExtensionCollectionFilterModel>;

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

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

	#onSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const filter = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ filter }), this.#inputTimerAmount);
	}

	protected override renderToolbar() {
		return html`
			<div id="toolbar" slot="header">
				<uui-input @input=${this.#onSearch} label="Search" placeholder="Search..." id="input-search"></uui-input>
				<uui-select
					label="Select type..."
					placeholder="Select type..."
					.options=${this.#options}
					@change=${this.#onChange}></uui-select>
			</div>
		`;
	}

	static override styles = [
		css`
			#toolbar {
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between;
				align-items: center;
				padding-left: var(--uui-size-space-4);
				padding-right: var(--uui-size-space-6);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
			uui-select {
				width: 100%;
			}
		`,
	];
}

export default UmbExtensionCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-collection': UmbExtensionCollectionElement;
	}
}
