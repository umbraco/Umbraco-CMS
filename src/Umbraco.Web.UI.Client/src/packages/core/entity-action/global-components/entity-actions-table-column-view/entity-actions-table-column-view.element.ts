import type { UmbEntityModel, UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

type UmbEntityActionsTableColumnValue = UmbEntityModel | UmbNamedEntityModel | { name?: string };

const deprecation = new UmbDeprecation({
	deprecated: 'Passing `entityType` and `unique` via the `value` property on `<umb-entity-actions-table-column-view>`.',
	removeInVersion: '19',
	solution: 'Provide the entity type and unique via the UMB_ENTITY_CONTEXT context instead.',
});

@customElement('umb-entity-actions-table-column-view')
export class UmbEntityActionsTableColumnViewElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	@property({ attribute: false })
	column!: UmbTableColumn;

	@property({ attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	get value(): UmbEntityActionsTableColumnValue {
		return this.#value;
	}
	set value(newValue: UmbEntityActionsTableColumnValue) {
		const oldValue = this.#value;
		this.#value = newValue ?? {};
		if ('entityType' in this.#value || 'unique' in this.#value) {
			deprecation.warn();
		}
		this.requestUpdate('value', oldValue);
	}
	#value: UmbEntityActionsTableColumnValue = {};

	override render() {
		// TODO (v19): Remove deprecated property forwarding when entityType/unique on value is removed.
		const entityType = 'entityType' in this.value ? this.value.entityType : undefined;
		const unique = 'unique' in this.value ? this.value.unique : undefined;

		return html`
			<umb-entity-actions-bundle
				.entityType=${entityType}
				.unique=${unique}
				.label=${this.localize.string((this.value as UmbNamedEntityModel)?.name)}>
			</umb-entity-actions-bundle>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-table-column-view': UmbEntityActionsTableColumnViewElement;
	}
}
