import type { UmbBlockTypeWithGroupKey, UmbInputBlockTypeElement } from '../../../block-type/index.js';
import '../../../block-type/components/input-block-type/index.js';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_PROPERTY_DATASET_CONTEXT, UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { BlockGridGroupConfigration } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-property-editor-ui-block-grid-type-configuration
 */
@customElement('umb-property-editor-ui-block-grid-type-configuration')
export class UmbPropertyEditorUIBlockGridTypeConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#context?: UmbPropertyDatasetContext;

	@property({ attribute: false })
	value: UmbBlockTypeWithGroupKey[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _blockGroups: Array<BlockGridGroupConfigration> = [];

	//	@state()
	//	private _groups: Array<{ name: string; key: string | null; blocks: UmbBlockTypeWithGroupKey[] }> = [];

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			this.#context = instance;
			this.#observeProperties();
		});
	}

	async #observeProperties() {
		if (!this.#context) return;

		this.observe(await this.#context.propertyValueByAlias('blockGroups'), (value) => {
			this._blockGroups = value as Array<BlockGridGroupConfigration>;
		});
	}

	render() {
		return html`<umb-input-block-type
			entity-type="block-grid-type"
			.groups=${this._blockGroups}
			.value=${this.value}
			@change=${(e: Event) => (this.value = (e.target as UmbInputBlockTypeElement).value)}></umb-input-block-type>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-input {
				margin-top: var(--uui-size-6);
				margin-bottom: var(--uui-size-4);
			}

			uui-input:not(:hover, :focus) {
				border: 1px solid transparent;
			}
			uui-input:not(:hover, :focus) uui-button {
				opacity: 0;
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockGridTypeConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-type-configuration': UmbPropertyEditorUIBlockGridTypeConfigurationElement;
	}
}
