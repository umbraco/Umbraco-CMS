import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbInputNumberRangeElement } from '@umbraco-cms/backoffice/components';
import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/data-type';

@customElement('umb-block-grid-type-workspace-view')
export class UmbBlockGridTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#labelOnTopSetting = { alias: 'labelOnTop', value: true };

	@state()
	private _showSizeOptions = false;

	@state()
	private _rowMinSpan?: number;

	@state()
	private _rowMaxSpan?: number;

	@state()
	private _dataTypeGridColumns?: number;

	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, async (context) => {
			this.observe(
				await context?.propertyValueByAlias<undefined | string>('gridColumns'),
				(value) => {
					this._dataTypeGridColumns = value ? parseInt(value, 10) : undefined;
				},
				'observeGridColumns',
			);
		}).passContextAliasMatches();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.#datasetContext = context;

			// TODO set showSizeOption to true when rowMinSpan or rowMaxSpan is set

			this.observe(
				await context?.propertyValueByAlias('columnSpanOptions'),
				(value) => {
					if (Array.isArray(value) && value.length > 0) {
						this._showSizeOptions = true;
					}
					this.removeUmbControllerByAlias('_observeColumnSpanOptions');
				},
				'observeColumnSpanOptions',
			);

			this.observe(
				await context?.propertyValueByAlias<number | undefined>('rowMinSpan'),
				(value) => {
					this._rowMinSpan = value;
				},
				'observeRowMinSpan',
			);

			this.observe(
				await context?.propertyValueByAlias<number | undefined>('rowMaxSpan'),
				(value) => {
					this._rowMaxSpan = value;
				},
				'observeRowMaxSpan',
			);
		});
	}

	#onRowSpanChange(e: CustomEvent) {
		this.#datasetContext?.setPropertyValue('rowMinSpan', (e.target as UmbInputNumberRangeElement).minValue);
		this.#datasetContext?.setPropertyValue('rowMaxSpan', (e.target as UmbInputNumberRangeElement).maxValue);
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('general_general')}>
				<umb-property
					label=${this.localize.term('general_label')}
					alias="label"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
					.config=${[this.#labelOnTopSetting]}></umb-property>

				<umb-property
					label=${this.localize.term('blockEditor_labelContentElementType')}
					read-only
					alias="contentElementTypeKey"
					property-editor-ui-alias="Umb.PropertyEditorUi.DocumentTypePicker"
					.config=${[
						{ alias: 'onlyPickElementTypes', value: true },
						{ alias: 'validationLimit', value: { min: 0, max: 1 } },
					]}></umb-property>

				<umb-property
					label=${this.localize.term('blockEditor_labelSettingsElementType')}
					alias="settingsElementTypeKey"
					property-editor-ui-alias="Umb.PropertyEditorUi.DocumentTypePicker"
					.config=${[
						{ alias: 'onlyPickElementTypes', value: true },
						{ alias: 'validationLimit', value: { min: 0, max: 1 } },
					]}></umb-property>
			</uui-box>
			<uui-box headline=${this.localize.term('blockEditor_headlineAllowance')}>
				<umb-property
					label=${this.localize.term('blockEditor_allowBlockInRoot')}
					alias="allowAtRoot"
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>

				<umb-property
					label=${this.localize.term('blockEditor_allowBlockInAreas')}
					alias="allowInAreas"
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>
			</uui-box>
			<uui-box headline=${this.localize.term('blockEditor_sizeOptions')}> ${this.#renderSizeOptions()} </uui-box>
		`;
	}

	#renderSizeOptions() {
		if (this._showSizeOptions) {
			return html`
				<umb-property
					label=${this.localize.term('blockEditor_allowedBlockColumns')}
					alias="columnSpanOptions"
					property-editor-ui-alias="Umb.PropertyEditorUi.BlockGridColumnSpan"
					data-test-attr="${this._dataTypeGridColumns + ' '}"
					.config=${[
						{
							alias: 'maxColumns',
							value: this._dataTypeGridColumns,
						},
					]}></umb-property>

				<umb-property-layout label=${this.localize.term('blockEditor_allowedBlockRows')}>
					<umb-input-number-range
						slot="editor"
						.minValue=${this._rowMinSpan}
						.maxValue=${this._rowMaxSpan}
						@change=${this.#onRowSpanChange}></umb-input-number-range>
				</umb-property-layout>
			`;
		} else {
			return html`<div id="showOptions">
				<uui-button
					label=${this.localize.term('blockEditor_showSizeOptions')}
					look="placeholder"
					@click=${() => (this._showSizeOptions = !this._showSizeOptions)}></uui-button>
			</div>`;
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(600px, 1fr));
				gap: var(--uui-size-layout-1);
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}

			#showOptions {
				display: flex;
				height: 100px;
			}
			#showOptions uui-button {
				flex: 1;
			}
		`,
	];
}

export default UmbBlockGridTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-type-workspace-view-settings': UmbBlockGridTypeWorkspaceViewSettingsElement;
	}
}
