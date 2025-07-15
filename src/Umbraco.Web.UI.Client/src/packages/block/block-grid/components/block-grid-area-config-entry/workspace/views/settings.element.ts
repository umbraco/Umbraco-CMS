import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputNumberRangeElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-block-grid-area-type-workspace-view')
export class UmbBlockGridAreaTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	// TODO: Add Localizations...
	// TODO: Validation to prevent spaces and weird characters in alias:

	#dataset?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	@state()
	_minValue?: number;
	@state()
	_maxValue?: number;

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.#dataset = context;
			this.observe(
				await this.#dataset?.propertyValueByAlias<number>('minAllowed'),
				(min) => {
					this._minValue = min ?? 0;
				},
				'observeMinAllowed',
			);
			this.observe(
				await this.#dataset?.propertyValueByAlias<number>('maxAllowed'),
				(max) => {
					this._maxValue = max ?? Infinity;
				},
				'observeMaxAllowed',
			);
		});
	}
	#onAllowedRangeChange = (e: UmbChangeEvent) => {
		this.#dataset?.setPropertyValue('minAllowed', (e!.target! as UmbInputNumberRangeElement).minValue);
		this.#dataset?.setPropertyValue('maxAllowed', (e!.target! as UmbInputNumberRangeElement).maxValue);
	};

	override render() {
		return html`
			<uui-box headline=${'Identification'}>
				<umb-property
					label=${this.localize.term('general_alias')}
					alias="alias"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>

				<umb-property
					label=${'Create Button Label'}
					alias="createLabel"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>
			</uui-box>
			<uui-box headline=${'Validation'}>
				<umb-property-layout label=${'rangeAllowed'}>
					<umb-input-number-range
						slot="editor"
						.minValue=${this._minValue}
						.maxValue=${this._maxValue}
						@change=${this.#onAllowedRangeChange}>
					</umb-input-number-range>
				</umb-property-layout>

				<umb-property
					label=${'specifiedAllowance'}
					alias="specifiedAllowance"
					property-editor-ui-alias="Umb.PropertyEditorUi.BlockGridAreaTypePermission"></umb-property>
			</uui-box>
		`;
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

export default UmbBlockGridAreaTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-area-type-workspace-view-settings': UmbBlockGridAreaTypeWorkspaceViewSettingsElement;
	}
}
