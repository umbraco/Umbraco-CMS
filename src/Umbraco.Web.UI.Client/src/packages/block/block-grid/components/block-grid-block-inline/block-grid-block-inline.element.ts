import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from '../../context/block-grid-entry.context-token.js';
import { UmbBlockGridInlinePropertyDatasetContext } from './block-grid-inline-property-dataset.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import '../block-grid-areas-container/index.js';
import '../ref-grid-block/index.js';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-block-grid-block-inline
 */
@customElement('umb-block-grid-block-inline')
export class UmbBlockGridBlockInlineElement extends UmbLitElement {
	//
	@property({ attribute: false })
	label?: string;

	@property({ type: String, reflect: false })
	icon?: string;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@state()
	_inlineProperty: UmbPropertyTypeModel | undefined;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_GRID_ENTRY_CONTEXT, (context) => {
			new UmbBlockGridInlinePropertyDatasetContext(this, context);

			this.observe(
				context.firstPropertyType,
				(property) => {
					this._inlineProperty = property;
				},
				'inlineProperty',
			);
		});
	}

	override render() {
		return html`<umb-ref-grid-block standalone href=${this.config?.editContentPath ?? ''}>
			<umb-icon slot="icon" .name=${this.icon}></umb-icon>
			<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
			<umb-property-type-based-property
				.property=${this._inlineProperty}
				slot="areas"></umb-property-type-based-property>
			<umb-block-grid-areas-container slot="areas"></umb-block-grid-areas-container>
		</umb-ref-grid-block>`;
	}

	static override styles = [
		css`
			umb-block-grid-areas-container {
				margin-top: calc(var(--uui-size-2) + 1px);
			}
			umb-block-grid-areas-container::part(area) {
				margin: var(--uui-size-2);
			}
			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
			}
		`,
	];
}

export default UmbBlockGridBlockInlineElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-block-inline': UmbBlockGridBlockInlineElement;
	}
}
