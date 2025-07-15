import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { type UmbPropertyValueData, type UmbPropertyDatasetElement } from '@umbraco-cms/backoffice/property';

@customElement('example-dataset-dashboard')
export class ExampleDatasetDashboard extends UmbElementMixin(LitElement) {
	data: UmbPropertyValueData[] = [
		{
			alias: 'textProperty',
			value: 'Hello',
		},
	];

	#onDataChange(e: Event) {
		const oldValue = this.data;
		this.data = (e.target as UmbPropertyDatasetElement).value;
		this.requestUpdate('data', oldValue);
	}

	override render() {
		return html`
			<uui-box class="uui-text">
				<h1 class="uui-h2" style="margin-top: var(--uui-size-layout-1);">Dataset Example</h1>
				<umb-property-dataset .value=${this.data} @change=${this.#onDataChange}>
					<umb-property
						label="Textual input"
						description="Example of text editor"
						alias="textProperty"
						property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>
					<umb-property
						label="List of options"
						description="Example of dropdown editor"
						alias="listProperty"
						.config=${[
							{
								alias: 'multiple',
								value: false,
							},
							{
								alias: 'items',
								value: ['First Option', 'Second Option', 'Third Option'],
							},
						]}
						property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"></umb-property>
				</umb-property-dataset>

				<h5 class="uui-h3" style="margin-top: var(--uui-size-layout-1);">Output of dashboard data:</h5>
				<code> ${JSON.stringify(this.data, null, 2)} </code>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export default ExampleDatasetDashboard;

declare global {
	interface HTMLElementTagNameMap {
		'example-dataset-dashboard': ExampleDatasetDashboard;
	}
}
