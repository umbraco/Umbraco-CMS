import { umbExtensionsRegistry } from '../registry.js';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UMB_DEFAULT_COLLECTION_CONTEXT, UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-extension-collection')
export class UmbExtensionCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: UmbDefaultCollectionContext;

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	#options: Array<Option> = [];

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
		});

		this.observe(umbExtensionsRegistry.extensions, (extensions) => {
			const types = [...new Set(extensions.map((x) => x.type))];
			const options = types.sort().map((x) => ({ name: this.#camelCaseToWords(x), value: x }));
			this.#options = [{ name: 'All', value: '' }, ...options];
		});
	}

	// credit: https://stackoverflow.com/a/7225450/12787 [LK]
	#camelCaseToWords(input: string) {
		const result = input.replace(/([A-Z])/g, ' $1');
		return result.charAt(0).toUpperCase() + result.slice(1);
	}

	#onChange(event: UUISelectEvent) {
		const extensionType = event.target.value;
		console.log('onChange', extensionType);
		this.#collectionContext?.setFilter({ type: extensionType });
	}

	#onSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const query = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ query }), this.#inputTimerAmount);
	}

	protected renderToolbar() {
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

	static styles = [
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
