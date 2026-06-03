import { customElement, nothing, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import { UMB_DOCUMENT_ENTITY_TYPE, UmbDocumentItemDataResolver } from '@umbraco-cms/backoffice/document';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import type { UmbContentPickerResolvedItem } from './value-summary.resolver.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-content-picker-property-editor-value-summary')
export class UmbContentPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbContentPickerResolvedItem>
> {
	@state()
	private _names: Array<string> = [];

	#documentResolvers = new Map<string, UmbDocumentItemDataResolver<UmbDocumentItemModel>>();
	#resolvedDocNames = new Map<string, string>();

	protected override willUpdate(changedProperties: PropertyValueMap<this>): void {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_value' as keyof this)) {
			this.#syncResolvers();
		}
	}

	#syncResolvers() {
		const value = this._value ?? [];
		const docEntries = value.filter((e) => e.entityType === UMB_DOCUMENT_ENTITY_TYPE);

		for (const unique of this.#documentResolvers.keys()) {
			if (!docEntries.find((e) => e.unique === unique)) {
				this.#documentResolvers.get(unique)?.destroy();
				this.#documentResolvers.delete(unique);
				this.#resolvedDocNames.delete(unique);
			}
		}

		for (const entry of docEntries) {
			if (!this.#documentResolvers.has(entry.unique)) {
				const resolver = new UmbDocumentItemDataResolver<UmbDocumentItemModel>(this);
				resolver.setData(entry);
				this.#documentResolvers.set(entry.unique, resolver);
				this.observe(
					resolver.name,
					(name) => {
						this.#resolvedDocNames.set(entry.unique, name ?? '');
						this.#buildNames();
					},
					`doc-${entry.unique}`,
				);
			}
		}

		this.#buildNames();
	}

	#buildNames() {
		this._names = (this._value ?? []).map((entry) => {
			if (entry.entityType === UMB_DOCUMENT_ENTITY_TYPE) {
				return this.#resolvedDocNames.get(entry.unique) ?? '';
			}
			return entry.name;
		});
	}

	override render() {
		if (!this._value?.length) return nothing;
		const text = this._names.filter(Boolean).join(', ');
		if (!text) return nothing;
		return html`<span title="${text}">${text}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-picker-property-editor-value-summary': UmbContentPickerPropertyEditorValueSummaryElement;
	}
}
