import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

@customElement('umb-user-element-start-node')
export class UmbUserElementStartNodeElement extends UmbLitElement {
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
		// TODO: get back to this when elements have been decoupled from users.
		// The repository alias is hardcoded on purpose to avoid a element import in the user module.
		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			'Umb.Repository.ElementFolderItem',
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
					name="Element Root"
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
					<!-- TODO: get correct variant name -->
					<uui-ref-node
						name=${item.variants[0]?.name}
						?disabled=${this.readonly}
						style="--uui-color-disabled-contrast: var(--uui-color-text)">
						<uui-icon slot="icon" name="folder"></uui-icon>
					</uui-ref-node>
				`;
			},
		);
	}
}

export default UmbUserElementStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-element-start-node': UmbUserElementStartNodeElement;
	}
}
