import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-image-crops-configuration
 */
@customElement('umb-property-editor-ui-image-crops-configuration')
export class UmbPropertyEditorUIImageCropsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	//TODO MAKE TYPE
	@property()
	value: {
		alias: string;
		width: number;
		height: number;
	}[] = [];

	render() {
		return html`
			<div class="inputs">
				<div class="input">
					<uui-label for="alias">Aliasss</uui-label>
					<uui-input id="alias" type="text" autocomplete="false" value=""></uui-input>
				</div>
				<div class="input">
					<uui-label for="width">Width</uui-label>
					<uui-input id="width" type="number" autocomplete="false" value="">
						<span class="append" slot="append">px</span>
					</uui-input>
				</div>
				<div class="input">
					<uui-label for="height">Height</uui-label>
					<uui-input id="height" type="number" autocomplete="false" value="">
						<span class="append" slot="append">px</span>
					</uui-input>
				</div>
				<uui-button look="secondary">Add</uui-button>
			</div>
			<div></div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.inputs {
				display: flex;
				gap: var(--uui-size-space-2);
			}
			.input {
				display: flex;
				flex-direction: column;
			}
			.append {
				padding-inline: var(--uui-size-space-4);
				background: var(--uui-color-disabled);
				border-left: 1px solid var(--uui-color-border);
				color: var(--uui-color-disabled-contrast);
				font-size: 0.8em;
				display: flex;
				align-items: center;
			}
		`,
	];
}

export default UmbPropertyEditorUIImageCropsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-crops-configuration': UmbPropertyEditorUIImageCropsConfigurationElement;
	}
}
