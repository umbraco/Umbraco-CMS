import { UmbDocumentItemDataResolver } from '../../../item/document-item-data-resolver.js';
import type { UmbDocumentItemModel } from '../../../item/repository/types.js';
import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

/** Renders picked document names (variant-aware, comma-joined) for collection view cells. */
@customElement('umb-multiple-document-picker-property-editor-value-summary')
export class UmbMultipleDocumentPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbDocumentItemModel>
> {
	@state()
	private _names: Array<string> = [];

	readonly #resolvers = new Map<string, UmbDocumentItemDataResolver<UmbDocumentItemModel>>();
	readonly #resolvedNames = new Map<string, string>();

	protected override willUpdate(changedProperties: PropertyValueMap<this>): void {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_value' as keyof this)) {
			this.#syncResolvers();
		}
	}

	#syncResolvers() {
		const value = this._value ?? [];

		for (const unique of this.#resolvers.keys()) {
			if (!value.some((item) => item.unique === unique)) {
				this.#removeResolver(unique);
			}
		}

		for (const item of value) {
			this.#addOrUpdateResolver(item);
		}

		this.#buildNames();
	}

	#removeResolver(unique: string) {
		this.#resolvers.get(unique)?.destroy();
		this.#resolvers.delete(unique);
		this.#resolvedNames.delete(unique);
		this.removeUmbControllerByAlias(`document-${unique}`);
	}

	#addOrUpdateResolver(item: UmbDocumentItemModel) {
		if (this.#resolvers.has(item.unique)) {
			this.#resolvers.get(item.unique)!.setData(item);
			return;
		}

		const resolver = new UmbDocumentItemDataResolver<UmbDocumentItemModel>(this);
		resolver.setData(item);
		this.#resolvers.set(item.unique, resolver);
		this.observe(
			resolver.name,
			(name) => {
				this.#resolvedNames.set(item.unique, name ?? '');
				this.#buildNames();
			},
			`document-${item.unique}`,
		);
	}

	#buildNames() {
		this._names = (this._value ?? []).map((item) => this.#resolvedNames.get(item.unique) ?? '');
	}

	override render() {
		if (!this._value?.length) return nothing;
		const text = this._names.filter(Boolean).join(', ');
		if (!text) return nothing;
		return html`<span title="${text}">${text}</span>`;
	}
}

export { UmbMultipleDocumentPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-multiple-document-picker-property-editor-value-summary': UmbMultipleDocumentPickerPropertyEditorValueSummaryElement;
	}
}
