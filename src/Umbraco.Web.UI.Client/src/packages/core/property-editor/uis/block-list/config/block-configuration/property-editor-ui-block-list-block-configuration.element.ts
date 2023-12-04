import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from 'src/packages/core/data-type/workspace/data-type-workspace.context';

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

	private _workspaceContext?: typeof UMB_DATA_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_VARIANT_CONTEXT, async (variantContext) => {
			console.log('variantContext', variantContext);
			this.observe(await variantContext.propertyValueByAlias('validationLimit'), (x) =>
				console.log('variant_ validationLimit', x),
			);
		});
		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, async (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			console.log('workspaceContext', workspaceContext);
			this.observe(await workspaceContext.propertyValueByAlias('validationLimit'), (x) =>
				console.log('workspace_ validationLimit', x),
			);
		});
	}

	render() {
		return html`
			<div>umb-property-editor-ui-block-list-block-configuration</div>

			Temporary used for the Variant Context tester..

			<button
				@click=${() => {
					this._workspaceContext?.setPropertyValue('validationLimit', { min: 123, max: 456 });
				}}>
				hello
			</button>
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
