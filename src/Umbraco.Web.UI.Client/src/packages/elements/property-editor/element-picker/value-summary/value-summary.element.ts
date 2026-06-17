import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import { UmbElementItemDataResolver } from '../../../item/data-resolver/element-item-data-resolver.js';
import type { UmbElementItemModel } from '../../../types.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

/** Renders picked element names (variant-aware, comma-joined) for collection view cells. */
@customElement('umb-element-picker-property-editor-value-summary')
export class UmbElementPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbElementItemModel>
> {
	@state()
	private _names: Array<string> = [];

	#resolvers = new Map<string, UmbElementItemDataResolver<UmbElementItemModel>>();
	#resolvedNames = new Map<string, string>();

	protected override willUpdate(changedProperties: PropertyValueMap<this>): void {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_value' as keyof this)) {
			this.#syncResolvers();
		}
	}

	#syncResolvers() {
		const value = this._value ?? [];

		for (const unique of this.#resolvers.keys()) {
			if (!value.find((item) => item.unique === unique)) {
				this.#resolvers.get(unique)?.destroy();
				this.#resolvers.delete(unique);
				this.#resolvedNames.delete(unique);
				this.removeUmbControllerByAlias(`element-${unique}`);
			}
		}

		for (const item of value) {
			if (!this.#resolvers.has(item.unique)) {
				const resolver = new UmbElementItemDataResolver<UmbElementItemModel>(this);
				resolver.setData(item);
				this.#resolvers.set(item.unique, resolver);
				this.observe(
					resolver.name,
					(name) => {
						this.#resolvedNames.set(item.unique, name ?? '');
						this.#buildNames();
					},
					`element-${item.unique}`,
				);
			} else {
				this.#resolvers.get(item.unique)!.setData(item);
			}
		}

		this.#buildNames();
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

export { UmbElementPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-picker-property-editor-value-summary': UmbElementPickerPropertyEditorValueSummaryElement;
	}
}
