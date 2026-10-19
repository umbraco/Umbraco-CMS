import { UMB_BLOCK_ENTRY_CONTEXT } from '../../context/block-entry.context-token.js';
import { getBlockStateTagConfig } from './get-block-state-tag-config.function.js';
import type { UmbBlockStateTagConfig } from './get-block-state-tag-config.function.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';

/**
 * @element umb-block-state-tag
 * @description Renders the publish state of the surrounding Block (Draft, Published with pending changes, Trashed) as a tag. Renders nothing when the Block needs no tag, or when it is not rendered inside a Block entry.
 */
@customElement('umb-block-state-tag')
export class UmbBlockStateTagElement extends UmbLitElement {
	@state()
	private _config?: UmbBlockStateTagConfig;

	constructor() {
		super();
		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.observe(
				context
					? mergeObservables(
							[context.isExposed, context.isExternalContent, context.externalContentVariantState],
							([isExposed, isExternalContent, variantState]) =>
								getBlockStateTagConfig({ unpublished: isExposed === false, isExternalContent, variantState }),
						)
					: undefined,
				(config) => (this._config = config),
				'observeStateTagConfig',
			);
		});
	}

	override render() {
		if (!this._config) return nothing;

		return html`
			<uui-tag color=${this._config.color} look="secondary" title=${this.localize.term(this._config.titleKey)}>
				<umb-localize key=${this._config.labelKey}></umb-localize>
			</uui-tag>
		`;
	}

	static override styles = [
		css`
			:host {
				display: inline-flex;
				vertical-align: text-top;
			}

			uui-tag {
				margin-left: 0.5em;
				margin-bottom: -0.3em;
				margin-top: -0.3em;
			}
		`,
	];
}

export default UmbBlockStateTagElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-state-tag': UmbBlockStateTagElement;
	}
}
