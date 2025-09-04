import type {
	UmbTiptapTablePropertiesModalData,
	UmbTiptapTablePropertiesModalValue,
} from './table-properties-modal.token.js';
import { css, customElement, html, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbPropertyDatasetElement, UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

type UmbProperty = {
	alias: string;
	config?: UmbPropertyEditorConfig;
	description?: string;
	label: string;
	propertyEditorUiAlias: string;
};

@customElement('umb-tiptap-table-properties-modal')
export class UmbTiptapTablePropertiesModalElement extends UmbModalBaseElement<
	UmbTiptapTablePropertiesModalData,
	UmbTiptapTablePropertiesModalValue
> {
	#appearance = { labelOnTop: true };

	#properties: Array<UmbProperty> = [
		{ alias: 'width', label: 'Width', propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox' },
		{ alias: 'height', label: 'Height', propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox' },
		{
			alias: 'backgroundColor',
			label: 'Background color',
			propertyEditorUiAlias: 'Umb.PropertyEditorUi.EyeDropper',
			config: [{ alias: 'showPalette', value: true }],
		},
		{
			alias: 'alignment',
			label: 'Alignment',
			propertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
			config: [
				{
					alias: 'items',
					value: [
						{ name: 'None', value: 'none' },
						{ name: 'Left', value: 'left' },
						{ name: 'Center', value: 'center' },
						{ name: 'Right', value: 'right' },
					],
				},
			],
		},
		{ alias: 'borderWidth', label: 'Border width', propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox' },
		{
			alias: 'borderStyle',
			label: 'Border style',
			propertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
			config: [
				{
					alias: 'items',
					value: [
						{ name: 'Solid', value: 'solid' },
						{ name: 'Dotted', value: 'dotted' },
						{ name: 'Dashed', value: 'dashed' },
						{ name: 'Double', value: 'double' },
						{ name: 'Groove', value: 'groove' },
						{ name: 'Ridge', value: 'ridge' },
						{ name: 'Inset', value: 'inset' },
						{ name: 'Outset', value: 'outset' },
						{ name: 'None', value: 'none' },
						{ name: 'Hidden', value: 'hidden' },
					],
				},
			],
		},
		{
			alias: 'borderColor',
			label: 'Border color',
			propertyEditorUiAlias: 'Umb.PropertyEditorUi.EyeDropper',
			config: [{ alias: 'showPalette', value: true }],
		},
	];

	#values: Array<UmbPropertyValueData> = [];

	override connectedCallback(): void {
		super.connectedCallback();

		if (this.data) {
			this.#values = Object.entries(this.data).map(([alias, value]) => ({ alias, value }));
		}
	}

	#onChange(event: Event & { target: UmbPropertyDatasetElement }) {
		this.#values = event.target.value;
	}

	#onSubmit() {
		this.value = this.#values;
		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline="Table properties">
				<uui-box>
					${when(
						this.#properties?.length,
						() => html`
							<umb-property-dataset .value=${this.#values} @change=${this.#onChange}>
								${repeat(
									this.#properties,
									(property) => property.alias,
									(property) => html`
										<umb-property
											alias=${property.alias}
											label=${property.label}
											property-editor-ui-alias=${property.propertyEditorUiAlias}
											.appearance=${this.#appearance}
											.config=${property.config}>
										</umb-property>
									`,
								)}
							</umb-property-dataset>
						`,
						() => html`<p>There are no properties for this modal.</p>`,
					)}
				</uui-box>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
				<uui-button
					slot="actions"
					color="positive"
					look="primary"
					label=${this.localize.term('bulk_done')}
					@click=${this.#onSubmit}></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			umb-property {
				--uui-size-layout-1: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbTiptapTablePropertiesModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-table-properties-modal': UmbTiptapTablePropertiesModalElement;
	}
}
