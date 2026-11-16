import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

@customElement('umb-user-document-blueprint-start-node')
export class UmbUserDocumentBlueprintStartNodeElement extends UmbLitElement {
	#uniques: Array<string> = [];
	@property({ type: Array, attribute: false })
	public get uniques(): Array<string> {
		return this.#uniques;
	}
	public set uniques(value: Array<string>) {
		this.#uniques = value;

		if (this.#uniques.length > 0) {
			this.#observeItems();
		}
	}

	@property({ type: Boolean })
	readonly = false;

	@state()
	private _displayValue: Array<any> = [];

	async #observeItems() {
		// The repository alias is hardcoded on purpose to avoid a document blueprint import in the user module.
		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			'Umb.Repository.DocumentBlueprintFolderItem',
		);
		const { asObservable } = await itemRepository.requestItems(this.#uniques);

		this.observe(asObservable?.(), (data) => {
			this._displayValue = data || [];
		});
	}

	override render() {
		if (this.uniques.length < 1) {
			return html`
				<uui-ref-node
					name="Document Blueprint Root"
					?disabled=${this.readonly}
					style="--uui-color-disabled-contrast: var(--uui-color-text)">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			`;
		}

		return repeat(
			this._displayValue,
			(item) => item.unique,
			(item) => {
				return html`
					<uui-ref-node
						name=${item.name}
						?disabled=${this.readonly}
						style="--uui-color-disabled-contrast: var(--uui-color-text)">
						<uui-icon slot="icon" name=${item.icon ?? 'icon-folder'}></uui-icon>
					</uui-ref-node>
				`;
			},
		);
	}
}

export default UmbUserDocumentBlueprintStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-document-blueprint-start-node': UmbUserDocumentBlueprintStartNodeElement;
	}
}
