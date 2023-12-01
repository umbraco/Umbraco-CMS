import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-list-block-configuration
 */
@customElement('umb-property-editor-ui-block-list-block-configuration')
export class UmbPropertyEditorUIBlockListBlockConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property()
	value = '';

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	constructor() {
		super();
		this.consumeContext(UMB_VARIANT_CONTEXT, async (variantContext) => {
			console.log('variantContext', variantContext);
			this.observe(await variantContext.propertyValueByAlias('validationLimit'), (x) =>
				console.log('validationLimit', x),
			);
		});
	}

	render() {
		return html`
			<div>umb-property-editor-ui-block-list-block-configuration</div>

			Temporary used for the Variant Context tester..
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockListBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list-block-configuration': UmbPropertyEditorUIBlockListBlockConfigurationElement;
	}
}
